using Bindito.Core;
using ModSettings.CoreUI;

namespace Mods.WebUI.Scripts {
  internal class WebUIConfiguratorBase : IConfigurator {
    public virtual void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<MainThread>().AsSingleton();
      containerDefinition.Bind<WebUISettings>().AsSingleton();
      containerDefinition.Bind<WebUIServer>().AsSingleton();
      containerDefinition.Bind<StaticAssetsHandler>().AsSingleton();
      containerDefinition.Bind<PlayerLogHandler>().AsSingleton();
    }
  }

  [Context("Game")]
  internal class WebUIConfiguratorForGame : WebUIConfiguratorBase {
    public override void Configure(IContainerDefinition containerDefinition) {
      base.Configure(containerDefinition);
      containerDefinition.Bind<IndexGameHandler>().AsSingleton();
      containerDefinition.Bind<CharacterInformation>().AsSingleton();
    }
  }

  [Context("MainMenu")]
  internal class WebUIConfiguratorForMainMenu : WebUIConfiguratorBase {

    public override void Configure(IContainerDefinition containerDefinition) {
      base.Configure(containerDefinition);
      containerDefinition.MultiBind<IModSettingElementFactory>()
          .To<ReadOnlyStringModSettingElementFactory>()
          .AsSingleton();
      containerDefinition.Bind<IndexMenuHandler>().AsSingleton();
    }
  }
}
