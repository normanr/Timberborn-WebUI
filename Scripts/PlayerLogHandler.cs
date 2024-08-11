using System.IO;
using System.Web.Routing;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  internal class PlayerLogHandler {
    private readonly WebUISettings _webUISettings;

    public PlayerLogHandler(WebUIServer webUIServer, WebUISettings webUISettings) {
      _webUISettings = webUISettings;
      webUIServer.MapGet("/player.log", HandleRequest);
    }

    string HandleRequest(RequestContext requestContext) {
      var httpContext = requestContext.HttpContext;
      if (!_webUISettings.AllowPlayerLog.Value) {
        Debug.Log("404 Not Found: " + httpContext.Request.Url);
        httpContext.Response.StatusCode = 404;
        return null;
      }
      var forwardedFor = httpContext.Request.Headers.Get("X-Forwarded-For");
      if (forwardedFor != null) {
        Debug.Log("403 Forbidden: " + httpContext.Request.Url);
        httpContext.Response.StatusCode = 403;
        return null;
      }
      httpContext.Response.ContentType = "text/plain";
      string filename = Application.persistentDataPath + "/Player.log";
      var info = new FileInfo(filename);
      httpContext.Response.AppendHeader("Content-Length", info.Length.ToString());
      // WriteFile opens the file with FileShare.Read, so it can't open
      // files already open for writing (like Unity's Player.log).
      httpContext.Response.TransmitFile(filename, 0, info.Length);
      return null;
    }

  }
}
