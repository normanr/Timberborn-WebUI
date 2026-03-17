using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Routing;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  internal class PlayerLogHandler {
    private readonly WebUISettings _webUISettings;
    private readonly string _fileName;

    public PlayerLogHandler(WebUIServer webUIServer, WebUISettings webUISettings) {
      _webUISettings = webUISettings;
      webUIServer.MapGet("/player.log", HandleRequest);
      _fileName = Application.persistentDataPath + "/Player.log";
    }

    async Task<string> HandleRequest(RequestContext requestContext) {
      var httpContext = requestContext.HttpContext;
      if (!_webUISettings.AllowPlayerLog.Value) {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Web UI: 404 Not Found: " + httpContext.Request.Url);
        httpContext.Response.StatusCode = 404;
        return null;
      }
      var forwardedFor = httpContext.Request.Headers.Get("X-Forwarded-For");
      if (forwardedFor != null) {
        Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Web UI: 403 Forbidden: " + httpContext.Request.Url);
        httpContext.Response.StatusCode = 403;
        return null;
      }
      httpContext.Response.ContentType = "text/plain";
      var info = new FileInfo(_fileName);
      httpContext.Response.AppendHeader("Content-Length", info.Length.ToString());
      // WriteFile opens the file with FileShare.Read, so it can't open
      // files already open for writing (like Unity's Player.log).
      httpContext.Response.TransmitFile(_fileName, 0, info.Length);
      return null;
    }

  }
}
