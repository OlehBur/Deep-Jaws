using System;
//using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;


/// <summary>
/// spawn fishes including player, stores them in an array with index access
/// </summary>
public class SpawnManager : MonoBehaviour //with ObjectPool
{
    [Header("Managers Dependencies")]
    [SerializeField] FishSkinManager fishSkinManager;
    [SerializeField] VFXpoolManager fishAdditionalComponentManager;
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
    //[SerializeField] Transform weakFishSpawnPointsParent;
    //[SerializeField] Transform strongFishSpawnPointsParent;
    //[SerializeField] Transform sharkSpawnPointsParent;
    [SerializeField] Transform pufferfishPointsParent;
    [SerializeField] Transform cuttlefishPointsParent;

    //[Header("Effects Prefabs")]  //player  
    //[SerializeField] GameObject spawnParticles;
    //[SerializeField] GameObject enemySkeletonParticle;
    //[SerializeField] GameObject enemyDieParticles;
    //[SerializeField] EnemyDetector enemyDetector;


    //public static GameObject[] visible_poolFishObjects { get; } = new GameObject[GameStaticInfo.POOL_FISH_SIZE];
    //public static Fish[] visible_poolFishComponents { get; } = new Fish[GameStaticInfo.POOL_FISH_SIZE];//ref to visibleFishPool
    ///// <summary>
    /// with indexes equal lvl-1
    /// </summary>
    //public static GameObject[] playerPool_Objects { get; private set; }
    //public static Fish[] playerPool_FishComponents { get; private set; }
    //public static GameObject[] pufferfishPool_Objects { get; } = new GameObject[GameStaticInfo.PUFFERFISH_SPAWN_COUNT];
    //public static Fish[] pufferfishPool_FishComponents { get; } = new Fish[GameStaticInfo.PUFFERFISH_SPAWN_COUNT];
    //public static GameObject[] cuttlefishPool_Objects { get; } = new GameObject[GameStaticInfo.CUTTLEFISH_SPAWN_COUNT];
    //public static Fish[] cuttlefishPool_FishComponents { get; } = new Fish[GameStaticInfo.CUTTLEFISH_SPAWN_COUNT];
    //public static GameObject[] dartfishPool_Objects { get; } = new GameObject[GameStaticInfo.DARTFISH_SPAWN_COUNT];
    //public static Fish[] dartfishPool_FishComponents { get; } = new Fish[GameStaticInfo.DARTFISH_SPAWN_COUNT];

    //private float SpawnFishHeight => weakFishSpawnPointsParent.position.y;

    //private GameObject[,] globalPool_Objects = new GameObject[(int)GameStaticInfo.MAX_FISH_LEVEL + 1, GameStaticInfo.POOL_FISH_SIZE];
    //private Fish[,] globalPool_Components = new Fish[(int)GameStaticInfo.MAX_FISH_LEVEL + 1, GameStaticInfo.POOL_FISH_SIZE];
    //private Coroutine playerShieldCorutine;


    public void Init() {      
        //fishAdditionalComponentManager.Init(GameStaticInfo.SHIELDS_POOL_CNT,
        //    GameStaticInfo.DEAD_BUBBLES_POOL_CNT,
        //    GameStaticInfo.BITE_PARTICLES_POOL_CNT,
        //    GameStaticInfo.SKELETON_PARTICLE_POOL_CNT,
        //    GameStaticInfo.WHIRLE_POOL_CNT,
        //    GameStaticInfo.INK_PARTICLES_POOL_CNT);

        
        //InitPool();//init global enemy fishes pool
        //InitPlayerPool();

        DefaultSpawnAllPrefabs();

#if UNITY_ANDROID && !UNITY_EDITOR
        if (GameStaticInfo.Gamemode == GameMode.Hard)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_the_hard_way);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InitPlayerPool() {
        //playerPool_Objects = new GameObject[PlayerPoolSize];
        //playerPool_FishComponents = new Fish[PlayerPoolSize];

        //for (int playerInd = 0, prefabInd = 1; playerInd < PlayerPoolSize; playerInd++, prefabInd++) {

        //    GameObject playerPrefab = Instantiate(prefabs[prefabInd], fishPoolParent);
        //    FishControl fishControl = playerPrefab.GetComponent<FishControl>();
        //    Fish newPlayerFish = fishControl.Fish;//playerPrefab.GetComponent<Fish>();
        //    //if (newPlayerFish == null) {
        //    //    Debug.LogWarning("Player prefabs havent fish componend: " + playerPrefab.name);
        //    //    return;
        //    //}
        //    newPlayerFish.IsPlayer = true;
        //    newPlayerFish.OnFishDie += FishDieAct;
        //    newPlayerFish.OnFishLvlUp += PlayerLvlUpAct;
        //    fishControl.Init(this/*, newPlayerFish*/);

        //    playerPool_Objects[playerInd] = playerPrefab;
        //    playerPool_FishComponents[playerInd] = newPlayerFish;

        //    PrefabNamingTaging(playerPrefab, newPlayerFish.Level, true);
        //    PrefabInitFishAnimAndEffectManagers(playerPrefab, out FishAnim fishAnim, true);
        //    InitPlayerSkin(newPlayerFish, fishAnim);
        //    AddInitAutoScalerComponent(playerPrefab);

        //    playerPrefab.SetActive(false);
        //}
    }


    //void InitPlayerSkin(Fish fish, FishAnim fishAnim) {
    //    //load selected skin
    //    Skin selectedSkin;
    //    AccountData currentData = ProgressSaver.IsAccountDataExist ?
    //        ProgressSaver.LoadAccountData() : new AccountData();

    //    if (currentData != null && currentData.selectedSkin != Skin.Default)
    //        selectedSkin = currentData.selectedSkin;
    //    else
    //        return;

    //    Renderer renderer = fishAnim.FishSkinnedMesh;
    //    if (fishSkinManager != null)
    //        fishSkinManager.InitSkin(renderer, selectedSkin, fish.Level);
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //void PrefabNamingTaging(GameObject prefab, FishLevel level, bool isPlayer = false) {
    //    string name = isPlayer ? GameStaticInfo.NAME_PLAYER : GameStaticInfo.NAME_ENEMY;
    //    string tag = isPlayer ? GameStaticInfo.TAG_PLAYER : GameStaticInfo.TAG_ENTITY;

    //    prefab.name = name + level.ToString();
    //    prefab.tag = tag;
    //    foreach (Transform t in prefab.transform) //for bubble collider
    //        t.gameObject.tag = tag;
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //void EnableAutoScalerComponent(GameObject toPrefab) {
    //    AutoScalerToDef autoScaler = toPrefab.GetComponent<AutoScalerToDef>();
    //    if (autoScaler == null) {
    //        Debug.LogWarning("prefab havent AutoScaler component: " + toPrefab.name);
    //        toPrefab.AddComponent<AutoScalerToDef>().enabled = true;
    //    }
    //    else {
    //        autoScaler.enabled = true;
    //        return;
    //    }
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //void InitPool() {
    //    GameObject prefab;
    //    Fish enemyFish;
    //    for (int i = 0; i <= (int)GameStaticInfo.MAX_FISH_LEVEL; i++)
    //        for (int j = 0; j < GameStaticInfo.POOL_FISH_SIZE; j++) {
    //            prefab = Instantiate(prefabs[i], fishPoolParent);
    //            globalPool_Objects[i, j] = prefab;

    //            PrefabInitFishAnimAndEffectManagers(prefab, out FishAnim fishAnim);

    //            enemyFish = prefab.GetComponent<Fish>();
    //            globalPool_Components[i, j] = enemyFish;
    //            enemyFish.OnFishDie += FishDieAct;

    //            prefab.SetActive(false);
    //        }
    //    //Debug.LogWarning("mega pool enable and spawn disable");
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //void PrefabInitFishAnimAndEffectManagers(GameObject prefab, out FishAnim fishAnim, bool isPlayer = false) {
    //    if (prefab.TryGetComponent(out fishAnim) && prefab.TryGetComponent(out EffectManager effectManager)) {
    //        if (isPlayer) {
    //            fishAnim.Init(audioManager, effectManager, fishAdditionalComponentManager);
    //            effectManager.Init(audioManager, fishAdditionalComponentManager);
    //        }
    //        else {
    //            fishAnim.Init(null, effectManager, fishAdditionalComponentManager);
    //            effectManager.Init(null, fishAdditionalComponentManager);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="alreadyDeadObj">null if init spawn</param>
    ///// <param name="level"></param>
    ///// <param name="index"></param>
    ///// <returns></returns>
    //GameObject GetFishFromPool(GameObject alreadyDeadObj, int level, out int index) {
    //    index = -1;

    //    for (int i = 0; i < GameStaticInfo.POOL_FISH_SIZE; i++) {
    //        GameObject obj = globalPool_Objects[level, i];

    //        if (!obj.activeInHierarchy && obj != alreadyDeadObj && !globalPool_Components[level, i].IsAlreadySelected) {
    //            index = i;
    //            globalPool_Components[level, i].IsAlreadySelected = true;

    //            return obj;
    //        }
    //    }
    //    return null;
    //}

    //save data
    private void /*OnApplicationQuit*/OnDisable() {
        if (GameStaticInfo.Gamemode != GameMode.Hard)//in case of death & automatic scene reload
            SavePlayerData();

        PlayerCurrentFish.OnFishDie -= FishDieAct;
        PlayerCurrentFish.OnFishLvlUp -= PlayerLvlUpAct;

        UnSubscribeFishesDieAct(pufferfishPool_FishComponents, GameStaticInfo.PUFFERFISH_SPAWN_COUNT);
        UnSubscribeFishesDieAct(cuttlefishPool_FishComponents, GameStaticInfo.CUTTLEFISH_SPAWN_COUNT);
        UnSubscribeFishesDieAct(dartfishPool_FishComponents, GameStaticInfo.DARTFISH_SPAWN_COUNT);

        for (int i = 0; i <= (int)GameStaticInfo.MAX_FISH_LEVEL; i++)
            for (int j = 0; j < GameStaticInfo.ENEMY_POOL_SIZE; j++)
                globalPool_Components[i, j].OnFishDie -= FishDieAct;
    }

    void UnSubscribeFishesDieAct(Fish[] pool, int cnt) {
        for (int i = 0; i < cnt; i++)
            pool[i].OnFishDie -= FishDieAct;
    }

    //public void OnSceneExit() {
    //    SavePlayerData();
    //}

    ///// <summary>
    ///// respawn all +2 lvl fishes depending on player lvl after hes death
    ///// </summary>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //void RespawnEntitiesMap(FishLevel playerFishLVL) {
    //    //Debug.LogWarning("Disabled Reinit Entities map");
    //    for (int i = 0; i < GameStaticInfo.POOL_FISH_SIZE; i++)
    //        if (visible_poolFishObjects[i].activeInHierarchy && visible_poolFishComponents[i].Level > playerFishLVL) {
    //            visible_poolFishComponents[i].Die();
    //            //Debug.LogWarning("Respawn entity: " + visible_poolFishComponents[i]);
    //        }
    //}

    //void FishDieAct(GameObject gameObject, FishLevel level, bool isPlayer) {
    //    DeactivateRespawnPrefab(gameObject, level, isPlayer, true);
    //    //if (isPlayer)
    //    //    Debug.LogWarning("Player Dead");
    //}

    //void PlayerLvlUpAct(GameObject gameObject, FishLevel level) {
    //    if (GameManager.IsPC_Demo() && level >= FishLevel.Mackerel) {
    //        gameManager.GameSetPause(true);
    //        mainmenuManager.OnOpenDemoFin();
    //    }
    //    else
    //        DeactivateRespawnPrefab(gameObject, level, true, false);//true - only plyer event
    //    //Debug.LogWarning("player lvl up");
    //}

    void CheckPlayerLvlAchievements(FishLevel playerLvl) {
#if UNITY_ANDROID && !UNITY_EDITOR
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
#endif
    }

    //void SpawnEnemyFish(int visPoolIndex, int lvl, bool isRandomPos = false) {
    //    GameObject enemyPrefab;

    //    enemyPrefab = GetFishFromPool(visible_poolFishObjects[visPoolIndex], lvl, out int globalIndex);//Instantiate(prefabs[lvl], GetRandSpawnPosition(lvl, isRandomPos), Quaternion.identity);
    //    enemyPrefab.transform.position = GetRandSpawnPosition(lvl, isRandomPos);

    //    FishControl enemyControl = enemyPrefab.GetComponent<FishControl>();
    //    Fish enemyFish = globalPool_Components[lvl, globalIndex];//enemyPrefab.GetComponent<Fish>();
    //    enemyFish.IsPlayer = false;
    //    /*enemyPrefab.GetComponent<FishControl>()*/
    //    enemyFish.SetRandomProgress(); //change progress every respawn
    //    enemyControl.Init(this/*, enemyFish*/);

    //    PrefabNamingTaging(enemyPrefab, enemyFish.Level);

    //    visible_poolFishComponents[visPoolIndex] = enemyFish;
    //    visible_poolFishObjects[visPoolIndex] = enemyPrefab;

    //    enemyPrefab.SetActive(true);
    //}

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
            Transform spawnPoints = GetSpawnPointsByFishLevel(PlayerCurrentFish.Level);
            RespawnPermamentFish(visIndex, dartfishPool_Objects, dartfishPool_FishComponents, spawnPoints);
            break;

            //default://others default fishes & player

            //if (!isPlayer) {
            //    visIndex = Array.IndexOf(visible_poolFishObjects, prefab);

            //    if (visIndex >= 0)
            //        RespawnEnemy(visIndex, isDie);
            //}
            //else {//player
            //    //wait for retry\revive buttons touched & ad watch
            //    SaveDataForDelayPlayerRespawn();
            //    if (isDie)
            //        OnPlayerDied?.Invoke();
            //    else {
            //        RespawnPlayer(false); // lvl up
            //        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
            //        if (!prem && !noadd) {//if doesnt have immunitet

            //            if (/*gameAdsManager*/GameAdsManager.Instance.OnShowInterstitialAd()) {//only if ad will be shown
            //                mainmenuManager.ActivatePauseWnd(true);
            //            }
            //        }
            //    }
            //}
            break;
        }
    }

    //void SaveDataForDelayPlayerRespawn() {
    //    _playerPos = PlayerCurrentFish.transform.position;
    //    _playerRot = PlayerCurrentFish.transform.rotation;
    //    _playerProg = PlayerCurrentFish.Progress;
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private int GetRandSpawnLevel() {
    //    if (PlayerCurrentFish == null)
    //        return (int)GameStaticInfo.PLAYER_INIT_LVL;

    //    const int ALLOWED_LVLS_CNT = 3;
    //    int[] allowedLevels = new int[ALLOWED_LVLS_CNT] {
    //        Math.Max((int) PlayerCurrentFish.Level - 1, 0),
    //        (int) PlayerCurrentFish.Level,
    //        Math.Min((int)PlayerCurrentFish.Level + 1, (int)GameStaticInfo.MAX_FISH_LEVEL)
    //    };
    //    float random = UnityEngine.Random.value;//0 to 1
    //    float currentWeight = 0f;
    //    float[] spawnWeight = (PlayerCurrentFish.Level == FishLevel.SeaBass) ?
    //        SPAWN_WEIGHTS_EASY_START : SPAWN_WEIGHTS_EASY;//easy enemies for beginner players
    //                                                      //weight by gamemode

    //    if (GameStaticInfo.Gamemode == GameMode.None) {
    //        Debug.LogWarning("Activate fast game scene start!");

    //        if (ProgressSaver.IsWorldDataExist)
    //            GameStaticInfo.Gamemode = ProgressSaver.LoadWorldData().gameMode;

    //        if (GameStaticInfo.Gamemode == GameMode.None) {
    //            GameStaticInfo.Gamemode = GameMode.Easy;
    //            Debug.LogWarning("Gamemode selection System doesnt work properly!");
    //        }
    //    }
    //    spawnWeight = GameStaticInfo.Gamemode switch {
    //        GameMode.Easy => spawnWeight,
    //        GameMode.Medium => SPAWN_WEIGHTS_MEDIUM,
    //        GameMode.Hard => SPAWN_WEIGHTS_HARD,
    //        _ => SPAWN_WEIGHTS_EASY,
    //    };

    //    for (int i = 0; i < ALLOWED_LVLS_CNT; i++) {
    //        currentWeight += spawnWeight[i];

    //        if (random < currentWeight)
    //            return allowedLevels[i];
    //    }

    //    return allowedLevels[0];
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DefaultSpawnAllPrefabs() {
        //int currnentRandLvl = 0;

        ////spawn player from save
        //if (_savedWorldData != null)
        //    PlayerCurrentFish = SpawnPlayerFish(_savedWorldData.playerPosition, _savedWorldData.playerRotation, true, (int)_savedWorldData.playerLevel, _savedWorldData.playerProgress);
        //else//for begin
        //    PlayerCurrentFish = SpawnPlayerFish(_playerInitSpawnPos, Quaternion.identity, false);

        //OnPlayerRespawn?.Invoke(PlayerCurrentFish, false);

        //for (int i = 0; i < GameStaticInfo.POOL_FISH_SIZE; i++) {
        //    currnentRandLvl = GetRandSpawnLevel();
        //    SpawnEnemyFish(i, currnentRandLvl, _savedWorldData == null);
        //}

        //spawn Pufferfish
        InitSpawnPermamentFishes(pufferfishPrefab, pufferfishPointsParent /*pufferfishPoints*/, pufferfishPool_Objects, pufferfishPool_FishComponents, GameStaticInfo.PUFFERFISH_SPAWN_COUNT);
        //spawn cuttlefish
        InitSpawnPermamentFishes(cuttlefishPrefab, cuttlefishPointsParent /*cuttlefishPoints*/, cuttlefishPool_Objects, cuttlefishPool_FishComponents, GameStaticInfo.CUTTLEFISH_SPAWN_COUNT);
        //dartfish
        Transform spawnPoints = GetSpawnPointsByFishLevel(PlayerCurrentFish.Level);
        InitSpawnPermamentFishes(dartfishPrefab, spawnPoints, dartfishPool_Objects, dartfishPool_FishComponents, GameStaticInfo.DARTFISH_SPAWN_COUNT);
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
            if (Vector3.Distance(PlayerCurrentFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
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

    ///// <summary>
    ///// Fish (enemy) get here after death)
    ///// </summary>
    ///// <param name="prefab"></param>
    //private void RespawnEnemy(int visIndex, bool isDie = true) {//Attempted to realize spawn by weight respawn
    //    Fish currentFish = visible_poolFishComponents[visIndex];

    //    //spawn die particles
    //    if (currentFish.IsVisible) {
    //        fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.SkeletonParticle, fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
    //        fishAdditionalComponentManager.ActivateObjectFromPool(FishComponentType.DeadBubblesParticle, fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
    //    }
    //    int currnentRandLvl = GetRandSpawnLevel();

    //    visible_poolFishComponents[visIndex] = null;
    //    visible_poolFishObjects[visIndex] = null;
    //    SpawnEnemyFish(visIndex, currnentRandLvl, false);
    //}

    //public void RespawnPlayer(bool isDie, bool isRevive = false) {
    //    int playerLevel = (int)PlayerCurrentFish.Level;
    //    int playerIndex = playerLevel - 1;

    //    if (!isRevive) {
    //        if (isDie) {//player and die
    //            --playerLevel;
    //            playerLevel = Math.Max((int)GameStaticInfo.PLAYER_INIT_LVL, playerLevel);
    //            _playerPos = _playerInitSpawnPos;
    //            _playerRot = Quaternion.identity;
    //        }
    //        else  //lvl up
    //            ++playerLevel;

    //        _playerProg = 0f;
    //        playerPool_Objects[playerIndex].SetActive(false);
    //    }

    //    PlayerCurrentFish = SpawnPlayerFish(_playerPos, _playerRot, false, playerLevel, _playerProg, isDie);

    //    if (PlayerCurrentFish != null) //null in case death of hard mode
    //        OnPlayerRespawn?.Invoke(PlayerCurrentFish, isDie);
    //}

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
            if (Vector3.Distance(PlayerCurrentFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                pool[index].transform.position = spPoints.GetChild(spawnPointIndex).position;

                return;
            }

            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        }

    }

    ///// <summary>
    ///// повертає позицію рандомну найдальшу від гравця позицію для спавна ворогів у певному радіусі, враховуючи спавн точки для різних типів риб за рівнем 
    ///// якщо відповідної близької точки не знайдено, то повертає Vector3.one або -Vector3.one відповідно до необхідності більшого чи меншого рівня спавну риби
    ///// returns a random spawn position farthest from the player for spawning enemy in some max radius, considering spawn points for different types of fish by level
    ///// if no suitable spawn point found, returns Vector3.one or -Vector3.one depending on the need for a higher or lower level of fish spawn
    ///// </summary>
    ///// <param name="lvl"></param>
    ///// <returns></returns>
    //private Vector3 GetRandSpawnPosition(int lvl, bool isRandomPos = false) {//mwx lvl - 9
    //                                                                         //const float MAX_DISTANCE_FROM_PLAYER = 200f; //to avoid spawn too far to player
    //    const float SPAWN_POINTS_WEIGHT = 0.33f; //to get index in range [0; 2] for spawn points
    //    const float WEIGHT_FIXER = 0.01f; //to control some fish spawn points distribution
    //    float distanceToPlayer = 0f;
    //    int currentSpawnPointIndex = (int)(lvl * SPAWN_POINTS_WEIGHT + WEIGHT_FIXER);
    //    Vector3 playerPos = PlayerCurrentFish == null ? Vector3.zero : PlayerCurrentFish.transform.position;

    //    Transform/*[]*/ spawnPoints = currentSpawnPointIndex switch {
    //        0 => weakFishSpawnPointsParent/*weakFishSpawnPoints*/,
    //        1 => strongFishSpawnPointsParent/*strongFishSpawnPoints*/,
    //        2 => sharkSpawnPointsParent/*sharkSpawnPoints*/,
    //        _ => weakFishSpawnPointsParent /*weakFishSpawnPoints*/
    //    };

    //    int pointsMaxInd = spawnPoints.childCount/*Length*/ - 1;
    //    int index = (int)(UnityEngine.Random.value * pointsMaxInd);
    //    if (isRandomPos) //initial spawn in random position
    //        return spawnPoints.GetChild(index).position + RandomXZOffset();

    //    //rand spawn position farthest from player by distance
    //    for (int i = 0; i <= pointsMaxInd; i++) {//max attempts to find suitable spawn point
    //        index = (int)(UnityEngine.Random.value * pointsMaxInd);
    //        distanceToPlayer = Vector3.Distance(/*pool[PLAYER_INDEX]*/playerPos, spawnPoints.GetChild(index).position);

    //        if (distanceToPlayer < GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW/* && distanceToPlayer < MAX_DISTANCE_FROM_PLAYER*/)
    //            continue;

    //        return spawnPoints.GetChild(index).position + RandomXZOffset();
    //    }

    //    return spawnPoints.GetChild(index).position + RandomXZOffset();
    //}

    //Vector3 RandomXZOffset(float maxOffset = 6) {
    //    float offsetMultiplierX = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]
    //    float offsetMultiplierZ = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]

    //    return new Vector3(offsetMultiplierX * maxOffset, 0, offsetMultiplierZ * maxOffset);
    //}
}
