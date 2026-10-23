using UnityEngine;

[CreateAssetMenu(fileName = "FishPrefabs", menuName = "Scriptable Objects/FishPrefabs")]
public class FishPrefabsInfo : ScriptableObject {
    [field: SerializeField] public GameObject Pufferfish { get; }
    [field: SerializeField] public GameObject Cuttlefish { get; }
    [field: SerializeField] public GameObject Dartfish { get; }
    [field: SerializeField] public GameObject Crab { get; }
    [field: SerializeField] public GameObject SeaBass { get; }
    [field: SerializeField] public GameObject Mackerel { get; }
    [field: SerializeField] public GameObject Moray { get; }
    [field: SerializeField] public GameObject Lionfish { get; }
    [field: SerializeField] public GameObject Barracuda { get; }
    [field: SerializeField] public GameObject Tuna { get; }
    [field: SerializeField] public GameObject HammerShark { get; }
    [field: SerializeField] public GameObject TigerShark { get; }
    [field: SerializeField] public GameObject WhiteShark { get; }

    //[field: SerializeField] public Fish PufferfishComp { get; }
    //[field: SerializeField] public Fish CuttlefishComp { get; }
    //[field: SerializeField] public Fish DartfishComp { get; }
    //[field: SerializeField] public Fish CrabComp { get; }
    //[field: SerializeField] public Fish SeaBassComp { get; }
    //[field: SerializeField] public Fish MackerelComp { get; }
    //[field: SerializeField] public Fish MorayComp { get; }
    //[field: SerializeField] public Fish LionfishComp { get; }
    //[field: SerializeField] public Fish BarracudaComp { get; }
    //[field: SerializeField] public Fish TunaComp { get; }
    //[field: SerializeField] public Fish HammerSharkComp { get; }
    //[field: SerializeField] public Fish TigerSharkComp { get; }
    //[field: SerializeField] public Fish WhiteSharkComp { get; }

    public GameObject GetFishPrefabByType(FishLevel type) {
        return type switch {
            FishLevel.Dartfish => Dartfish,
            FishLevel.Crab => Crab,
            FishLevel.SeaBass => SeaBass,
            FishLevel.Mackerel => Mackerel,
            FishLevel.Moray => Moray,
            FishLevel.Lionfish => Lionfish,
            FishLevel.Barracuda => Barracuda,
            FishLevel.Tuna => Tuna,
            FishLevel.HammerShark => HammerShark,
            FishLevel.TigerShark => TigerShark,
            FishLevel.WhiteShark => WhiteShark,
            FishLevel.Cuttlefish => Cuttlefish,
            FishLevel.Pufferfish => Pufferfish,
            _ => null
        };
    }

    //public Fish GetFishComponentByType(FishLevel type) {
    //    return type switch {
    //        FishLevel.Dartfish => DartfishComp,
    //        FishLevel.Crab => CrabComp,
    //        FishLevel.SeaBass => SeaBassComp,
    //        FishLevel.Mackerel => MackerelComp,
    //        FishLevel.Moray => MorayComp,
    //        FishLevel.Lionfish => LionfishComp,
    //        FishLevel.Barracuda => BarracudaComp,
    //        FishLevel.Tuna => TunaComp,
    //        FishLevel.HammerShark => HammerSharkComp,
    //        FishLevel.TigerShark => TigerSharkComp,
    //        FishLevel.WhiteShark => WhiteSharkComp,
    //        FishLevel.Cuttlefish => CuttlefishComp,
    //        FishLevel.Pufferfish => PufferfishComp,
    //        _ => null
    //    };
    //}
}
