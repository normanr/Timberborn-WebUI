using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Timberborn.AssetSystem;

namespace Mods.WebUI.Scripts {
  [HarmonyPatch(typeof(AssetLoader))]
  public static class AssetLoaderPatch {

    private static ConditionalWeakTable<UnityEngine.Object, string> _assetPaths = new ConditionalWeakTable<UnityEngine.Object, string>();

    [HarmonyPatch(nameof(AssetLoader.Load))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(Type) })]
    public static void Postfix(UnityEngine.Object __result, string path) {
      _assetPaths.GetValue(__result, (_) => path);
    }

    public static string GetAssetLoaderPath(this UnityEngine.Object asset) {
      _assetPaths.TryGetValue(asset, out string result);
      return result;
    }
  }
}
