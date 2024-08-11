using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  internal class IndexGameHandler : StaticAssetsHandler {
    public IndexGameHandler(WebUIServer webUIServer) : base(webUIServer) {
      webUIServer.MapGet("/", HandleRequest, new RouteValueDictionary() {
        { "path_info", "index.html"},
      });
    }
  }
}
