namespace Mods.WebUI.Scripts;

internal class IndexGameHandler {
  public IndexGameHandler(WebUIServer webUIServer, StaticAssetsHandler staticAssetsHandler) {
    webUIServer.MapGet("/", staticAssetsHandler.HandleRequest, new() {
        { "path_info", "index.html"},
      });
  }
}
