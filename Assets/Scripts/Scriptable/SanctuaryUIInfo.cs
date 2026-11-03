using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "SanctuaryUIInfo", menuName = "Scriptable Objects/SanctuaryUIInfo")]
public class SanctuaryUIInfo : ScriptableObject {
    [field: SerializeField] public LocalizedString ResearchNeutralize { get; private set; }
    [field: SerializeField] public LocalizedString ResearchProg { get; private set; }
    [field: SerializeField] public LocalizedString HourShort { get; private set; }
    [field: SerializeField] public LocalizedString MinuteShort { get; private set; }
    [field: SerializeField] public LocalizedString YesStr { get; private set; }
    [field: SerializeField] public LocalizedString NoStr { get; private set; }
    [field: SerializeField] public LocalizedString Units { get; private set; }
    [field: SerializeField] public LocalizedString MetersPerSec { get; private set; }
    [field: SerializeField] public LocalizedString Meters { get; private set; }
}
