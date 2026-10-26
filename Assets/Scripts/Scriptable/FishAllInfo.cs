using UnityEngine;


[CreateAssetMenu(fileName = "FishAllInfo", menuName = "Scriptable Objects/FishAllInfo")]
public class FishAllInfo : ScriptableObject
{
    [field: SerializeField] public FishInfo Dartfish {get;private set;}
    [field: SerializeField] public FishInfo Crab {get;private set;}
    [field: SerializeField] public FishInfo Seabass {get;private set;}
    [field: SerializeField] public FishInfo Mackerel {get;private set;}
    [field: SerializeField] public FishInfo Moray {get;private set;}
    [field: SerializeField] public FishInfo Lionfish {get;private set;}
    [field: SerializeField] public FishInfo Barracuda {get;private set;}
    [field: SerializeField] public FishInfo Tuna {get;private set;}
    [field: SerializeField] public FishInfo Hammer {get;private set;}
    [field: SerializeField] public FishInfo Tiger {get;private set;}
    [field: SerializeField] public FishInfo White {get;private set;}
    [field: SerializeField] public FishInfo Cuttle {get;private set;}
    [field: SerializeField] public FishInfo Puffer { get; private set; }

    public FishInfo GetFishInfo(FishLevel fishType)
    {
        return fishType switch
        {
            FishLevel.Dartfish => Dartfish,
            FishLevel.Crab => Crab,
            FishLevel.SeaBass => Seabass,
            FishLevel.Mackerel => Mackerel,
            FishLevel.Moray => Moray,
            FishLevel.Lionfish => Lionfish,
            FishLevel.Barracuda => Barracuda,
            FishLevel.Tuna => Tuna,
            FishLevel.HammerShark => Hammer,
            FishLevel.TigerShark => Tiger,
            FishLevel.WhiteShark => White,
            FishLevel.Cuttlefish => Cuttle,
            FishLevel.Pufferfish => Puffer,
            _ => null
        };
    }
}
