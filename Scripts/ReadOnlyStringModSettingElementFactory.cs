using Timberborn.CoreUI;
using ModSettings.Core;
using ModSettings.CoreUI;
using ModSettings.CommonUI;
using UnityEngine.UIElements;

namespace Mods.WebUI.Scripts {

  internal class ReadOnlyStringModSettingElementFactory : IModSettingElementFactory {

    private readonly VisualElementLoader _visualElementLoader;
    private readonly ModSettingDescriptorInitializer _modSettingDescriptorInitializer;

    public ReadOnlyStringModSettingElementFactory(VisualElementLoader visualElementLoader,
                                                  ModSettingDescriptorInitializer modSettingDescriptorInitializer) {
      _visualElementLoader = visualElementLoader;
      _modSettingDescriptorInitializer = modSettingDescriptorInitializer;
    }

    public int Priority => 0;

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement element) {
      if (modSetting is ReadOnlyModSetting<string> stringModSetting) {
        var root = _visualElementLoader.LoadVisualElement("ModSettings/StringModSettingElement");
        _modSettingDescriptorInitializer.Initialize(root.Q<VisualElement>("Descriptor"),
                                                    stringModSetting);

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
