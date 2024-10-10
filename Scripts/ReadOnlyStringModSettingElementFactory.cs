using Timberborn.CoreUI;
using Timberborn.Localization;
using ModSettings.Core;
using ModSettings.CoreUI;
using ModSettings.CommonUI;
using UnityEngine.UIElements;

namespace Mods.WebUI.Scripts {

  internal class ReadOnlyStringModSettingElementFactory : IModSettingElementFactory {

    private readonly VisualElementLoader _visualElementLoader;
    private readonly ILoc _loc;

    public ReadOnlyStringModSettingElementFactory(VisualElementLoader visualElementLoader,
                                                  ILoc loc) {
      _visualElementLoader = visualElementLoader;
      _loc = loc;
    }

    public int Priority => 1;

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement element) {
      if (modSetting is ReadOnlyModSetting<string> stringModSetting) {
        var root = _visualElementLoader.LoadVisualElement("ModSettings/StringModSettingElement");

        // Inline-Start: _modSettingDescriptorInitializer.Initialize(root.Q<VisualElement>("Descriptor"), stringModSetting);
        var descriptor = root.Q<VisualElement>("Descriptor");
        descriptor.Q<Label>("SettingLabel").text =
                  modSetting.Descriptor.Name ?? _loc.T(modSetting.Descriptor.NameLocKey);
        var tooltipElement = descriptor.Q<VisualElement>("SettingTooltip");
        tooltipElement.ToggleDisplayStyle(false);
        // Inline-End

        var textField = root.Q<TextField>();
        textField.value = stringModSetting.Value;

        // Added-Start
        stringModSetting.ValueChanged += (sender, e) => {
          textField.value = stringModSetting.Value;
        };
        // Added-End

        element = new TextInputBaseFieldModSettingElement<string>(root, modSetting, textField);
        return true;
      }
      element = null;
      return false;
    }

  }
}
