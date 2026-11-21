using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class PlayerPoolService : PoolServise, IPlayerFishPoolService {
    private GameObject[] _playerPool;
    private FishStatsController[] _playerPoolComp;
    private Vector3 _playerPos;
    private Quaternion _playerRot;
    private float _playerProg;
    private Vector3 _playerInitSpawnPos;
    private GameObject _enemyDetector;

    private readonly FishSkinInfo _fishSkinManager;
    private readonly IAdsManager _adsManager;

    public event Action<FishStatsController, bool> OnPlayerRespawn;//bool - is respawn after death
    public event Action<FishStatsController> OnFishDie;
    public event Action<FishLevel> OnPlayerLVLup;
    public event Action OnPoolInitialSpawnCompleted;
    public bool IsInitialSpawnCompleted { get; private set; } = false;

    public FishStatsController PlayerCurrentFish { get; private set; }

    public int Count => _playerPoolComp == null ? 0 : _playerPoolComp.Length;


    [Inject]
    public PlayerPoolService(FishPrefabsInfo fishPrefabs, FishSkinInfo fishSkinManager, IObjectResolver resolver, IAdsManager adsManager) : base(fishPrefabs, resolver) {
        _savedWorldData = ProgressSaver.LoadWorldData();

        _fishSkinManager = fishSkinManager;
        _adsManager = adsManager;

        _playerInitSpawnPos = Vector3.up * GameStaticInfo.FISH_SPAWN_HEINGT;//in center map
                                                                            //InitPlayerPool(fishPrefabs, fishPoolParent, audioManager, fishAdditionalCompManager, fishSkinManager);

#if !UNITY_STANDALONE_WIN
        _adsManager.OnDeadRespawn += PlayerRespawnHandler;
        _adsManager.OnDeadRevive += PlayerReviveHandler;
#endif
    }

    public FishStatsController GetAt(int index) => _playerPoolComp[index];

    public IEnumerator<FishStatsController> GetEnumerator() {
        for (int i = 0; i < _playerPoolComp.Length; i++)
            yield return _playerPoolComp[i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Init(Transform fishPoolParent) {
        _fishPoolParent = fishPoolParent;
        _playerPool = new GameObject[GameStaticInfo.PLAYER_POOL_SIZE];
        _playerPoolComp = new FishStatsController[GameStaticInfo.PLAYER_POOL_SIZE];
        _enemyDetector = _resolver.Instantiate(_fishPrefabs.EnemyDetector, _fishPoolParent);

        FishLevel prefabInd = FishLevel.SeaBass;
        for (int playerInd = 0; playerInd < GameStaticInfo.PLAYER_POOL_SIZE; playerInd++, prefabInd++) {
            GameObject playerPrefab = _resolver.Instantiate(_fishPrefabs.GetFishPrefabByType(prefabInd), _fishPoolParent);
            FishController fishControl = playerPrefab.GetComponent<FishController>();
            FishStatsController newPlayerFish = playerPrefab.GetComponent<FishStatsController>();// fishControl.Fish;
            newPlayerFish.IsPlayer = true;
            newPlayerFish.OnFishDie += FishDieAct;
            newPlayerFish.OnFishLvlUp += PlayerLvlUpAct;
            fishControl.Init();

            _playerPool[playerInd] = playerPrefab;
            _playerPoolComp[playerInd] = newPlayerFish;

            PrefabNamingTaging(playerPrefab, newPlayerFish.Level, GameStaticInfo.NAME_PLAYER, GameStaticInfo.TAG_PLAYER);
            if (playerPrefab.TryGetComponent(out FishAnim fishAnim))
                fishAnim.Init();

            InitPlayerSkin(newPlayerFish, fishAnim, _fishSkinManager);
            EnableAutoScalerComponent(playerPrefab);

            playerPrefab.SetActive(false);
        }
    }

#if !UNITY_STANDALONE_WIN
    void PlayerRespawnHandler() =>
        RespawnPlayer(true, false);
    void PlayerReviveHandler() =>
        RespawnPlayer(true, true);
#endif

    public bool TryGetFish(GameObject fishObject, out FishStatsController fish) {
        int index = Array.IndexOf(_playerPool, fishObject);
        if (index < 0) {
            fish = null;
            return false;
        }
        else {
            fish = _playerPoolComp[index];
            return true;
        }
    }

    void FishDieAct(GameObject gameObject, FishLevel level) =>
        RespawnPrefab(level, true);

    void PlayerLvlUpAct(GameObject gameObject, FishLevel level) =>
        RespawnPrefab(level, false);//true - only plyer event

    private void RespawnPrefab(FishLevel level, bool isDie = true) {

        //wait for retry\revive buttons touched & ad watch
        SaveDataForDelayPlayerRespawn();
        if (isDie)
            OnFishDie?.Invoke(PlayerCurrentFish);
        else {
            OnPlayerLVLup?.Invoke(level);
            RespawnPlayer(false); // lvl up
        }
    }

    void InitPlayerSkin(FishStatsController fish, FishAnim fishAnim, FishSkinInfo fishSkinManager) {
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
        worldPlayerData.gameMode = GameModeWnd.CurrentMode;

        ProgressSaver.SaveData(worldPlayerData);
    }

    void SaveDataForDelayPlayerRespawn() {
        _playerPos = PlayerCurrentFish.transform.position;
        _playerRot = PlayerCurrentFish.transform.rotation;
        _playerProg = PlayerCurrentFish.Progress;
    }

    public void InitialSpawnPrefabs() {
        if (_savedWorldData != null) //continue
            PlayerCurrentFish = SpawnPlayerFish(_savedWorldData.playerPosition, _savedWorldData.playerRotation, true, (int)_savedWorldData.playerLevel, _savedWorldData.playerProgress);
        else//new game
            PlayerCurrentFish = SpawnPlayerFish(_playerInitSpawnPos, Quaternion.identity, false);

        IsInitialSpawnCompleted = true;

        OnPlayerRespawn?.Invoke(PlayerCurrentFish, false);
        OnPoolInitialSpawnCompleted?.Invoke();
    }

    FishStatsController SpawnPlayerFish(Vector3 coords, Quaternion rotation, bool isLoadFromSave, int playerLvl = (int)GameStaticInfo.PLAYER_INIT_LVL, float playerProgress = 0f, bool isDie = false) {
        //playerLvl = (int)FishLevel.Mackerel;
        //playerProgress = .94f;
        //Debug.LogWarning("Activeted Cheat spawn lvl: " + playerLvl);

        int playerIndex = playerLvl - 1;
        GameObject playerPrefab = _playerPool[playerIndex];
        coords.y = GameStaticInfo.FISH_SPAWN_HEINGT;
        playerPrefab.transform.position = coords;
        playerPrefab.transform.rotation = rotation;
        playerPrefab.transform.SetParent(_fishPoolParent, true);

        FishStatsController playerFishBuffer = _playerPoolComp[playerIndex];
        playerFishBuffer.CopyData(playerFishBuffer.DefaultInfo.MaxHealth, playerProgress);
        playerFishBuffer.ActivateCinematic(false);

        if (_enemyDetector != null) {
            _enemyDetector.transform.SetParent(playerPrefab.transform);
            _enemyDetector.transform.localPosition = Vector3.zero;
        }

        EffectManager playerEffectManager = playerPrefab?.GetComponent<EffectManager>();
        if (isDie) {//apply shield effect after every player death

            playerEffectManager.SetEffectInQueue(FishEffectType.Shield, GameStaticInfo.PLAYER_INIT_SHIELD_DURATION);

#if UNITY_ANDROID && !UNITY_EDITOR
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_here_we_go_again);
#endif
        }
        else {
            if (isLoadFromSave)
                playerFishBuffer.CopyData(_savedWorldData.playerHealth, _savedWorldData.playerProgress);//copy data like health and progress from save
        }

        EnableAutoScalerComponent(playerPrefab);

        playerPrefab.SetActive(true);

        return playerFishBuffer;
    }

    public void RespawnPlayer(bool isDie, bool isRevive = false) {
        int playerLevel = (int)PlayerCurrentFish.Level;

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
        }

        PlayerCurrentFish = SpawnPlayerFish(_playerPos, _playerRot, false, playerLevel, _playerProg, isDie);

        if (PlayerCurrentFish != null) //null in case death of hard mode
            OnPlayerRespawn?.Invoke(PlayerCurrentFish, isDie);
    }

    public void OnDestroy() {
#if !UNITY_STANDALONE_WIN
        _adsManager.OnDeadRespawn -= PlayerRespawnHandler;
        _adsManager.OnDeadRevive -= PlayerReviveHandler;
#endif
    }
}
