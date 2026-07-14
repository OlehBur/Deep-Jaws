using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// клас для ініціалізації частинок серцебиття риб та управління компонентів що відповідають за їх відображення як камери та текстури рендеру
/// class for initializing fish heartbeat particles and managing components responsible for their display such as cameras and render textures
/// </summary>
public class FishHeartbeatsManager : MonoBehaviour {
    [SerializeField] ParticleSystem heartbeatPrefab;
    [SerializeField] Transform heartbeatParent;
    [SerializeField] RenderTexture heartbeatView;
    [SerializeField] RenderTexture defaultView;
    [Header("for otimization disabling")]
    [SerializeField] GameObject shaderCamerasParent;
    [SerializeField] Image uiShaderVisualize;

    private SpawnManager spawnManager;
    private ParticleSystem[] fishHeartbeatsParticlesPool;

    private readonly Color WEAK_ENEMY_COLOR = Color.green;
    private readonly Color SAME_ENEMY_COLOR = Color.yellow;
    private readonly Color UNBEATABLE_ENEMY_COLOR = Color.orange;
    private readonly Color STRONG_ENEMY_COLOR = Color.red;
    private readonly Color UNKNOWN_ENEMY_COLOR = Color.blue;
    private readonly Color PLAYER_COLOR = new Color(.0f, .9f, 1f, 1f);

    private const int TEXTURE_SIZE_DIVIDER = 4; //в big texture resolution is expensive for mobile devices
    private const float HERTBEAT_PARTICLES_UPDATE_INTERVAL = 0.05f;


    public void Init(SpawnManager spawnManager) {
        this.spawnManager = spawnManager;
        heartbeatParent.gameObject.SetActive(false);
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
        if (fishHeartbeatsParticlesPool == null)
            fishHeartbeatsParticlesPool = new ParticleSystem[SpawnManager.POOL_SIZE + SpawnManager.PUFFERFISH_SPAWN_COUNT + SpawnManager.CUTTLEFISH_SPAWN_COUNT + spawnManager.PlayerPoolSize];
        FishLevel playerLevel = spawnManager.PlayerFish.Level;
        ParticleSystem.MainModule particleMainModule;

        int index = 0;
        //Debug.LogWarning("HeartBeatDisabled");
        foreach (Fish fish in SpawnManager.visible_poolFishComponents/*pool_FishComp*/)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);

        foreach (Fish fish in SpawnManager.pufferfishPool_FishComponents)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);

        foreach (Fish fish in SpawnManager.cuttlefishPool_FishComponents)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);

        foreach (Fish fish in SpawnManager.playerPool_FishComponents)
            InitParticleByFish(fish, ref index, ref particleMainModule, playerLevel);
    }

    void InitParticleByFish(Fish fish, ref int index, ref ParticleSystem.MainModule particleMainModule, FishLevel playerLevel) {
        Color enemyFishColor = UNKNOWN_ENEMY_COLOR;

        if (fishHeartbeatsParticlesPool[index] == null) {
            fishHeartbeatsParticlesPool[index] = Instantiate(heartbeatPrefab,
                fish.transform.position, Quaternion.identity, heartbeatParent);
            fishHeartbeatsParticlesPool[index].gameObject.SetActive(false);
        }
        else {
            //Debug.Log("heartbeat optimize reuse");
            fishHeartbeatsParticlesPool[index].transform.position = fish.transform.position;
        }

        if (fish.IsActive) {
            particleMainModule = fishHeartbeatsParticlesPool[index].main;
            enemyFishColor = (fish.Level <= GameStaticInfo.UNBEATABLE_FISH_LVL) ? UNBEATABLE_ENEMY_COLOR :
                (fish.Level == playerLevel) ? SAME_ENEMY_COLOR :
                (fish.Level < playerLevel) ? WEAK_ENEMY_COLOR :
                    STRONG_ENEMY_COLOR;

            enemyFishColor = (fish == spawnManager.PlayerFish) ?
                PLAYER_COLOR : enemyFishColor;

            particleMainModule.startColor = enemyFishColor;
            fishHeartbeatsParticlesPool[index].gameObject.SetActive(true);
        }
        else
            fishHeartbeatsParticlesPool[index].gameObject.SetActive(false);

        ++index;
    }

    public void SetActivateShaderPreparation(bool isActivate) {
        if (isActivate)
            InvokeRepeating(nameof(ReInitPool), 0f, HERTBEAT_PARTICLES_UPDATE_INTERVAL);
        else
            CancelInvoke(nameof(ReInitPool));

        heartbeatParent.gameObject.SetActive(isActivate);
        shaderCamerasParent.SetActive(isActivate);
        uiShaderVisualize.enabled = isActivate;

        RemoveEntityFromCamera(isActivate);
    }

    void RemoveEntityFromCamera(bool isRemove) {
        int layer;
        if (isRemove) {
            layer = LayerMask.NameToLayer("Entity");//subtract layer
            Camera.main.cullingMask &= ~(1 << layer);
        }
        else {
            layer = LayerMask.NameToLayer("Entity");//add layer
            Camera.main.cullingMask |= (1 << layer);
        }
    }
}
