using UnityEngine;
using VContainer;
using VContainer.Unity;


public enum FishComponentType {
    ShieldBubble,
    PlayerSprintLine,
    PlayerHealParticle,
    WhirlLine,
    DeadBubblesParticle,
    BiteParticle,
    SkeletonParticle,
    InkParticles
}


public class VFXpoolManager : IInitializable/*MonoBehaviour*/ {
    //[SerializeField] Transform fishAddCompPoolParent;
    //[SerializeField] GameObject shieldBubblePrefab;
    //[SerializeField] GameObject spintLinePrefab;
    //[SerializeField] GameObject whirlLinePrefab;
    //[SerializeField] GameObject deadBubblesParticlPrefab;
    //[SerializeField] GameObject biteParticlePrefab;
    //[SerializeField] GameObject skeletParticlePrefab;
    //[SerializeField] GameObject healParticlePrefab;
    //[SerializeField] GameObject inkParticlesPrefab;
    [Inject] public VFXpoolManager(VFX_PrefabsInfo additionalComponentsInfo, Transform fishAddCompPoolParent) {
        _additionalComponentsInfo = additionalComponentsInfo;
        _fishAddCompPoolParent = fishAddCompPoolParent;
    }

    private IObjectResolver _resolver;
    private readonly VFX_PrefabsInfo _additionalComponentsInfo;
    private readonly Transform _fishAddCompPoolParent;
    private readonly IPlayerPoolService _playerSpawnService;
    private readonly IEnemyPoolService _enemyPoolService;

    private /*GameObject*/BubbleShieldWrapper[] _shieldsPool;
    private GameObject[] _sprintLinePool;
    private GameObject[] _whirlLinePool;
    private GameObject[] _deadBubblesPool;
    private GameObject[] _biteParticlePool;
    private GameObject[] _skeletParticlePool;
    private GameObject[] _healParticlePool;
    private GameObject[] _inkParticlesPool;


    public void Initialize() { 
        InitPool(ref _shieldsPool, _additionalComponentsInfo.ShieldBubblePrefab, GameStaticInfo.SHIELDS_POOL_CNT);
        InitPool(ref _whirlLinePool, _additionalComponentsInfo.WhirlLinePrefab, GameStaticInfo.WHIRLE_POOL_CNT);
        InitPool(ref _deadBubblesPool, _additionalComponentsInfo.DeadBubblesParticlPrefab, GameStaticInfo.DEAD_BUBBLES_POOL_CNT);
        InitPool(ref _biteParticlePool, _additionalComponentsInfo.BiteParticlePrefab, GameStaticInfo.BITE_PARTICLES_POOL_CNT);
        InitPool(ref _skeletParticlePool, _additionalComponentsInfo.SkeletParticlePrefab, GameStaticInfo.SKELETON_PARTICLE_POOL_CNT);
        InitPool(ref _inkParticlesPool, _additionalComponentsInfo.InkParticlesPrefab, GameStaticInfo.INK_PARTICLES_POOL_CNT);
        InitPool(ref _sprintLinePool, _additionalComponentsInfo.SpintLinePrefab, GameStaticInfo.SPRINT_PARTICLES_POOL_MAX_CNT);//for player only and in case respwn
        InitPool(ref _healParticlePool, _additionalComponentsInfo.HealParticlePrefab, GameStaticInfo.HEAL_PARTICLES_POOL_MAX_CNT);//for player only
    }

    void InitPool(ref GameObject[] pool, GameObject prefab, int count) {
        pool = new GameObject[count];

        for (int i = 0; i < count; i++) {
            pool[i] = _resolver.Instantiate(prefab, _fishAddCompPoolParent);
            pool[i].SetActive(false);
        }
    }

    void InitPool(ref BubbleShieldWrapper[] pool, GameObject prefab, int count) {
        pool = new BubbleShieldWrapper[count];

        for (int i = 0; i < count; i++) {
            pool[i] = new();
            pool[i].obj = _resolver.Instantiate(prefab, _fishAddCompPoolParent);
            pool[i].obj.SetActive(false);
            pool[i].collider = pool[i].obj.GetComponent<CapsuleCollider>();
            pool[i].visualTransform = pool[i].obj.transform.GetChild(0);
        }
    }

    public GameObject ActivateObjectFromPool(FishComponentType type, Transform parent, Vector3 localPositon, Vector3 localScale, Quaternion locRotation) {
        GameObject objToActivate = null;

        switch (type) {
            //case FishComponentType.ShieldBubble:
            //GetInactiveObjectFromPool(shieldsPool, out objToActivate);
            //break;
            case FishComponentType.DeadBubblesParticle:
            GetInactiveObjectFromPool(_deadBubblesPool, out objToActivate);
            break;
            case FishComponentType.BiteParticle:
            GetInactiveObjectFromPool(_biteParticlePool, out objToActivate);
            break;
            case FishComponentType.SkeletonParticle:
            GetInactiveObjectFromPool(_skeletParticlePool, out objToActivate);
            break;
            case FishComponentType.PlayerSprintLine:
            GetInactiveObjectFromPool(_sprintLinePool, out objToActivate);
            break;
            case FishComponentType.PlayerHealParticle:
            GetInactiveObjectFromPool(_healParticlePool, out objToActivate);
            break;
            case FishComponentType.WhirlLine:
            GetInactiveObjectFromPool(_whirlLinePool, out objToActivate);
            break;
            case FishComponentType.InkParticles:
            GetInactiveObjectFromPool(_inkParticlesPool, out objToActivate);
            break;
            default:
            Debug.LogWarning("ActivateObjectFromPool don`n have type: " + type);
            break;
        }

        objToActivate.transform.SetParent(parent);
        objToActivate.transform.localPosition = localPositon;
        objToActivate.transform.localScale = localScale;
        objToActivate.transform.localRotation = locRotation;
        if (type == FishComponentType.BiteParticle)
            objToActivate.transform.SetParent(null, true);
        objToActivate.SetActive(true);

        return objToActivate;
    }

    public BubbleShieldWrapper ActivateShieldFromPool(Transform parent, Vector3 localPositon) {
        GetInactiveShieldFromPool(_shieldsPool, out BubbleShieldWrapper shield);

        shield.obj.transform.SetParent(parent);
        shield.obj.transform.localPosition = localPositon;
        shield.obj.transform.localScale = Vector3.one;
        shield.obj.transform.localRotation = Quaternion.identity;
        shield.obj.SetActive(true);

        return shield;
    }

    void GetInactiveObjectFromPool(GameObject[] pool, out GameObject obj) {
        obj = null;
        for (int i = 0; i < pool.Length; i++) {
            if (!pool[i].activeInHierarchy) {
                obj = pool[i];
                obj.SetActive(false);
                return;
            }
        }

        Debug.LogWarning("No inactive objects in pool of type: " + pool[0].ToString());
        pool[0].SetActive(false);
        obj = pool[0];
    }

    void GetInactiveShieldFromPool(BubbleShieldWrapper[] pool, out BubbleShieldWrapper obj) {
        obj = null;
        for (int i = 0; i < pool.Length; i++) {
            if (!pool[i].obj.activeInHierarchy) {
                obj = pool[i];
                return;
            }
        }

        Debug.LogWarning("No inactive objects in pool of type: " + pool[0].ToString());
        obj = pool[0];
    }

}
