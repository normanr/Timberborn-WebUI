using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  internal class IndexGameHandler {
    public IndexGameHandler(WebUIServer webUIServer, StaticAssetsHandler staticAssetsHandler) {
      webUIServer.MapGet("/", staticAssetsHandler.HandleRequest, new RouteValueDictionary() {
        { "path_info", "index.html"},
      });
    }
  }
}
