using System.Web.Routing;

namespace Mods.WebUI.Scripts {
  internal class IndexMenuHandler: StaticAssetsHandler {
    public IndexMenuHandler(WebUIServer webUIServer) : base(webUIServer) {
      webUIServer.MapGet("/", HandleRequest, new RouteValueDictionary() {
        { "path_info", "index-menu.html"},
      });
    }
  }
}
