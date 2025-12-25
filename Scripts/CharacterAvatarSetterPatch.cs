using System;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Timberborn.BlueprintSystem;

namespace Mods.WebUI.Scripts {

  [HarmonyPatch]
  public static class CharacterAvatarSetterPatch {

    private static readonly MethodInfo _targetMethod = AccessTools.TypeByName("Bobingabout.CharacterCustomizer.CharacterAvatarSetter").Method("GetEntityAvatarAsset");

    public static bool Prepare() {
      return _targetMethod != null;
    }

    public static MethodBase TargetMethod() {
      Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "Web UI: Patching CharacterAvatarSetter.GetEntityAvatarAsset");
      return _targetMethod;
    }

    public static void Postfix(string avatarPath, Sprite __result) {
      __result = new AssetRef<Sprite>(avatarPath, new Lazy<Sprite>(() => __result)).Asset;
    }
  }
}