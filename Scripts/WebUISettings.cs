using Timberborn.Modding;
using Timberborn.SettingsSystem;
using ModSettings.Core;

namespace Mods.WebUI.Scripts {
  internal class WebUISettings : ModSettingsOwner {

    public ModSetting<int> Port { get; } =
      new ModSetting<int>(8080, ModSettingDescriptor.CreateLocalized("WebUI.Port"));

    public ModSetting<string> Status { get; } = new ModSetting<string>(
      "", ModSettingDescriptor.CreateLocalized("WebUI.Status").SetEnableCondition(() => false));

    public ModSetting<bool> AllowPlayerLog { get; } =
      new ModSetting<bool>(false, ModSettingDescriptor.CreateLocalized("WebUI.AllowPlayerLog"));

    public WebUISettings(ISettings settings,
                         ModSettingsOwnerRegistry modSettingsOwnerRegistry,
                         ModRepository modRepository) : base(
      settings, modSettingsOwnerRegistry, modRepository) {
    }

    public override ModSettingsContext ChangeableOn => ModSettingsContext.MainMenu | ModSettingsContext.Game;

    protected override string ModId => "WebUI";

  }
}
