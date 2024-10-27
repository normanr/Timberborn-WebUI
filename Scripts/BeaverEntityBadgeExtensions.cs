using System;
using System.Reflection;
using Timberborn.BeaversUI;

namespace Mods.WebUI.Scripts {
  public static class BeaverEntityBadgeExtensions {
    private static readonly MethodInfo GetEntityAvatarPathMethod;

    static BeaverEntityBadgeExtensions() {
      GetEntityAvatarPathMethod = typeof(BeaverEntityBadge).GetMethod("GetEntityAvatarPath", BindingFlags.Instance | BindingFlags.NonPublic);
      if (GetEntityAvatarPathMethod == null) {
        throw new Exception($"GetEntityAvatarPath method "
                   + $"wasn't found in {typeof(BeaverEntityBadge).Name}");
      }
    }

    public static string GetEntityAvatarPath(this BeaverEntityBadge beaverEntityBadge) {
      return (string)GetEntityAvatarPathMethod.Invoke(beaverEntityBadge, null);
    }
  }
}
