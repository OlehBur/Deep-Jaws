using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



/// <summary>
/// class for initializing fish heartbeat particles and managing components responsible for their display such as cameras and render textures
/// </summary>
public class FishHeartbeatsManager : MonoBehaviour {
    [SerializeField] ParticleSystem heartbeatPrefab;
    //[SerializeField] Transform heartbeatParent;
    //[field: SerializeField] public Transform HeartbeatParent {get;private set;}
    [SerializeField] RenderTexture heartbeatView;
    [SerializeField] RenderTexture defaultView;
    [Header("for otimization disabling")]
    [SerializeField] GameObject shaderCamerasParent;
    [SerializeField] Image uiShaderVisualize;

    //private SpawnManager spawnManager;
    private ParticleSystem[] _fishHeartbeatsParticlesPool;
    private Coroutine _heartBeatParticleCorutine = null;

    [Inject] private readonly IEnemyFishPoolService _enemyPoolService;
    [Inject] private readonly IPermamentFishPoolService _permamentFishPoolService;
    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    private readonly WaitForSecondsRealtime _waitForParticleUpd = new WaitForSecondsRealtime(HERTBEAT_PARTICLES_UPDATE_INTERVAL);

    private static readonly Color WEAK_ENEMY_COLOR = Color.green;
    private static readonly Color SAME_ENEMY_COLOR = Color.yellow;
    private static readonly Color UNBEATABLE_ENEMY_COLOR = Color.orange;
    private static readonly Color STRONG_ENEMY_COLOR = Color.red;
    private static readonly Color UNKNOWN_ENEMY_COLOR = Color.blue;
    private static readonly Color PLAYER_COLOR = new Color(.0f, .9f, 1f, 1f);

    private const int TEXTURE_SIZE_DIVIDER = 4; //â big texture resolution is expensive for mobile devices
    private const float HERTBEAT_PARTICLES_UPDATE_INTERVAL = 0.05f;



    public void Init(/*SpawnManager spawnManager*/) {
        //this.spawnManager = spawnManager;
        /*heartbeatParent.*/
        gameObject.SetActive(false);
        ReInitPool();
        InitRenderTextureSize(defaultView);
        InitRenderTextureSize(heartbeatView);
        SetActivateShaderPreparation(false);
    }

    void InitRenderTextureSize(RenderTexture texture) {
        if (texture != null) {
            texture.Release();               //release GPU-resources
            texture.width = Screen.width / TEXTURE_SIZE_DIVIDER;
            texture.height = Screen.height / TEXTURE_SIZE_DIVIDER;
            texture.Create();                // recreate texture
        }
    }

    void ReInitPool() {
        if (_fishHeartbeatsParticlesPool == null)
            _fishHeartbeatsParticlesPool = new ParticleSystem[_enemyPoolService.Count + _permamentFishPoolService.Count + _playerFishPoolService.Count];
        FishLevel playerLevel = _playerFishPoolService.PlayerCurrentFish.Level;
        ParticleSystem.MainModule particleMainModule;

        int index = 0;
        //Debug.LogWarning("HeartBeatDisabled");
        foreach (Fish fish in _enemyPoolService)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);

        foreach (Fish fish in _permamentFishPoolService)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);

        foreach (Fish fish in _playerFishPoolService)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);
    }

    void InitParticleByFish(Fish fish, ref int index, ref ParticleSystem.MainModule particleMainModule, FishLevel playerLevel) {
        Color enemyFishColor = UNKNOWN_ENEMY_COLOR;

        if (_fishHeartbeatsParticlesPool[index] == null) {
            _fishHeartbeatsParticlesPool[index] = Instantiate(heartbeatPrefab,
                fish.transform.position, Quaternion.identity, /*heartbeatParent*/transform);
            _fishHeartbeatsParticlesPool[index].gameObject.SetActive(false);
        }
        else {
            //Debug.Log("heartbeat optimize reuse");
            _fishHeartbeatsParticlesPool[index].transform.position = fish.transform.position;
        }

        if (fish.IsActive) {
            particleMainModule = _fishHeartbeatsParticlesPool[index].main;
            enemyFishColor = (fish.Level <= GameStaticInfo.UNBEATABLE_FISH_LVL) ? UNBEATABLE_ENEMY_COLOR :
                (fish.Level == playerLevel) ? SAME_ENEMY_COLOR :
                (fish.Level < playerLevel) ? WEAK_ENEMY_COLOR :
                    STRONG_ENEMY_COLOR;

            enemyFishColor = (fish == _playerFishPoolService.PlayerCurrentFish) ?
                PLAYER_COLOR : enemyFishColor;

            particleMainModule.startColor = enemyFishColor;
            _fishHeartbeatsParticlesPool[index].gameObject.SetActive(true);
        }
        else
            _fishHeartbeatsParticlesPool[index].gameObject.SetActive(false);

        ++index;
    }

    public void SetActivateShaderPreparation(bool isActivate) {
        if (isActivate && _heartBeatParticleCorutine == null) {
            gameObject.SetActive(true);
            _heartBeatParticleCorutine = StartCoroutine(HeartBeatParticles());
            //InvokeRepeating(nameof(ReInitPool), 0f, HERTBEAT_PARTICLES_UPDATE_INTERVAL);
        }
        else
            StopCoroutine(HeartBeatParticles());
        //CancelInvoke(nameof(ReInitPool));

        /*heartbeatParent.*/
        gameObject.SetActive(isActivate);
        shaderCamerasParent.SetActive(isActivate);
        uiShaderVisualize.enabled = isActivate;

        RemoveEntityFromCamera(isActivate);
    }

    IEnumerator HeartBeatParticles() {
        while (true) {
            ReInitPool();
            yield return _waitForParticleUpd;
        }
    }

    void RemoveEntityFromCamera(bool isRemove) {
        int layer = GameStaticInfo.GetEntityLayerID();

        if (isRemove)
            Camera.main.cullingMask &= ~(1 << layer);//subtract layer
        else
            Camera.main.cullingMask |= (1 << layer);//add layer
    }

}
