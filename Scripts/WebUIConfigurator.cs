using Bindito.Core;

namespace Mods.WebUI.Scripts {
  [Context("Game")]
  internal class WebUIConfigurator : IConfigurator {

    public void Configure(IContainerDefinition containerDefinition) {
      containerDefinition.Bind<MainThread>().AsSingleton();
      containerDefinition.Bind<WebUIServer>().AsSingleton();
    }

  }
}
