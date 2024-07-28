using ModSettings.Core;

namespace Mods.WebUI.Scripts {
  public class ReadOnlyModSetting<T> : ModSetting<T> {
    public ReadOnlyModSetting(T defaultValue, ModSettingDescriptor descriptor) : base(defaultValue, descriptor) {
    }

    public override void Reset() {
    }
  }
}
