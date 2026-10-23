using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public interface IPlayerPoolService {
    Fish PlayerCurrentFish { get; }
    event Action<Fish, bool> OnPlayerRespawn;
    event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void InitPool(Transform fishPoolParent, GameObject enemyDetectorPrefab);
}

public class PlayerPoolService : IPlayerPoolService {
    private GameObject[] _playerPool;
    private Fish[] _playerPoolComp;
    //private ObjectPool<GameObject> _playerPool;
    //private ObjectPool<Fish> _playerPoolComp;
    private Vector3 _playerPos;
    private Quaternion _playerRot;
    private float _playerProg;
    private WorldData _savedWorldData;
    private Vector3 _playerInitSpawnPos;
    private IObjectResolver _resolver;
    private Transform _fishPoolParent;
    private EnemyDetector _enemyDetector;

    private readonly FishPrefabsInfo _fishPrefabs;
    private readonly AudioManager _audioManager;
    private readonly VFXpoolManager _fishAdditionalCompManager;
    private readonly FishSkinManager _fishSkinManager;

    IObjectResolver resolver;

    public event Action<Fish, bool> OnPlayerRespawn;//bool - is respawn after death
    public event Action OnPlayerDied;
    public event Action<FishLevel> OnPlayerLVLup;

    public Fish PlayerCurrentFish { get; private set; }

    [Inject]
    public PlayerPoolService(FishPrefabsInfo fishPrefabs, AudioManager audioManager,
        VFXpoolManager fishAdditionalCompManager, FishSkinManager fishSkinManager) {
        _savedWorldData = ProgressSaver.LoadWorldData();// ProgressSaver.IsWorldDataExist() ?             ProgressSaver.LoadWorldData() : new WorldData();//load save data if exists
        _fishPrefabs = fishPrefabs;
        _audioManager = audioManager;
        _fishAdditionalCompManager = fishAdditionalCompManager;
        _fishSkinManager = fishSkinManager;

        _playerInitSpawnPos = Vector3.up * GameStaticInfo.FISH_SPAWN_HEINGT;//in center map
        //InitPlayerPool(fishPrefabs, fishPoolParent, audioManager, fishAdditionalCompManager, fishSkinManager);
    }

    public void InitPool(Transform fishPoolParent, GameObject enemyDetectorPrefab) {
        _fishPoolParent = fishPoolParent;
        _playerPool = new GameObject[GameStaticInfo.PLAYER_POOL_SIZE];
        _playerPoolComp = new Fish[GameStaticInfo.PLAYER_POOL_SIZE];
        _enemyDetector = resolver.Instantiate(enemyDetectorPrefab, fishPoolParent).GetComponent<EnemyDetector>();

        for (int playerInd = 0, prefabInd = 1; playerInd < GameStaticInfo.PLAYER_POOL_SIZE; playerInd++, prefabInd++) {

            GameObject playerPrefab = resolver.Instantiate(_fishPrefabs.GetFishPrefabByType((FishLevel)prefabInd), fishPoolParent);
            //FishControl fishControl = playerPrefab.GetComponent<FishControl>();
            Fish newPlayerFish = playerPrefab.GetComponent<Fish>();// fishControl.Fish;
            newPlayerFish.IsPlayer = true;
            newPlayerFish.OnFishDie += FishDieAct;
            newPlayerFish.OnFishLvlUp += PlayerLvlUpAct;
            //fishControl.Init(this/*, newPlayerFish*/);

            _playerPool[playerInd] = playerPrefab;
            _playerPoolComp[playerInd] = newPlayerFish;

            PrefabNamingTaging(playerPrefab, newPlayerFish.Level);
            if (playerPrefab.TryGetComponent(out FishAnim fishAnim))
                fishAnim.Init();
            //PrefabInitFishAnimAndEffectManagers(playerPrefab, _audioManager, _fishAdditionalCompManager, out FishAnim fishAnim);
            InitPlayerSkin(newPlayerFish, fishAnim, _fishSkinManager);
            EnableAutoScalerComponent(playerPrefab);

            playerPrefab.SetActive(false);
        }
    }

    void PrefabNamingTaging(GameObject prefab, FishLevel level) {
        prefab.name = GameStaticInfo.NAME_PLAYER + level.ToString();
        prefab.tag = GameStaticInfo.TAG_PLAYER;

        foreach (Transform t in prefab.transform) //for bubble collider
            t.gameObject.tag = prefab.tag;
    }

    //void PrefabInitFishAnimAndEffectManagers(GameObject prefab, AudioManager audioManager, FishAdditionalCompManager fishAdditionalComponentManager, out FishAnim fishAnim) {
    //    if (prefab.TryGetComponent(out fishAnim) /*&& prefab.TryGetComponent(out EffectManager effectManager)*/) {
    //        fishAnim.Init( /*effectManager*/);
    //        //effectManager.Init();
    //    }
    //}

    void FishDieAct(GameObject gameObject, FishLevel level) =>
        RespawnPrefab(level, true);

    void PlayerLvlUpAct(GameObject gameObject, FishLevel level) =>
            RespawnPrefab(level, false);//true - only plyer event



    private void RespawnPrefab(FishLevel level, bool isDie = true) {

        //wait for retry\revive buttons touched & ad watch
        SaveDataForDelayPlayerRespawn();
        if (isDie)
            OnPlayerDied?.Invoke();
        else {
            OnPlayerLVLup?.Invoke(level);
            RespawnPlayer(false); // lvl up
        }
    }

    void InitPlayerSkin(Fish fish, FishAnim fishAnim, FishSkinManager fishSkinManager) {
        //load selected skin
        Skin selectedSkin;
        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        if (currentData != null && currentData.selectedSkin != Skin.Default)
            selectedSkin = currentData.selectedSkin;
        else
            return;

        Renderer renderer = fishAnim.FishSkinnedMesh;
        if (fishSkinManager != null)
            fishSkinManager.InitSkin(renderer, selectedSkin, fish.Level);
    }

    void EnableAutoScalerComponent(GameObject toPrefab) {
        AutoScalerToDef autoScaler = toPrefab.GetComponent<AutoScalerToDef>();
        if (autoScaler == null) {
            Debug.LogWarning("prefab havent AutoScaler component: " + toPrefab.name);
            toPrefab.AddComponent<AutoScalerToDef>().enabled = true;
        }
        else {
            autoScaler.enabled = true;
            return;
        }
    }

    public void SavePlayerData() {
        if (PlayerCurrentFish == null)
            return;

        WorldData worldPlayerData = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        worldPlayerData.playerPosition = PlayerCurrentFish.Rb.position;
        worldPlayerData.playerRotation = PlayerCurrentFish.VisualRotation;
        worldPlayerData.playerHealth = PlayerCurrentFish.Health;
        worldPlayerData.playerLevel = PlayerCurrentFish.Level;
        worldPlayerData.playerProgress = PlayerCurrentFish.Progress;
        worldPlayerData.gameMode = GameStaticInfo.Gamemode;

        ProgressSaver.SaveData(worldPlayerData);
    }

    void SaveDataForDelayPlayerRespawn() {
        _playerPos = PlayerCurrentFish.transform.position;
        _playerRot = PlayerCurrentFish.transform.rotation;
        _playerProg = PlayerCurrentFish.Progress;
    }


    Fish SpawnPlayerFish(Vector3 coords, Quaternion rotation, bool isLoadFromSave, int playerLvl = (int)GameStaticInfo.PLAYER_INIT_LVL, float playerProgress = 0f, bool isDie = false) {
        //playerLvl = (int)FishLevel.Mackerel;
        //playerProgress = .94f;
        //Debug.LogWarning("Activeted Cheat spawn lvl: " + playerLvl);

        int playerIndex = playerLvl - 1;
        GameObject playerPrefab = _playerPool[playerIndex];
        coords.y = GameStaticInfo.FISH_SPAWN_HEINGT;
        playerPrefab.transform.position = coords;
        playerPrefab.transform.rotation = rotation;

        Fish playerFishBuffer = _playerPoolComp[playerIndex];
        playerFishBuffer.CopyData(playerFishBuffer.DefaultInfo.MaxHealth/*MaxHealth*/, playerProgress);

        if (_enemyDetector != null) {
            _enemyDetector.transform.SetParent(playerPrefab.transform);
            _enemyDetector.transform.localPosition = Vector3.zero;
            _enemyDetector.Init(playerFishBuffer);
        }

        EffectManager playerEffectManager = playerPrefab?.GetComponent<EffectManager>();
        if (isDie) {//apply shield effect after every player death

            playerEffectManager.SetEffectInQueue(FishEffectType.Shield, GameStaticInfo.PLAYER_INIT_SHIELD_DURATION);

#if UNITY_ANDROID && !UNITY_EDITOR
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_here_we_go_again);
#endif
            RespawnEntitiesMap(playerFishBuffer.Level);
        }
        else {
            if (isLoadFromSave)
                playerFishBuffer.CopyData(_savedWorldData.playerHealth, _savedWorldData.playerProgress);//copy data like health and progress from save
            else
                CheckPlayerLvlAchievements((FishLevel)playerLvl);//if current player respawn not called from save, check achievements maybe new lvl reached 
        }
        EnableAutoScalerComponent(playerPrefab);

        playerPrefab.SetActive(true);
        playerSpawnParticles.transform.position = coords;
        playerSpawnParticles.SetActive(true);//auto play on awake                  

        return playerFishBuffer;
    }

    public void RespawnPlayer(bool isDie, bool isRevive = false) {
        int playerLevel = (int)PlayerCurrentFish.Level;
        int playerIndex = playerLevel - 1;

        if (!isRevive) {
            if (isDie) {//player and die
                --playerLevel;
                playerLevel = Math.Max((int)GameStaticInfo.PLAYER_INIT_LVL, playerLevel);
                _playerPos = _playerInitSpawnPos;
                _playerRot = Quaternion.identity;
            }
            else  //lvl up
                ++playerLevel;

            _playerProg = 0f;
            _playerPool[playerIndex].SetActive(false);
        }

        PlayerCurrentFish = SpawnPlayerFish(_playerPos, _playerRot, false, playerLevel, _playerProg, isDie);

        if (PlayerCurrentFish != null) //null in case death of hard mode
            OnPlayerRespawn?.Invoke(PlayerCurrentFish, isDie);
    }
}
