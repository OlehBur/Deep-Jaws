using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;


/// <summary>
/// spawn fishes including player, stores them in an array with index access
/// </summary>
public class SpawnManager : MonoBehaviour //with ObjectPool
{
    [Header("Managers Dependencies")]
    [SerializeField] FishSkinManager fishSkinManager;
    [SerializeField] FishAdditionalCompManager fishAdditionalComponentManager;
    [SerializeField] AudioManager audioManager;
    [SerializeField] MainmenuManager mainmenuManager;
    //[SerializeField] GameAdsManager gameAdsManager;
    [SerializeField] GameManager gameManager;

    [Header("Prefabs of Fishes in order of Levels")]//pool
    [SerializeField] Transform fishPoolParent;
    [SerializeField] GameObject[] prefabs;
    [SerializeField] GameObject pufferfishPrefab;
    [SerializeField] GameObject cuttlefishPrefab;
    [SerializeField] GameObject dartfishPrefab;

    [Header("Prefabs Spawnpoints")]  //spawn  
    [SerializeField] Transform weakFishSpawnPointsParent;
    [SerializeField] Transform strongFishSpawnPointsParent;
    [SerializeField] Transform sharkSpawnPointsParent;
    [SerializeField] Transform pufferfishPointsParent;
    [SerializeField] Transform cuttlefishPointsParent;

    [Header("Effects Prefabs")]  //player  
    [SerializeField] GameObject playerSpawnParticles;
    [SerializeField] GameObject enemySkeletonParticle;
    [SerializeField] GameObject enemyDieParticles;
    [SerializeField] EnemyDetector enemyDetector;

    /// <summary>
    /// used to notify when player respawns after death (for camera follow, etc.)
    /// </summary>
    public event Action<Fish, bool> OnPlayerRespawn;//bool - is respawn after death
    public event Action OnPlayerDied;
    public Fish PlayerFish { get; private set; }
    public static GameObject[] visible_poolFishObjects { get; } = new GameObject[POOL_SIZE];
    public static Fish[] visible_poolFishComponents { get; } = new Fish[POOL_SIZE];//ref to visibleFishPool
    /// <summary>
    /// with indexes equal lvl-1
    /// </summary>
    public static GameObject[] playerPool_Objects { get; private set; }
    public static Fish[] playerPool_FishComponents { get; private set; }
    public static GameObject[] pufferfishPool_Objects { get; } = new GameObject[PUFFERFISH_SPAWN_COUNT];
    public static Fish[] pufferfishPool_FishComponents { get; } = new Fish[PUFFERFISH_SPAWN_COUNT];
    public static GameObject[] cuttlefishPool_Objects { get; } = new GameObject[CUTTLEFISH_SPAWN_COUNT];
    public static Fish[] cuttlefishPool_FishComponents { get; } = new Fish[CUTTLEFISH_SPAWN_COUNT];
    public static GameObject[] dartfishPool_Objects { get; } = new GameObject[DARTFISH_SPAWN_COUNT];
    public static Fish[] dartfishPool_FishComponents { get; } = new Fish[DARTFISH_SPAWN_COUNT];
    public int PlayerPoolSize => prefabs.Length - 1;//crab is uncounted

    private float SpawnFishHeight => weakFishSpawnPointsParent.position.y;
    private Vector3 PlayerInitSpawnPos;
    private static readonly float[] SPAWN_WEIGHTS_EASY = { .5f, .35f, .15f };
    private static readonly float[] SPAWN_WEIGHTS_EASY_START = { .65f, .33f, .02f };
    private static readonly float[] SPAWN_WEIGHTS_MEDIUM = { .4f, .4f, .2f };
    private static readonly float[] SPAWN_WEIGHTS_HARD = { .34f, .33f, .33f };

    private Vector3 playerPos;
    private Quaternion playerRot;
    private float playerProg;
    private WorldData savedWorldData;
    /// <summary>
    /// тільки для новооновлених гравців які можливо перебувають в недозволеній зоні відповідно до рівня
    /// only for newly updated players who may be in an unauthorized zone according to their level
    /// </summary>
    private bool isInAllowTPfromZone = false;
    private GameObject[,] globalPool_Objects = new GameObject[(int)GameStaticInfo.MAX_FISH_LEVEL + 1, POOL_SIZE];
    private Fish[,] globalPool_Components = new Fish[(int)GameStaticInfo.MAX_FISH_LEVEL + 1, POOL_SIZE];
    private Coroutine playerShieldCorutine;

    public const int PUFFERFISH_SPAWN_COUNT = 20;
    public const int CUTTLEFISH_SPAWN_COUNT = 30;
    public const int DARTFISH_SPAWN_COUNT = 50;
    public const int POOL_SIZE = 85;

    private const float PLAYER_INIT_SHIELD_DURATION = 13f; //initial shield duration after player respawn
    private const int SHIELDS_POOL_CNT = 30; // POOL_SIZE/4
    private const int WHIRLE_POOL_CNT = 18;
    private const int BITE_PARTICLES_POOL_CNT = 25;
    private const int SKELETON_PARTICLE_POOL_CNT = 12;
    private const int DEAD_BUBBLES_POOL_CNT = 12;
    private const int INK_PARTICLES_POOL_CNT = 5;//CUTTLEFISH_SPAWN_COUNT / 6


    public void Init() {
        fishAdditionalComponentManager.Init(SHIELDS_POOL_CNT,
            DEAD_BUBBLES_POOL_CNT,
            BITE_PARTICLES_POOL_CNT,
            SKELETON_PARTICLE_POOL_CNT,
            WHIRLE_POOL_CNT,
            INK_PARTICLES_POOL_CNT);

        savedWorldData = ProgressSaver.LoadWorldData();// ProgressSaver.IsWorldDataExist() ?             ProgressSaver.LoadWorldData() : new WorldData();//load save data if exists
        PlayerInitSpawnPos = Vector3.up * (weakFishSpawnPointsParent.position.y /*+ SPAWN_HEIGHT*/);//in center map

        InitPool();//init global enemy fishes pool
        InitPlayerPool();
        DefaultSpawnAllPrefabs();

        isInAllowTPfromZone = PlayerPrefs.GetInt(nameof(isInAllowTPfromZone), 1) == 1;
        PlayerPrefs.SetInt(nameof(isInAllowTPfromZone), 0);//deny next time

        if (isInAllowTPfromZone)
            PlayerFish.transform.position = Vector3.up * PlayerFish.transform.position.y;

        //Debug.LogWarning("todo: fin method of pool down. (also fish comp arr)");
        if (GameStaticInfo.Gamemode == GameMode.Hard)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_the_hard_way);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InitPlayerPool() {
        playerPool_Objects = new GameObject[PlayerPoolSize];
        playerPool_FishComponents = new Fish[PlayerPoolSize];

        for (int playerInd = 0, prefabInd = 1; playerInd < PlayerPoolSize; playerInd++, prefabInd++) {

            GameObject playerPrefab = Instantiate(prefabs[prefabInd], fishPoolParent);
            FishControl fishControl = playerPrefab.GetComponent<FishControl>();
            Fish newPlayerFish = fishControl.Fish;//playerPrefab.GetComponent<Fish>();
            //if (newPlayerFish == null) {
            //    Debug.LogWarning("Player prefabs havent fish componend: " + playerPrefab.name);
            //    return;
            //}
            newPlayerFish.IsPlayer = true;
            newPlayerFish.OnFishDie += FishDieAct;
            newPlayerFish.OnFishLvlUp += PlayerLvlUpAct;
            fishControl.Init(this/*, newPlayerFish*/);

            playerPool_Objects[playerInd] = playerPrefab;
            playerPool_FishComponents[playerInd] = newPlayerFish;

            PrefabNamingTaging(playerPrefab, newPlayerFish.Level, true);
            PrefabInitFishAnimAndEffectManagers(playerPrefab, out FishAnim fishAnim, true);
            InitPlayerSkin(newPlayerFish, fishAnim);
            AddInitAutoScalerComponent(playerPrefab);

            playerPrefab.SetActive(false);
        }
    }


    void InitPlayerSkin(Fish fish, FishAnim fishAnim) {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PrefabNamingTaging(GameObject prefab, FishLevel level, bool isPlayer = false) {
        string name = isPlayer ? GameStaticInfo.NAME_PLAYER : GameStaticInfo.NAME_ENEMY;
        string tag = isPlayer ? GameStaticInfo.TAG_PLAYER : GameStaticInfo.TAG_ENTITY;

        prefab.name = name + level.ToString();
        prefab.tag = tag;
        foreach (Transform t in prefab.transform) //for bubble collider
            t.gameObject.tag = tag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void AddInitAutoScalerComponent(GameObject toPrefab) {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InitPool() {
        GameObject prefab;
        Fish enemyFish;
        for (int i = 0; i <= (int)GameStaticInfo.MAX_FISH_LEVEL; i++)
            for (int j = 0; j < POOL_SIZE; j++) {
                prefab = Instantiate(prefabs[i], fishPoolParent);
                globalPool_Objects[i, j] = prefab;

                PrefabInitFishAnimAndEffectManagers(prefab, out FishAnim fishAnim);

                enemyFish = prefab.GetComponent<Fish>();
                globalPool_Components[i, j] = enemyFish;
                enemyFish.OnFishDie += FishDieAct;

                prefab.SetActive(false);
            }
        //Debug.LogWarning("mega pool enable and spawn disable");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void PrefabInitFishAnimAndEffectManagers(GameObject prefab, out FishAnim fishAnim, bool isPlayer = false) {
        if (prefab.TryGetComponent(out fishAnim) && prefab.TryGetComponent(out EffectManager effectManager)) {
            if (isPlayer) {
                fishAnim.Init(audioManager, effectManager, fishAdditionalComponentManager);
                effectManager.Init(audioManager, fishAdditionalComponentManager);
            }
            else {
                fishAnim.Init(null, effectManager, fishAdditionalComponentManager);
                effectManager.Init(null, fishAdditionalComponentManager);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="alreadyDeadObj">null if init spawn</param>
    /// <param name="level"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    GameObject GetFishFromPool(GameObject alreadyDeadObj, int level, out int index) {
        index = -1;

        for (int i = 0; i < POOL_SIZE; i++) {
            GameObject obj = globalPool_Objects[level, i];

            if (!obj.activeInHierarchy && obj != alreadyDeadObj && !globalPool_Components[level, i].IsAlreadySelected) {
                index = i;
                globalPool_Components[level, i].IsAlreadySelected = true;

                return obj;
            }
        }
        return null;
    }

    //save data
    private void /*OnApplicationQuit*/OnDisable() {
        if (GameStaticInfo.Gamemode != GameMode.Hard)//in case of death & automatic scene reload
            SavePlayerData();

        PlayerFish.OnFishDie -= FishDieAct;
        PlayerFish.OnFishLvlUp -= PlayerLvlUpAct;

        UnSubscribeFishesDieAct(pufferfishPool_FishComponents, PUFFERFISH_SPAWN_COUNT);
        UnSubscribeFishesDieAct(cuttlefishPool_FishComponents, CUTTLEFISH_SPAWN_COUNT);
        UnSubscribeFishesDieAct(dartfishPool_FishComponents, DARTFISH_SPAWN_COUNT);

        for (int i = 0; i <= (int)GameStaticInfo.MAX_FISH_LEVEL; i++)
            for (int j = 0; j < POOL_SIZE; j++)
                globalPool_Components[i, j].OnFishDie -= FishDieAct;
    }

    void UnSubscribeFishesDieAct(Fish[] pool, int cnt) {
        for (int i = 0; i < cnt; i++)
            pool[i].OnFishDie -= FishDieAct;
    }

    public void OnSceneExit() {
        SavePlayerData();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SavePlayerData() {
        if (PlayerFish == null)
            return;

        WorldData worldPlayerData = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        worldPlayerData.playerPosition = PlayerFish.Rb.position;
        worldPlayerData.playerRotation = PlayerFish.VisualRotation;
        worldPlayerData.playerHealth = PlayerFish.Health;
        worldPlayerData.playerLevel = PlayerFish.Level;
        worldPlayerData.playerProgress = PlayerFish.Progress;
        worldPlayerData.gameMode = GameStaticInfo.Gamemode;

        ProgressSaver.SaveData(worldPlayerData);
    }
    Fish SpawnPlayerFish(Vector3 coords, Quaternion rotation, bool isLoadFromSave, int playerLvl = (int)GameStaticInfo.PLAYER_INIT_LVL, float playerProgress = 0f, bool isDie = false) {
        //playerLvl = (int)FishLevel.Moray;
        //playerProgress = .0f;
        //Debug.LogWarning("Activeted Cheat spawn lvl: " + playerLvl);

        int playerIndex = playerLvl - 1;

        if (GameStaticInfo.Gamemode == GameMode.Hard && isDie) {            //playerLvl = (int)GameStaticInfo.PLAYER_INIT_LVL;
            mainmenuManager.ResetAndReloadGame_Hard();
            enabled = false;
            return null;
        }

        GameObject playerPrefab = playerPool_Objects[playerIndex];
        coords.y = SpawnFishHeight;
        playerPrefab.transform.position = coords;
        playerPrefab.transform.rotation = rotation;

        Fish playerFishBuffer = playerPool_FishComponents[playerIndex];
        playerFishBuffer.CopyData(playerFishBuffer.DefaultInfo.MaxHealth/*MaxHealth*/, playerProgress);

        enemyDetector.transform.SetParent(playerPrefab.transform);
        enemyDetector.transform.localPosition = Vector3.zero;
        enemyDetector.Init(playerFishBuffer);

        EffectManager playerEffectManager = playerPrefab?.GetComponent<EffectManager>();
        if (isDie) {//apply shield effect after every player death
            if (playerShieldCorutine != null)
                StopCoroutine(playerShieldCorutine);

            playerShieldCorutine = StartCoroutine(AddShieldToPlayer_Delay(playerEffectManager));
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_here_we_go_again);
            RespawnEntitiesMap(playerFishBuffer.Level);
        }
        else {
            if (isLoadFromSave)
                playerFishBuffer.CopyData(savedWorldData.playerHealth, savedWorldData.playerProgress);//copy data like health and progress from save
            else
                CheckPlayerLvlAchievements((FishLevel)playerLvl);//if current player respawn not called from save, check achievements maybe new lvl reached 
        }
        AddInitAutoScalerComponent(playerPrefab);

        playerPrefab.SetActive(true);
        playerSpawnParticles.transform.position = coords;
        playerSpawnParticles.SetActive(true);//auto play on awake                  

        return playerFishBuffer;
    }

    IEnumerator AddShieldToPlayer_Delay(EffectManager playerEffectManager) {
        yield return null;
        playerEffectManager.ApplyEffect(FishEffectType.Shield, PLAYER_INIT_SHIELD_DURATION);
    }

    /// <summary>
    /// respawn all +2 lvl fishes depending on player lvl after hes death
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RespawnEntitiesMap(FishLevel playerFishLVL) {
        //Debug.LogWarning("Disabled Reinit Entities map");
        for (int i = 0; i < POOL_SIZE; i++)
            if (visible_poolFishObjects[i].activeInHierarchy && visible_poolFishComponents[i].Level > playerFishLVL) {
                visible_poolFishComponents[i].Die();
                //Debug.LogWarning("Respawn entity: " + visible_poolFishComponents[i]);
            }
    }

    void FishDieAct(GameObject gameObject, FishLevel level, bool isPlayer) {
        DeactivateRespawnPrefab(gameObject, level, isPlayer, true);
        //if (isPlayer)
        //    Debug.LogWarning("Player Dead");
    }

    void PlayerLvlUpAct(GameObject gameObject) {
        DeactivateRespawnPrefab(gameObject, PlayerFish.Level, true, false);//true - only plyer event
        //Debug.LogWarning("player lvl up");
    }

    void CheckPlayerLvlAchievements(FishLevel playerLvl) {
        switch (playerLvl) {
            case FishLevel.Mackerel:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_so_thats_how_it_is);
            break;
            //case FishLevel.Moray://not 
            //    break;
            case FishLevel.Lionfish:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_am_the_danger);
            break;
            case FishLevel.Barracuda:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_mackerel_again);
            break;
            case FishLevel.Tuna:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_finally_true_speed);
            break;
            case FishLevel.HammerShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_got_a_hammer__now_bring_the_nails);
            break;
            case FishLevel.TigerShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_grrr_meow);
            break;
            case FishLevel.WhiteShark:
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_but_its_gray);
            break;
        }
    }

    void SpawnEnemyFish(int visPoolIndex, int lvl, bool isRandomPos = false) {
        GameObject enemyPrefab;

        enemyPrefab = GetFishFromPool(visible_poolFishObjects[visPoolIndex], lvl, out int globalIndex);//Instantiate(prefabs[lvl], GetRandSpawnPosition(lvl, isRandomPos), Quaternion.identity);
        enemyPrefab.transform.position = GetRandSpawnPosition(lvl, isRandomPos);

        FishControl enemyControl = enemyPrefab.GetComponent<FishControl>();
        Fish enemyFish = globalPool_Components[lvl, globalIndex];//enemyPrefab.GetComponent<Fish>();
        enemyFish.IsPlayer = false;
        /*enemyPrefab.GetComponent<FishControl>()*/
        enemyFish.SetRandomProgress(); //change progress every respawn
        enemyControl.Init(this/*, enemyFish*/);

        PrefabNamingTaging(enemyPrefab, enemyFish.Level);

        visible_poolFishComponents[visPoolIndex] = enemyFish;
        visible_poolFishObjects[visPoolIndex] = enemyPrefab;

        enemyPrefab.SetActive(true);
    }

    /// <summary>
    /// deactivates and respawn prefab
    /// </summary>
    /// <param name="prefab"></param>
    private void DeactivateRespawnPrefab(GameObject prefab, FishLevel level, bool isPlayer, bool isDie = true) {
        int visIndex = 0;
        //if (isPlayer)
        //Debug.LogWarning("Player died on level: " + level);

        switch (level) {
            case FishLevel.Pufferfish:
            visIndex = Array.IndexOf(pufferfishPool_Objects, prefab);
            if (visIndex < 0)// if prefab not in pool, exit from method
                return;
            RespawnPermamentFish(visIndex, pufferfishPool_Objects, pufferfishPool_FishComponents, pufferfishPointsParent);
            break;

            case FishLevel.Cuttlefish:
            visIndex = Array.IndexOf(cuttlefishPool_Objects, prefab);
            if (visIndex < 0)
                return;
            RespawnPermamentFish(visIndex, cuttlefishPool_Objects, cuttlefishPool_FishComponents, cuttlefishPointsParent);
            break;

            case FishLevel.Dartfish:
            visIndex = Array.IndexOf(dartfishPool_Objects, prefab);
            if (visIndex < 0)
                return;
            Transform spawnPoints = GetSpawnPointsByFishLevel(PlayerFish.Level);
            RespawnPermamentFish(visIndex, dartfishPool_Objects, dartfishPool_FishComponents, spawnPoints);
            break;

            default://others default fishes & player

            if (!isPlayer) {
                visIndex = Array.IndexOf(visible_poolFishObjects, prefab);

                if (visIndex >= 0)
                    RespawnEnemy(visIndex, isDie);
            }
            else {//player
                //wait for retry\revive buttons touched & ad watch
                SaveDataForDelayPlayerRespawn();
                if (isDie)
                    OnPlayerDied?.Invoke();
                else {
                    RespawnPlayer(false); // lvl up
                    (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
                    if (!prem && !noadd) {//if doesnt have immunitet

                        if (/*gameAdsManager*/GameAdsManager.Instance.OnShowInterstitialAd()) {//only if ad will be shown
                            gameManager.GameSetPause(true);
                            mainmenuManager.ActivatePauseWnd(true);
                        }
                    }
                }
            }
            break;
        }
    }

    void SaveDataForDelayPlayerRespawn() {
        playerPos = PlayerFish.transform.position;
        playerRot = PlayerFish.transform.rotation;
        playerProg = PlayerFish.Progress;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRandSpawnLevel() {
        if (PlayerFish == null)
            return (int)GameStaticInfo.PLAYER_INIT_LVL;

        const int ALLOWED_LVLS_CNT = 3;
        int[] allowedLevels = new int[ALLOWED_LVLS_CNT] {
            Math.Max((int) PlayerFish.Level - 1, 0),
            (int) PlayerFish.Level,
            Math.Min((int)PlayerFish.Level + 1, (int)GameStaticInfo.MAX_FISH_LEVEL)
        };
        float random = UnityEngine.Random.value;//0 to 1
        float currentWeight = 0f;
        float[] spawnWeight = (PlayerFish.Level == FishLevel.SeaBass) ?
            SPAWN_WEIGHTS_EASY_START : SPAWN_WEIGHTS_EASY;//easy enemies for beginner players
        //weight by gamemode
        spawnWeight = GameStaticInfo.Gamemode switch {
            GameMode.Easy => spawnWeight,
            GameMode.Medium => SPAWN_WEIGHTS_MEDIUM,
            GameMode.Hard => SPAWN_WEIGHTS_HARD,
            _ => SPAWN_WEIGHTS_EASY,
        };

        for (int i = 0; i < ALLOWED_LVLS_CNT; i++) {
            currentWeight += spawnWeight[i];

            if (random < currentWeight)
                return allowedLevels[i];
        }

        return allowedLevels[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DefaultSpawnAllPrefabs() {
        int currnentRandLvl = 0;

        //spawn player from save
        if (savedWorldData != null)
            PlayerFish = SpawnPlayerFish(savedWorldData.playerPosition, savedWorldData.playerRotation, true, (int)savedWorldData.playerLevel, savedWorldData.playerProgress);
        else//for begin
            PlayerFish = SpawnPlayerFish(PlayerInitSpawnPos, Quaternion.identity, false);

        OnPlayerRespawn?.Invoke(PlayerFish, false);

        for (int i = 0; i < POOL_SIZE; i++) {
            currnentRandLvl = GetRandSpawnLevel();
            SpawnEnemyFish(i, currnentRandLvl, savedWorldData == null);
        }

        //spawn Pufferfish
        InitSpawnPermamentFishes(pufferfishPrefab, pufferfishPointsParent /*pufferfishPoints*/, pufferfishPool_Objects, pufferfishPool_FishComponents, PUFFERFISH_SPAWN_COUNT);
        //spawn cuttlefish
        InitSpawnPermamentFishes(cuttlefishPrefab, cuttlefishPointsParent /*cuttlefishPoints*/, cuttlefishPool_Objects, cuttlefishPool_FishComponents, CUTTLEFISH_SPAWN_COUNT);
        //dartfish
        Transform spawnPoints = GetSpawnPointsByFishLevel(PlayerFish.Level);
        InitSpawnPermamentFishes(dartfishPrefab, spawnPoints, dartfishPool_Objects, dartfishPool_FishComponents, DARTFISH_SPAWN_COUNT);
    }

    Transform GetSpawnPointsByFishLevel(FishLevel playerLvl) {
        Transform spawnPoints;
        switch (playerLvl) {
            case FishLevel.SeaBass:
            case FishLevel.Mackerel:
            spawnPoints = weakFishSpawnPointsParent;
            break;
            case FishLevel.Moray:
            case FishLevel.Lionfish:
            case FishLevel.Barracuda:
            spawnPoints = strongFishSpawnPointsParent;
            break;
            case FishLevel.HammerShark:
            case FishLevel.TigerShark:
            case FishLevel.WhiteShark:
            spawnPoints = sharkSpawnPointsParent;
            break;
            default:
            spawnPoints = weakFishSpawnPointsParent;
            break;
        }

        return spawnPoints;
    }

    void InitSpawnPermamentFishes(GameObject prefab, Transform spPoints, GameObject[] pool, Fish[] fishCompPool, int maxCnt) {
        int currentInd = 0;
        int pointsMaxInd = spPoints.childCount - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);

        while (currentInd < maxCnt) {
            if (Vector3.Distance(PlayerFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                GameObject prefabInst = pool[currentInd] = Instantiate(prefab, spPoints.GetChild(spawnPointIndex).position /*+ RandomXZOffset()*/, Quaternion.identity, fishPoolParent);
                FishControl enemyPermControl = prefabInst.GetComponent<FishControl>();
                Fish fishPermament = enemyPermControl.Fish;

                if (prefabInst.TryGetComponent(out PufferfishController pufferController))
                    pufferController.Init(audioManager);
                else if (prefabInst.TryGetComponent(out СuttlefishController cuttleController))
                    cuttleController.Init(audioManager, fishAdditionalComponentManager);

                enemyPermControl.Init(this);
                fishPermament.SetRandomProgress();
                fishPermament.OnFishDie += FishDieAct;
                fishCompPool[currentInd] = fishPermament;
                pool[currentInd].SetActive(true);

                ++currentInd;
            }
            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);//(spawnPointIndex + 1 == spPoints.Length) ? 0 : ++spawnPointIndex;
        }
    }

    /// <summary>
    /// Fish (enemy) get here after death)
    /// </summary>
    /// <param name="prefab"></param>
    private void RespawnEnemy(int visIndex, bool isDie = true) {//Attempted to realize spawn by weight respawn
        Fish currentFish = visible_poolFishComponents[visIndex];

        //spawn die particles
        if (currentFish.IsVisible) {
            fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.SkeletonParticle, fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
            fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.DeadBubblesParticle, fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
        }
        int currnentRandLvl = GetRandSpawnLevel();

        visible_poolFishComponents[visIndex] = null;
        visible_poolFishObjects[visIndex] = null;
        SpawnEnemyFish(visIndex, currnentRandLvl, false);
    }

    public void RespawnPlayer(bool isDie, bool isRevive = false) {
        int playerLevel = (int)PlayerFish.Level;
        int playerIndex = playerLevel - 1;

        if (!isRevive) {
            if (isDie) {//player and die
                --playerLevel;
                playerLevel = Math.Max((int)GameStaticInfo.PLAYER_INIT_LVL, playerLevel);
                playerPos = PlayerInitSpawnPos;
                playerRot = Quaternion.identity;
            }
            else  //lvl up
                ++playerLevel;

            playerProg = 0f;
            playerPool_Objects[playerIndex].SetActive(false);
        }

        PlayerFish = SpawnPlayerFish(playerPos, playerRot, false, playerLevel, playerProg, isDie);

        if (PlayerFish != null) //null in case death of hard mode
            OnPlayerRespawn?.Invoke(PlayerFish, isDie);
    }

    void RespawnPermamentFish(int index, GameObject[] pool, Fish[] fishCompPool, Transform spPoints) {
        int pointsCnt = spPoints.childCount - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        GameObject prefabInst = pool[index];
        Fish fish = fishCompPool[index];

        //spawn dead particles
        if (fish.IsVisible) {
            fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.SkeletonParticle, fishPoolParent, fish.transform.localPosition, Vector3.one, Quaternion.identity);
            fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.DeadBubblesParticle, fishPoolParent, fish.transform.localPosition, Vector3.one, Quaternion.identity);
        }

        fish.Heal(fish./*MaxHealth*/DefaultInfo.MaxHealth);//restore init parameters 
        fish.SetRandomProgress();
        prefabInst.SetActive(true);

        for (int attempt = 0; attempt <= pool.Length; attempt++) {
            if (Vector3.Distance(PlayerFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                pool[index].transform.position = spPoints.GetChild(spawnPointIndex).position;

                return;
            }

            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        }

    }

    /// <summary>
    /// повертає позицію рандомну найдальшу від гравця позицію для спавна ворогів у певному радіусі, враховуючи спавн точки для різних типів риб за рівнем 
    /// якщо відповідної близької точки не знайдено, то повертає Vector3.one або -Vector3.one відповідно до необхідності більшого чи меншого рівня спавну риби
    /// returns a random spawn position farthest from the player for spawning enemy in some max radius, considering spawn points for different types of fish by level
    /// if no suitable spawn point found, returns Vector3.one or -Vector3.one depending on the need for a higher or lower level of fish spawn
    /// </summary>
    /// <param name="lvl"></param>
    /// <returns></returns>
    private Vector3 GetRandSpawnPosition(int lvl, bool isRandomPos = false) {//mwx lvl - 9
                                                                             //const float MAX_DISTANCE_FROM_PLAYER = 200f; //to avoid spawn too far to player
        const float SPAWN_POINTS_WEIGHT = 0.33f; //to get index in range [0; 2] for spawn points
        const float WEIGHT_FIXER = 0.01f; //to control some fish spawn points distribution
        float distanceToPlayer = 0f;
        int currentSpawnPointIndex = (int)(lvl * SPAWN_POINTS_WEIGHT + WEIGHT_FIXER);
        Vector3 playerPos = PlayerFish == null ? Vector3.zero : PlayerFish.transform.position;

        Transform/*[]*/ spawnPoints = currentSpawnPointIndex switch {
            0 => weakFishSpawnPointsParent/*weakFishSpawnPoints*/,
            1 => strongFishSpawnPointsParent/*strongFishSpawnPoints*/,
            2 => sharkSpawnPointsParent/*sharkSpawnPoints*/,
            _ => weakFishSpawnPointsParent /*weakFishSpawnPoints*/
        };

        int pointsMaxInd = spawnPoints.childCount/*Length*/ - 1;
        int index = (int)(UnityEngine.Random.value * pointsMaxInd);
        if (isRandomPos) //initial spawn in random position
            return spawnPoints.GetChild(index).position + RandomXZOffset();

        //rand spawn position farthest from player by distance
        for (int i = 0; i <= pointsMaxInd; i++) {//max attempts to find suitable spawn point
            index = (int)(UnityEngine.Random.value * pointsMaxInd);
            distanceToPlayer = Vector3.Distance(/*pool[PLAYER_INDEX]*/playerPos, spawnPoints.GetChild(index).position);

            if (distanceToPlayer < GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW/* && distanceToPlayer < MAX_DISTANCE_FROM_PLAYER*/)
                continue;

            return spawnPoints.GetChild(index).position + RandomXZOffset();
        }

        return spawnPoints.GetChild(index).position + RandomXZOffset();
    }

    Vector3 RandomXZOffset(float maxOffset = 6) {
        float offsetMultiplierX = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]
        float offsetMultiplierZ = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]

        return new Vector3(offsetMultiplierX * maxOffset, 0, offsetMultiplierZ * maxOffset);
    }
}
