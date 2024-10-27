using System;
using Timberborn.ModManagerScene;

namespace Mods.WebUI.Scripts {
  internal class WebUIStarter : IModStarter {
    public void StartMod() => throw new NotImplementedException();

    public void StartMod(IModEnvironment modEnvironment) {
      WebUIServer.RootPath = modEnvironment.ModPath;
    }
  }
}
