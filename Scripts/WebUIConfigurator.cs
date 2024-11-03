using Bindito.Core;
using ModSettings.CoreUI;

namespace Mods.WebUI.Scripts {
  [Context("MainMenu")]
  [Context("Game")]
  internal class WebUIConfiguratorCommon : IConfigurator {
    public virtual void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<MainThread>().AsSingleton();
      containerDefinition.Bind<WebUISettings>().AsSingleton();
      containerDefinition.Bind<WebUIServer>().AsSingleton();
      containerDefinition.Bind<TextureHandler>().AsSingleton();
      containerDefinition.Bind<StaticAssetsHandler>().AsSingleton();
      containerDefinition.Bind<PlayerLogHandler>().AsSingleton();
    }
  }

  [Context("Game")]
  internal class WebUIConfiguratorForGame : IConfigurator {
    public virtual void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<IndexGameHandler>().AsSingleton();
      containerDefinition.Bind<CharacterInformation>().AsSingleton();
    }
  }

  [Context("MainMenu")]
  internal class WebUIConfiguratorForMainMenu : IConfigurator {
    public virtual void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.MultiBind<IModSettingElementFactory>()
          .To<ReadOnlyStringModSettingElementFactory>()
          .AsSingleton();
      containerDefinition.Bind<IndexMenuHandler>().AsSingleton();
    }
  }
}
