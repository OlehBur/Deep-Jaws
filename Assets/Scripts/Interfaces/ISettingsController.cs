using TMPro;
using UnityEngine.UI;



public interface ISettingsController {
    Toggle BloomTog { get; }
    Toggle BlurTog { get; }
    Toggle VignetteTog { get; }
    Toggle ShadowCastTog { get; }
    Toggle AutoHideUI_tog { get; }
    Slider ShadowDistSl { get; }
    Slider ShadowFixSl { get; }
    Slider GameResolutSl { get; }
    Slider masterSlider { get; }
    Slider musicSlider { get; }
    Slider sfxSlider { get; }
    TMP_Dropdown ShadowResolution { get; }
    TMP_Dropdown LanguageDropdown { get; }
    TMP_Dropdown FramerateDropdown { get; }

    void SaveAudioData();
}