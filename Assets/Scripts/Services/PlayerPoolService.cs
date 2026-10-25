using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public class PlayerPoolService : PoolServise, IPlayerFishPoolService {
    private GameObject[] _playerPool;
    private Fish[] _playerPoolComp;
    //private ObjectPool<GameObject> _playerPool;
    //private ObjectPool<Fish> _playerPoolComp;
    private Vector3 _playerPos;
    private Quaternion _playerRot;
    private float _playerProg;
    private Vector3 _playerInitSpawnPos;
    private EnemyDetector _enemyDetector;

    //private readonly FishPrefabsInfo _fishPrefabs;
    //private readonly AudioManager _audioManager;
    //private readonly VFXpoolManager _vfxPoolManager;
    private readonly FishSkinManager _fishSkinManager;

    public event Action<Fish, bool> OnPlayerRespawn;//bool - is respawn after death
    public event Action<Fish> OnFishDie;
    public event Action<FishLevel> OnPlayerLVLup;

    public Fish PlayerCurrentFish { get; private set; }

    public int Count => _playerPoolComp.Length;


    [Inject]
    public PlayerPoolService(FishPrefabsInfo fishPrefabs,/* AudioManager audioManager,*/
        /*VFXpoolManager vfxPoolManager,*/ FishSkinManager fishSkinManager) : base(fishPrefabs) {
        _savedWorldData = ProgressSaver.LoadWorldData();// ProgressSaver.IsWorldDataExist() ?             ProgressSaver.LoadWorldData() : new WorldData();//load save data if exists
        //_fishPrefabs = fishPrefabs;
        //_audioManager = audioManager;
        //_vfxPoolManager = vfxPoolManager;
        _fishSkinManager = fishSkinManager;

        _playerInitSpawnPos = Vector3.up * GameStaticInfo.FISH_SPAWN_HEINGT;//in center map
        //InitPlayerPool(fishPrefabs, fishPoolParent, audioManager, fishAdditionalCompManager, fishSkinManager);
    }

    public Fish GetAt(int index) => _playerPoolComp[index];

    public IEnumerator<Fish> GetEnumerator() {
        for (int i = 0; i < _playerPoolComp.Length; i++)
            yield return _playerPoolComp[i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void SpawnPlayerEnemyDetector(GameObject enemyDetectorPrefab) =>
        _enemyDetector = _resolver.Instantiate(enemyDetectorPrefab, _fishPoolParent).GetComponent<EnemyDetector>();

    public void Init(Transform fishPoolParent) {
        _fishPoolParent = fishPoolParent;
        _playerPool = new GameObject[GameStaticInfo.PLAYER_POOL_SIZE];
        _playerPoolComp = new Fish[GameStaticInfo.PLAYER_POOL_SIZE];

        for (int playerInd = 0, prefabInd = 1; playerInd < GameStaticInfo.PLAYER_POOL_SIZE; playerInd++, prefabInd++) {

            GameObject playerPrefab = _resolver.Instantiate(_fishPrefabs.GetFishPrefabByType((FishLevel)prefabInd), _fishPoolParent);
            FishControl fishControl = playerPrefab.GetComponent<FishControl>();
            Fish newPlayerFish = playerPrefab.GetComponent<Fish>();// fishControl.Fish;
            newPlayerFish.IsPlayer = true;
            newPlayerFish.OnFishDie += FishDieAct;
            newPlayerFish.OnFishLvlUp += PlayerLvlUpAct;
            fishControl.Init(/*this*//*, newPlayerFish*/);

            _playerPool[playerInd] = playerPrefab;
            _playerPoolComp[playerInd] = newPlayerFish;

            PrefabNamingTaging(playerPrefab, newPlayerFish.Level, GameStaticInfo.NAME_PLAYER, GameStaticInfo.TAG_PLAYER);
            if (playerPrefab.TryGetComponent(out FishAnim fishAnim))
                fishAnim.Init();
            //PrefabInitFishAnimAndEffectManagers(playerPrefab, _audioManager, _fishAdditionalCompManager, out FishAnim fishAnim);
            InitPlayerSkin(newPlayerFish, fishAnim, _fishSkinManager);
            EnableAutoScalerComponent(playerPrefab);

            playerPrefab.SetActive(false);
        }
    }

    public bool TryGetFish(GameObject fishObject, out Fish fish) {
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
            OnFishDie?.Invoke(PlayerCurrentFish);
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

    public void InitialSpawnPrefabs() {
        if (_savedWorldData != null) //continue
            PlayerCurrentFish = SpawnPlayerFish(_savedWorldData.playerPosition, _savedWorldData.playerRotation, true, (int)_savedWorldData.playerLevel, _savedWorldData.playerProgress);
        else//new game
            PlayerCurrentFish = SpawnPlayerFish(_playerInitSpawnPos, Quaternion.identity, false);

        OnPlayerRespawn?.Invoke(PlayerCurrentFish, false);
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
            //RespawnEntities(playerFishBuffer.Level);
        }
        else {
            if (isLoadFromSave)
                playerFishBuffer.CopyData(_savedWorldData.playerHealth, _savedWorldData.playerProgress);//copy data like health and progress from save
            //else
            //    CheckPlayerLvlAchievements((FishLevel)playerLvl);//if current player respawn not called from save, check achievements maybe new lvl reached 
        }
        EnableAutoScalerComponent(playerPrefab);

        playerPrefab.SetActive(true);

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
