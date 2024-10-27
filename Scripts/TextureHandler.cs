using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Routing;
using UnityEngine;
using Timberborn.AssetSystem;

namespace Mods.WebUI.Scripts {
  internal class TextureHandler {
    private readonly MainThread _mainThread;
    private readonly IAssetLoader _assetLoader;
    private readonly byte[] _urlSigningKey;
    private readonly ConcurrentDictionary<string, byte[]> _assetCache;

    public TextureHandler(WebUIServer webUIServer, MainThread mainThread, IAssetLoader assetLoader) {
      _mainThread = mainThread;
      _assetLoader = assetLoader;

      _urlSigningKey = new byte[16];
      new System.Random().NextBytes(_urlSigningKey);
      _assetCache = new ConcurrentDictionary<string, byte[]>();

      webUIServer.MapGet("/buildings/{*path_info}", HandleRequest, new RouteValueDictionary() {
        { "prefix", "buildings/"},
      });
      webUIServer.MapGet("/sprites/{*path_info}", HandleRequest, new RouteValueDictionary() {
        { "prefix", "sprites/"},
      });
      webUIServer.MapGet("/ui/images/{*path_info}", HandleRequest, new RouteValueDictionary() {
        { "prefix", "ui/images/"},
      });
    }

    public string HandleRequest(RequestContext requestContext) {
      var httpContext = requestContext.HttpContext;
      var request = httpContext.Request;
      var response = httpContext.Response;

      if (!SignatureIsValid(request.Url)) {
        Debug.Log("404 Not Found: " + request.Url);
        response.StatusCode = 404;
        return null;
      }

      try {
        if (!_assetCache.TryGetValue(request.Url.PathAndQuery, out byte[] data)) {
          data = _mainThread.Invoke(() => {
            return GetAssetData(request.Url);
          }).Result;
          _assetCache[request.Url.PathAndQuery] = data;
        }
        response.ContentType = MimeMapping.GetMimeMapping(Path.GetFileName(request.Url.AbsolutePath));
        response.AppendHeader("Cache-Control", "max-age=3600, public");
        response.AppendHeader("Content-Length", data.Length.ToString());
        response.OutputStream.Write(data, 0, data.Length);
        return null;
      } catch (Exception ex) {
        Debug.Log("500 Server Error: " + request.Url);
        Debug.LogError(ex);
        response.StatusCode = 500;
        return null;
      }
    }

    private byte[] GetAssetData(Uri url) {
      Debug.Log("WebUI: Caching " + url.PathAndQuery);
      var ext = Path.GetExtension(url.AbsolutePath);
      var qs = HttpUtility.ParseQueryString(url.Query);
      var texture = _assetLoader.Load<Texture2D>(url.AbsolutePath.TrimStart('/').Replace(ext, ""));
      var w = int.Parse(qs.Get("w") ?? "0");
      var h = int.Parse(qs.Get("h") ?? "0");
      if (!texture.isReadable || w > 0 || h > 0) {
        texture = texture.DuplicateAsReadable(w, h);
      }
      byte[] data;
      switch (ext.ToLower()) {
        case ".png":
          data = texture.EncodeToPNG();
          break;
        case ".jpg":
          data = texture.EncodeToJPG();
          break;
        default:
          throw new InvalidDataException("Unsupported extension: " + ext);
      }
      return data;
    }

    public string SignUrl(string path) {
      if (!path.StartsWith("/", StringComparison.Ordinal)) {
        path = "/" + path;
      }
      using (var hmac = new HMACSHA256(_urlSigningKey)) {
        var sig = HttpServerUtility.UrlTokenEncode(
              hmac.ComputeHash(Encoding.UTF8.GetBytes(path)));
        path += (path.Contains("?") ? "&" : "?") + "sig=" + Uri.EscapeDataString(sig);
        return path;
      }
    }

    private bool SignatureIsValid(Uri url) {
      try {
        var qs = HttpUtility.ParseQueryString(url.Query);
        var receivedSignature = HttpServerUtility.UrlTokenDecode(qs.Get("sig"));
        var ub = new UriBuilder(url);
        qs.Remove("sig");
        ub.Query = qs.ToString();
        var path = ub.Uri.PathAndQuery;

        using (var hmac = new HMACSHA256(_urlSigningKey)) {
          var computedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(path));
          var signaturesMatch = CryptographicOperations.FixedTimeEquals(computedSignature, receivedSignature);
          return signaturesMatch;
        }
      }
      catch (Exception) {
        return false;
      }
    }
  }
}
