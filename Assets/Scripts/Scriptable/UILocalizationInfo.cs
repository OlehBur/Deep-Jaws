using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "UILocalizationInfo", menuName = "Scriptable Objects/UILocalizationInfo")]
public class UILocalizationInfo : ScriptableObject {
    [field: SerializeField] public LocalizedString Gamemode_EasyDesc { get; private set; }
    [field: SerializeField] public LocalizedString Gamemode_MediumDesc { get; private set; }
    [field: SerializeField] public LocalizedString Gamemode_HardDesc { get; private set; }
    [field: SerializeField] public LocalizedString SelectedTxt { get; private set; }
    [field: SerializeField] public LocalizedString SelectTxt { get; private set; }
    [field: SerializeField] public LocalizedString KingOfDeeps { get; private set; }

}
