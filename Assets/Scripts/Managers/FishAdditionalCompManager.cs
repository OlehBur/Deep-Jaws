using UnityEngine;


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


public class FishAdditionalCompManager : MonoBehaviour {
    [SerializeField] Transform fishAddCompPoolParent;
    [SerializeField] GameObject shieldBubblePrefab;
    [SerializeField] GameObject spintLinePrefab;
    [SerializeField] GameObject whirlLinePrefab;
    [SerializeField] GameObject deadBubblesParticlPrefab;
    [SerializeField] GameObject biteParticlePrefab;
    [SerializeField] GameObject skeletParticlePrefab;
    [SerializeField] GameObject healParticlePrefab;
    [SerializeField] GameObject inkParticlesPrefab;
    private const int HEAL_FRUIT_MAX_CNT = 3;

    private /*GameObject*/BubbleShieldWrapper[] shieldsPool;
    private GameObject[] sprintLinePool;
    private GameObject[] whirlLinePool;
    private GameObject[] deadBubblesPool;
    private GameObject[] biteParticlePool;
    private GameObject[] skeletParticlePool;
    private GameObject[] healParticlePool;
    private GameObject[] inkParticlesPool;


    public void Init(int shieldsCnt, int deadBubblesCnt, int biteParticlesCnt, int skeletonParticlesCnt, int whirlBubblesCnt, int inkParticlesCnt) {
        InitPool(ref shieldsPool, shieldBubblePrefab, shieldsCnt);
        InitPool(ref whirlLinePool, whirlLinePrefab, whirlBubblesCnt);
        InitPool(ref deadBubblesPool, deadBubblesParticlPrefab, deadBubblesCnt);
        InitPool(ref biteParticlePool, biteParticlePrefab, biteParticlesCnt);
        InitPool(ref skeletParticlePool, skeletParticlePrefab, skeletonParticlesCnt);
        InitPool(ref inkParticlesPool, inkParticlesPrefab, inkParticlesCnt);
        InitPool(ref sprintLinePool, spintLinePrefab, 2);//for player only and in case respwn
        InitPool(ref healParticlePool, healParticlePrefab, HEAL_FRUIT_MAX_CNT);//for player only
    }

    void InitPool(ref GameObject[] pool, GameObject prefab, int count) {
        pool = new GameObject[count];

        for (int i = 0; i < count; i++) {
            pool[i] = Instantiate(prefab, fishAddCompPoolParent);
            pool[i].SetActive(false);
        }
    }

    void InitPool(ref BubbleShieldWrapper[] pool, GameObject prefab, int count) {
        pool = new BubbleShieldWrapper[count];

        for (int i = 0; i < count; i++) {
            pool[i] = new();
            pool[i].obj = Instantiate(prefab, fishAddCompPoolParent);
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
            GetInactiveObjectFromPool(deadBubblesPool, out objToActivate);
            break;
            case FishComponentType.BiteParticle:
            GetInactiveObjectFromPool(biteParticlePool, out objToActivate);
            break;
            case FishComponentType.SkeletonParticle:
            GetInactiveObjectFromPool(skeletParticlePool, out objToActivate);
            break;
            case FishComponentType.PlayerSprintLine:
            GetInactiveObjectFromPool(sprintLinePool, out objToActivate);
            break;
            case FishComponentType.PlayerHealParticle:
            GetInactiveObjectFromPool(healParticlePool, out objToActivate);
            break;
            case FishComponentType.WhirlLine:
            GetInactiveObjectFromPool(whirlLinePool, out objToActivate);
            break;
            case FishComponentType.InkParticles:
            GetInactiveObjectFromPool(inkParticlesPool, out objToActivate);
            break;
            default:
            Debug.LogWarning("ActivateObjectFromPool don`n have type: " + type);
            break;
        }

        objToActivate.transform.SetParent(parent);
        objToActivate.transform.localPosition = localPositon;
        objToActivate.transform.localScale = localScale;
        objToActivate.transform.localRotation = locRotation;
        objToActivate.SetActive(true);

        return objToActivate;
    }

    public BubbleShieldWrapper ActivateShieldFromPool(Transform parent, Vector3 localPositon) {
        GetInactiveShieldFromPool(shieldsPool, out BubbleShieldWrapper shield);

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
