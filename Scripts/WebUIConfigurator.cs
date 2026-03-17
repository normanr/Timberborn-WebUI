using Bindito.Core;

namespace Mods.WebUI.Scripts;

[Context("MainMenu")]
[Context("Game")]
internal class WebUIConfiguratorCommon : Configurator {
  protected override void Configure() {
    Bind<WebUISettings>().AsSingleton();
    Bind<WebUIServer>().AsSingleton();
    Bind<TextureHandler>().AsSingleton();
    Bind<StaticAssetsHandler>().AsSingleton();
    Bind<PlayerLogHandler>().AsSingleton();
  }
}

[Context("Game")]
internal class WebUIConfiguratorForGame : Configurator {
  protected override void Configure() {
    Bind<IndexGameHandler>().AsSingleton();
    Bind<CharacterInformation>().AsSingleton();
    Bind<GoodsInformation>().AsSingleton();
  }
}

[Context("MainMenu")]
internal class WebUIConfiguratorForMainMenu : Configurator {
  protected override void Configure() {
    Bind<IndexMenuHandler>().AsSingleton();
  }
}
