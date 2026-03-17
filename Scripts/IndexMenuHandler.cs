namespace Mods.WebUI.Scripts;

internal class IndexMenuHandler {
  public IndexMenuHandler(WebUIServer webUIServer, StaticAssetsHandler staticAssetsHandler) {
    webUIServer.MapGet("/", staticAssetsHandler.HandleRequest, new() {
        { "path_info", "index-menu.html"},
      });
  }
}
