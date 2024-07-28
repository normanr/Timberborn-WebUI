using Bindito.Core;
using ModSettings.CoreUI;

namespace Mods.WebUI.Scripts {
  [Context("Game")]
  internal class WebUIConfiguratorForGame : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<MainThread>().AsSingleton();
      containerDefinition.Bind<WebUISettings>().AsSingleton();
      containerDefinition.Bind<WebUIInGameServer>().AsSingleton();
      containerDefinition.Bind<CharacterInformation>().AsSingleton();
    }

  }

  [Context("MainMenu")]
  internal class WebUIConfiguratorForMainMenu : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<MainThread>().AsSingleton();
      containerDefinition.Bind<WebUISettings>().AsSingleton();
      containerDefinition.MultiBind<IModSettingElementFactory>()
          .To<ReadOnlyStringModSettingElementFactory>()
          .AsSingleton();
      containerDefinition.Bind<WebUIServer>().AsSingleton();
    }

  }
}
