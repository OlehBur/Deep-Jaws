using UnityEngine;

[CreateAssetMenu(fileName = "FishInfoManager", menuName = "Scriptable Objects/FishInfoManager")]
public class FishInfoManager : ScriptableObject
{
    [SerializeField] FishInfo dartfish;
    [SerializeField] FishInfo crab;
    [SerializeField] FishInfo seabass;
    [SerializeField] FishInfo mackerel;
    [SerializeField] FishInfo moray;
    [SerializeField] FishInfo lionfish;
    [SerializeField] FishInfo barracuda;
    [SerializeField] FishInfo tuna;
    [SerializeField] FishInfo hammer;
    [SerializeField] FishInfo tiger;
    [SerializeField] FishInfo white;
    [SerializeField] FishInfo cuttle;
    [SerializeField] FishInfo puffer;

    [SerializeField] Sprite dartfish_img;
    [SerializeField] Sprite crab_img;
    [SerializeField] Sprite seabass_img;
    [SerializeField] Sprite mackerel_img;
    [SerializeField] Sprite moray_img;
    [SerializeField] Sprite lionfish_img;
    [SerializeField] Sprite barracuda_img;
    [SerializeField] Sprite tuna_img;
    [SerializeField] Sprite hammer_img;
    [SerializeField] Sprite tiger_img;
    [SerializeField] Sprite white_img;
    [SerializeField] Sprite cuttle_img;
    [SerializeField] Sprite puffer_img;

    public FishInfo GetFishInfo(FishLevel fishType)
    {
        return fishType switch
        {
            FishLevel.Dartfish => dartfish,
            FishLevel.Crab => crab,
            FishLevel.SeaBass => seabass,
            FishLevel.Mackerel => mackerel,
            FishLevel.Moray => moray,
            FishLevel.Lionfish => lionfish,
            FishLevel.Barracuda => barracuda,
            FishLevel.Tuna => tuna,
            FishLevel.HammerShark => hammer,
            FishLevel.TigerShark => tiger,
            FishLevel.WhiteShark => white,
            FishLevel.Cuttlefish => cuttle,
            FishLevel.Pufferfish => puffer,
            _ => null
        };
    }

    public Sprite GetFishSprite(FishLevel fishType)
    {
        return fishType switch
        {
            FishLevel.Dartfish => dartfish_img,
            FishLevel.Crab => crab_img,
            FishLevel.SeaBass => seabass_img,
            FishLevel.Mackerel => mackerel_img,
            FishLevel.Moray => moray_img,
            FishLevel.Lionfish => lionfish_img,
            FishLevel.Barracuda => barracuda_img,
            FishLevel.Tuna => tuna_img,
            FishLevel.HammerShark => hammer_img,
            FishLevel.TigerShark => tiger_img,
            FishLevel.WhiteShark => white_img,
            FishLevel.Cuttlefish => cuttle_img,
            FishLevel.Pufferfish => puffer_img,
            _ => null
        };
    }
}
