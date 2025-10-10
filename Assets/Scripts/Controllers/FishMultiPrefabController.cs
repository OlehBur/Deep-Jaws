using UnityEngine;


[System.Serializable]
public class PrefabsGroup {
    //[Header("Prefabs")]
    public GameObject crab;
    public GameObject seaBass;
    public GameObject mackarel;
    public GameObject moray;
    public GameObject lionfish;
    public GameObject barracuda;
    public GameObject tuna;
    public GameObject hammerheadShark;
    public GameObject tigerShark;
    public GameObject whiteShark;

    public GameObject this[FishLevel lvl] {
        get => lvl switch {
            FishLevel.Crab => crab,
            FishLevel.SeaBass => seaBass,
            FishLevel.Mackarel => mackarel,
            FishLevel.Moray => moray,
            FishLevel.Lionfish => lionfish,
            FishLevel.Barracuda => barracuda,
            FishLevel.Tuna => tuna,
            FishLevel.HammerShark => hammerheadShark,
            FishLevel.TigerShark => tigerShark,
            FishLevel.WhiteShark => whiteShark,
            _ => null,
        };
    }
}

[System.Serializable]
public class AnimatorsGroup {
    public Animator crab = null;
    public Animator seaBass;
    public Animator mackarel;
    public Animator moray;
    public Animator lionfish;
    public Animator barracuda;
    public Animator tuna;
    public Animator hammerheadShark;
    public Animator tigerShark;
    public Animator whiteShark;

    public Animator this[FishLevel lvl] {
        get => lvl switch {
            FishLevel.Crab => crab,
            FishLevel.SeaBass => seaBass,
            FishLevel.Mackarel => mackarel,
            FishLevel.Moray => moray,
            FishLevel.Lionfish => lionfish,
            FishLevel.Barracuda => barracuda,
            FishLevel.Tuna => tuna,
            FishLevel.HammerShark => hammerheadShark,
            FishLevel.TigerShark => tigerShark,
            FishLevel.WhiteShark => whiteShark,
            _ => null,
        };
    }
}

public struct PrefabData {
    public Vector3 bubbleScale;
    public Vector3 biteParticlesPos;
    public Vector3 waterLinePos;
    public Vector3 flowBubblesPos;
    public Fish fish;
}

/// <summary>
/// Used as a single fish unit, which changes the model accordingly and adapts 
/// to each type of fish (prefab) to avoid constant instance\destroy.
/// </summary>
public class FishMultiPrefabController : MonoBehaviour {
    [SerializeField] PrefabsGroup prefabs;
    [Space]
    [SerializeField] AnimatorsGroup prefabAnimators;
    [Space]
    [Header("General Prefab Components")]
    [SerializeField] FishControl enemyControl;
    [SerializeField] PlayerControl playerControl;
    [SerializeField] FishAnimManager fishAnimManager;
    [SerializeField] EnemyDetector enemyDetector;
    /*[SerializeField]*/ [HideInInspector] public /*Fish fish*/PrefabData prefab { get; private set; }


    public void InitFish(FishLevel lvl) {

        switch (lvl) {
            case FishLevel.Crab:
                //fishAnimManager.Init(null);
                break;
            case FishLevel.SeaBass:
                //crabPrefab.GetComponent<Animator>();
                break;
            case FishLevel.Mackarel:
                break;
            case FishLevel.Moray:
                break;
            case FishLevel.Lionfish:
                break;
            case FishLevel.Barracuda:
                break;
            case FishLevel.Tuna:
                break;
            case FishLevel.HammerShark:
                break;
            case FishLevel.TigerShark:
                break;
            case FishLevel.WhiteShark:
                break;
        }
    }
}
