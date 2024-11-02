using System;
using Timberborn.ModManagerScene;
using HarmonyLib;

namespace Mods.WebUI.Scripts {
  internal class WebUIStarter : IModStarter {
    public void StartMod() => throw new NotImplementedException();

    public void StartMod(IModEnvironment modEnvironment) {
      WebUIServer.RootPath = modEnvironment.ModPath;
      var harmony = new Harmony("Web UI");
      harmony.PatchAll();
    }
  }
}
