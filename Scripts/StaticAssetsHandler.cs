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
      HttpRequestBase request = httpContext.Request;
      var filename = request.MapPath(virtualPath);
      if (!File.Exists(filename)) {
        Debug.Log("404 Not Found: " + httpContext.Request.Url);
        httpContext.Response.StatusCode = 404;
        return null;
      }
      var info = new FileInfo(filename);
      httpContext.Response.ContentType = MimeMapping.GetMimeMapping(filename);
      httpContext.Response.AppendHeader("Content-Length", info.Length.ToString());
      httpContext.Response.TransmitFile(filename, 0, info.Length);
      return null;
    }
  }
}
