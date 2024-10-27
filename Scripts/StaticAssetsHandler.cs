using System.IO;
using System.Web;
using System.Web.Routing;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  public class StaticAssetsHandler {
    public StaticAssetsHandler(WebUIServer webUIServer) {
      webUIServer.MapGet("/assets/{*path_info}", HandleRequest);
      webUIServer.MapGet("/favicon.ico", HandleRequest, new RouteValueDictionary() {
        { "path_info", "favicon.ico"},
      });
    }

    public string HandleRequest(RequestContext requestContext) {
      var virtualPath = "Assets/" + (string)requestContext.RouteData.Values["path_info"];
      var httpContext = requestContext.HttpContext;
      var request = httpContext.Request;
      var response = httpContext.Response;
      var filename = request.MapPath(virtualPath);
      if (!File.Exists(filename)) {
        Debug.Log("404 Not Found: " + request.Url);
        response.StatusCode = 404;
        return null;
      }
      var info = new FileInfo(filename);
      response.ContentType = MimeMapping.GetMimeMapping(filename);
      response.AppendHeader("Cache-Control", "max-age=3600, public");
      response.AppendHeader("Content-Length", info.Length.ToString());
      response.TransmitFile(filename, 0, info.Length);
      return null;
    }
  }
}
