using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  internal class IndexMenuHandler {
    public IndexMenuHandler(WebUIServer webUIServer, StaticAssetsHandler staticAssetsHandler) {
      webUIServer.MapGet("/", staticAssetsHandler.HandleRequest, new RouteValueDictionary() {
        { "path_info", "index-menu.html"},
      });
    }
  }
}
