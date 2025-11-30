using System;
using System.Reflection;

using Timberborn.EntitySystem;

namespace Mods.WebUI.Scripts {
  internal static class CharacterBatchControlTab {
    private static readonly MethodInfo _getGroupingKey;
    private static readonly MethodInfo _getSortingKey;

    static CharacterBatchControlTab() {
      var characterBatchControlTab = Type.GetType("Timberborn.CharactersBatchControl.CharacterBatchControlTab, Timberborn.CharactersBatchControl");
      _getGroupingKey = characterBatchControlTab.GetMethod("GetGroupingKey", BindingFlags.NonPublic | BindingFlags.Static);
      _getSortingKey = characterBatchControlTab.GetMethod("GetSortingKey", BindingFlags.NonPublic | BindingFlags.Static);
    }

    internal static string GetGroupingKey(EntityComponent entityComponent) {
      return (string)_getGroupingKey.Invoke(null, [entityComponent]);
    }

    internal static string GetSortingKey(string locKey) {
      return (string)_getSortingKey.Invoke(null, [locKey]);
    }
  }
}
