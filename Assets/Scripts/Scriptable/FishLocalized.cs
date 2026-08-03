using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "FishLocalized", menuName = "Scriptable Objects/FishLocalized")]
public class FishLocalized : ScriptableObject
{
    [SerializeField] LocalizedString dartfish;
    [SerializeField] LocalizedString crab;
    [SerializeField] LocalizedString seabass;
    [SerializeField] LocalizedString mackarel;
    [SerializeField] LocalizedString moray;
    [SerializeField] LocalizedString lionfish;
    [SerializeField] LocalizedString barracuda;
    [SerializeField] LocalizedString tuna;
    [SerializeField] LocalizedString hammer;
    [SerializeField] LocalizedString tiger;
    [SerializeField] LocalizedString white;
    [SerializeField] LocalizedString puffer;
    [SerializeField] LocalizedString cuttle;

    public string GetFishLocalizeByType(FishLevel type) {
        return type switch {
            FishLevel.Dartfish => dartfish.GetLocalizedString(),
            FishLevel.Crab => crab.GetLocalizedString(),
            FishLevel.SeaBass => seabass.GetLocalizedString(),
            FishLevel.Mackerel => mackarel.GetLocalizedString(),
            FishLevel.Moray => moray.GetLocalizedString(),
            FishLevel.Lionfish => lionfish.GetLocalizedString(),
            FishLevel.Barracuda => barracuda.GetLocalizedString(),
            FishLevel.Tuna => tuna.GetLocalizedString(),
            FishLevel.HammerShark => hammer.GetLocalizedString(),
            FishLevel.TigerShark => tiger.GetLocalizedString(),
            FishLevel.WhiteShark => white.GetLocalizedString(),
            FishLevel.Cuttlefish => cuttle.GetLocalizedString(),
            FishLevel.Pufferfish => puffer.GetLocalizedString(),
            _ => "---"
        };
    }
}
