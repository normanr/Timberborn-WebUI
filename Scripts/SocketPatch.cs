using System.Net.Sockets;
using HarmonyLib;

namespace Mods.WebUI.Scripts {
  [HarmonyPatch(typeof(Socket))]
  public static class SocketPatch {
    [HarmonyPatch(nameof(Socket.Bind))]
    public static void Prefix(Socket __instance) {
      __instance.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    }
  }
}
