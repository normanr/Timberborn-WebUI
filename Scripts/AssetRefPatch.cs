using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Timberborn.BlueprintSystem;

namespace Mods.WebUI.Scripts {
  [HarmonyPatch(typeof(AssetRef<UnityEngine.Object>))]
  public static class AssetRefPatch {

    private static ConditionalWeakTable<UnityEngine.Object, string> _assetPaths = new ConditionalWeakTable<UnityEngine.Object, string>();

    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch([typeof(string), typeof(Lazy<UnityEngine.Object>)])]
    public static void Postfix(string path, ref Lazy<UnityEngine.Object> ____lazyAsset) {
      var lazyAsset = ____lazyAsset;
      ____lazyAsset = new Lazy<UnityEngine.Object>(() => {
        var value = lazyAsset.Value;
        _assetPaths.GetValue(value, (_) => path);
        return value;
      });
    }

    public static string GetAssetRefPath(this UnityEngine.Object asset) {
      _assetPaths.TryGetValue(asset, out string result);
      return result;
    }
  }
}
