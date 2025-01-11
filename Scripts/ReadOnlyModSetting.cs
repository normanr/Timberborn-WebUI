using System;
using ModSettings.Core;

namespace Mods.WebUI.Scripts {
  public class ReadOnlyModSetting<T> : NonPersistentSetting {
    public event EventHandler<T> ValueChanged;
    public T Value { get; private set; }

    public ReadOnlyModSetting(T defaultValue, ModSettingDescriptor descriptor) : base(descriptor) {
    }

    public virtual void SetValue(T value) {
      if (!value.Equals(Value)) {
        Value = value;
        ValueChanged?.Invoke(this, value);
      }
    }

    public override void Reset() {
    }
  }
}
