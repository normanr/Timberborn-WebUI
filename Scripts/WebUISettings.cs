using Timberborn.Modding;
using Timberborn.SettingsSystem;
using ModSettings.Core;

namespace Mods.WebUI.Scripts;

internal class WebUISettings(ISettings settings,
                     ModSettingsOwnerRegistry modSettingsOwnerRegistry,
                     ModRepository modRepository) : ModSettingsOwner(
  settings, modSettingsOwnerRegistry, modRepository) {

  public ModSetting<int> Port { get; } =
    new(8080, ModSettingDescriptor.CreateLocalized("WebUI.Port"));

  public ModSetting<string> Status { get; } =
    new("", ModSettingDescriptor.CreateLocalized("WebUI.Status").SetEnableCondition(() => false));

  public ModSetting<bool> AllowPlayerLog { get; } =
    new(false, ModSettingDescriptor.CreateLocalized("WebUI.AllowPlayerLog"));

  public override ModSettingsContext ChangeableOn => ModSettingsContext.MainMenu | ModSettingsContext.Game;

  protected override string ModId => "WebUI";

}
