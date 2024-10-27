using System;
using System.Reflection;
using Timberborn.EntitySystem;

namespace Mods.WebUI.Scripts {
  public static class LabeledEntityExtensions {

    private static readonly FieldInfo _labeledEntitySpecField;

    static LabeledEntityExtensions() {
      _labeledEntitySpecField = typeof(LabeledEntity).GetField("_labeledEntitySpec", BindingFlags.Instance | BindingFlags.NonPublic);
      if (_labeledEntitySpecField == null) {
        throw new Exception($"{nameof(LabeledEntitySpec)} field named _labeledEntitySpec "
                   + $"wasn't found in {typeof(LabeledEntity).Name}");
      }
    }

    public static LabeledEntitySpec GetLabeledEntitySpec(this LabeledEntity labeledEntity) {
      return (LabeledEntitySpec)_labeledEntitySpecField.GetValue(labeledEntity);
    }
  }
}
