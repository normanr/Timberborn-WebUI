using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using HarmonyLib;
using Timberborn.BlueprintSystem;

namespace Mods.WebUI.Scripts {
  [HarmonyPatch(typeof(AssetRef<UnityEngine.Object>))]
  public static class AssetRefPatch {

    private static readonly ConditionalWeakTable<UnityEngine.Object, string> _assetPaths = [];

    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch([typeof(string), typeof(Lazy<UnityEngine.Object>)])]
    public static void Postfix(string path, ref Lazy<UnityEngine.Object> ____lazyAsset) {
      var lazyAsset = ____lazyAsset;
      ____lazyAsset = new Lazy<UnityEngine.Object>(() => {
        var value = lazyAsset.Value;
        if (_assetPaths.GetValue(value, (_) => path) == null) {
          _assetPaths.AddOrUpdate(value, path);
        }
        return value;
      });
    }

    extension(UnityEngine.Object asset) {
      public string AssetRefPath {
        get {
          if (!_assetPaths.TryGetValue(asset, out string result)) {
            Debug.LogWarning(DateTime.Now.ToString("HH:mm:ss ") + $"Web UI: Missing AssetRef for {asset.name}");
            _assetPaths.GetValue(asset, (_) => null);
          }
          return result;
        }
      }
    }
  }
}
