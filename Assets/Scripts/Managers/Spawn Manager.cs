using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// spawn fishes including player, stores them in an array with index access
/// </summary>
public class SpawnManager : MonoBehaviour //with ObjectPool
{
    //pool
    [Header("Prefabs of fishes in order of levels")]
    [SerializeField] GameObject[] prefabs;
    //[SerializeField] GameObject prefab; //for multi prefab test
    //private readonly string[] fishPrefabAddressables = {
    //    "crab",
    //   "seaBass",
    //   "mackerel",
    //   "morayEel",
    //   "barracuda",
    //   "lionfish",
    //   "tuna",
    //   "hammerheadShark",
    //   "tigerShark",
    //   "w'eShark",
    //};
    [SerializeField] GameObject pufferfishPrefab;
    [SerializeField] GameObject cuttlefishPrefab;
    public const int PUFFERFISH_SPAWN_COUNT = 20;
    public const int CUTTLEFISH_SPAWN_COUNT = 30;
    public const int POOL_SIZE = 85;
    public const int PLAYER_INDEX = 0;
    /*[HideInInspector] public*/
    public static GameObject[] visibleFishPool { get; } = new GameObject[POOL_SIZE];
    GameObject[,] globalPool = new GameObject[(int)GameStaticInfo.MAX_FISH_LEVEL + 1, POOL_SIZE];
    public static Fish[] visPool_FishComp { get; } = new Fish[POOL_SIZE];//ref to visibleFishPool
    public static GameObject[] pufferfishPool { get; } = new GameObject[PUFFERFISH_SPAWN_COUNT];
    public static Fish[] pufferfishPool_FishComp { get; } = new Fish[PUFFERFISH_SPAWN_COUNT];
    public static GameObject[] cuttlefishPool { get; } = new GameObject[CUTTLEFISH_SPAWN_COUNT];
    public static Fish[] cuttlefishPool_FishComp { get; } = new Fish[CUTTLEFISH_SPAWN_COUNT];

    //spawn
    //[SerializeField] Transform spawnPlane;
    [SerializeField] Transform weakFishSpawnPointsParent;
    [SerializeField] Transform strongFishSpawnPointsParent;
    [SerializeField] Transform sharkSpawnPointsParent;
    [SerializeField] Transform pufferfishPointsParent;
    [SerializeField] Transform cuttlefishPointsParent;
    //private Transform[] weakFishSpawnPoints;
    //private Transform[] strongFishSpawnPoints;
    //private Transform[] sharkSpawnPoints;
    //private Transform[] pufferfishPoints;
    //private Transform[] cuttlefishPoints;
    /// <summary>
    /// used to notify when player respawns after death (for camera follow, etc.)
    /// </summary>
    public event Action OnPlayerDie;
    public event Action<Fish> OnPlayerRespawn;
    //public event Action OnFinishPoolCreation;
    //const float SPAWN_HEIGHT = 2.5f; //height offset
    private Vector3 playerSpawnPos;
    //[SerializeField] Transform[] spawnPoints;
    private static readonly float[] SPAWN_WEIGHTS = { .5f, .35f, .15f };
    private static readonly float[] SPAWN_WEIGHTS_EASY = { .65f, .33f, .02f };

    //player
    [SerializeField] GameObject playerSpawnParticles;
    [SerializeField] GameObject enemySkeletonParticle;
    [SerializeField] GameObject enemyDieParticles;
    [SerializeField] EnemyDetector enemyDetector;
    private Fish playerFish;
    public Fish PlayerFish {
        get { return playerFish; }
        private set { playerFish = value; }
    }
    private SaveData saveData;


    /// <summary>
    /// тільки для новооновлених гравців які можливо перебувають в недозволеній зоні відповідно до рівня
    /// only for newly updated players who may be in an unauthorized zone according to their level
    /// </summary>
    private bool isInAllowTPfromZone = false;


    public void Init() {
        saveData = ProgressSaver.Load();//load save data if exists
        playerSpawnPos = Vector3.up * (weakFishSpawnPointsParent.position.y /*+ SPAWN_HEIGHT*/);//in center map

        InitPool();//init global fishes pool

        if (saveData != null && saveData.IsDataValid) {
            playerFish = SpawnPlayerFish(playerFish, saveData.playerPosition, saveData.playerRotation, true, (FishLevel)saveData.playerLevel);
        }
        else
            playerFish = SpawnPlayerFish(playerFish, playerSpawnPos, Quaternion.identity, false);

        InitSpawnPrefabs();

        isInAllowTPfromZone = PlayerPrefs.GetInt(nameof(isInAllowTPfromZone), 1) == 1;
        PlayerPrefs.SetInt(nameof(isInAllowTPfromZone), 0);//deny next time

        if (isInAllowTPfromZone)
            playerFish.transform.position = Vector3.up * playerFish.transform.position.y;

        Debug.LogWarning("todo: fin method of pool down. (also fish comp arr)");
    }

    void InitPool() {
        for (int i = 0; i <= (int)GameStaticInfo.MAX_FISH_LEVEL; i++)
            for (int j = 0; j < POOL_SIZE; j++) {
                globalPool[i, j] = Instantiate(prefabs[i]);
                globalPool[i, j].SetActive(false);
            }
        Debug.LogWarning("mega pool enable and spawn disable");
    }

    GameObject GetFishFromPool(int level) {
        int fishType = level;
        //foreach (GameObject obj in poolAll[2]) {
        for (int i = 0; i < POOL_SIZE; i++) {
            GameObject obj = globalPool[fishType, i];

            if (!obj.activeInHierarchy) {
                obj.SetActive(true);
                return obj;
            }
        }
        return null; // або розширити пул
    }

    public void OnSceneExit() {
        SavePlayerData();
    }

    //save data
    private void OnApplicationQuit() {
        SavePlayerData();

        playerFish.OnFishDie -= FishDieAct;
        playerFish.OnFishLvlUp -= PlayerLvlUpAct;

        UnSubscribeFishesDieAct(pufferfishPool_FishComp, PUFFERFISH_SPAWN_COUNT/*pufferfishPool*/);
        UnSubscribeFishesDieAct(cuttlefishPool_FishComp, CUTTLEFISH_SPAWN_COUNT/*cuttlefishPool*/);
        UnSubscribeFishesDieAct(visPool_FishComp, POOL_SIZE/*pool*/);
    }

    void UnSubscribeFishesDieAct(Fish[] pool, int cnt) {
        for (int i = 0; i < cnt; i++)
            //if (pool[i].TryGetComponent<Fish>(out Fish fish))
            /*fish*/
            pool[i].OnFishDie -= FishDieAct;
    }

    void SavePlayerData() {
        saveData = new SaveData {
            playerPosition = /*pool[PLAYER_INDEX]*/playerFish.transform.position,
            playerRotation = /*pool[PLAYER_INDEX]*/playerFish.transform.rotation,
            playerHealth = playerFish.Health,
            playerLevel = (int)playerFish.Level,
            playerProgress = playerFish.Progress
        };
        ProgressSaver.Save(saveData);
    }

    //void InitSpawnPoints() {
    //    weakFishSpawnPoints = GetSpawnPointsFromParent(weakFishSpawnPointsParent);
    //    strongFishSpawnPoints = GetSpawnPointsFromParent(strongFishSpawnPointsParent);
    //    sharkSpawnPoints = GetSpawnPointsFromParent(sharkSpawnPointsParent);
    //    pufferfishPoints = GetSpawnPointsFromParent(pufferfishPointsParent);
    //    cuttlefishPoints = GetSpawnPointsFromParent(cuttlefishPointsParent);
    //}

    //Transform[] GetSpawnPointsFromParent(Transform parent) {
    //    Transform[] spawnPoints = new Transform[parent.childCount];
    //    int i = 0;

    //    foreach (Transform spawnPoint in parent.transform) {
    //        spawnPoints[i] = spawnPoint;
    //        ++i;
    //    }
    //    return spawnPoints;
    //}

    Fish SpawnPlayerFish(Fish prev_PlayerVar, Vector3 coords, Quaternion rotation, bool isLoadFromSave, FishLevel playerLvl = GameStaticInfo.PLAYER_INIT_LVL, bool isDie = false) {
        if (!isDie && visibleFishPool[PLAYER_INDEX] != null)
            OnPlayerRespawn?.Invoke(visPool_FishComp[PLAYER_INDEX]/*pool[PLAYER_INDEX].GetComponent<Fish>()*/);
        //playerLvl = FishLevel.WhiteShark;
        //Debug.LogWarning(playerLvl);
        const float PLAYER_INIT_SHIELD_DURATION = 18f; //initial shield duration after player respawn

        if (prev_PlayerVar != null) {//first spawn
            prev_PlayerVar.OnFishDie -= FishDieAct;
            prev_PlayerVar.OnFishLvlUp -= PlayerLvlUpAct;
        }

        GameObject playerPrefab = GetFishFromPool((int)playerLvl);/* Instantiate(prefabs[(int)playerLvl], coords, rotation)*/;
        visibleFishPool[PLAYER_INDEX] = playerPrefab;
        playerPrefab.transform.position = coords;
        playerPrefab.transform.rotation = rotation;

        prev_PlayerVar = visibleFishPool[PLAYER_INDEX].GetComponent<Fish>();
        prev_PlayerVar.IsPlayer = true;
        visPool_FishComp[PLAYER_INDEX] = prev_PlayerVar;

        //Destroy(playerPrefab.GetComponent<EnemyControl>());
        //playerPrefab.GetComponent<FishControl>().enabled = false;

        playerPrefab.name = "Player " + prev_PlayerVar.Level.ToString();
        playerPrefab.tag = "Player"; // set tag for player
        foreach (Transform t in playerPrefab.transform) //for bubble collider
            t.gameObject.tag = "Player";

        FishControl playerControl = playerPrefab./*AddComponent*/GetComponent<FishControl>();
        playerControl.Init(this, prev_PlayerVar);

        /* GameObject enemyDetector =*///Instantiate(enemyDetectorPrefab, playerPrefab.transform);
        enemyDetector.transform.SetParent(playerPrefab.transform);
        enemyDetector.Init(prev_PlayerVar/*playerPrefab.GetComponent<Fish>()*/);

        if (isDie) {//apply shield effect after every player death
            playerPrefab?.GetComponent<EffectManager>().ApplyEffect(FishEffectType.Shield, PLAYER_INIT_SHIELD_DURATION);
            GPG_Manager.OnTriggerAchievement(AchievementKey.HereWeGo);
            RespawnEntitiesMap();
        }
        else {
            if (isLoadFromSave)
                prev_PlayerVar.CopyData(saveData.playerHealth, saveData.playerProgress);//copy data like health and progress from save
            else
                CheckPlayerLvlAchievements(playerLvl);//if current player respawn not called from save, check achievements maybe new lvl reached 
        }

        playerPrefab.AddComponent<AutoScalerToDef>().Init();
        playerSpawnParticles.transform.position = coords;
        playerSpawnParticles.SetActive(true);//auto play on awake
                                             //playerSpawnParticles.Play();
                                             //StartCoroutine(DelayRevive(visibleFishPool[PLAYER_INDEX]));

        OnPlayerRespawn?.Invoke(prev_PlayerVar);
        prev_PlayerVar.OnFishDie += FishDieAct;
        prev_PlayerVar.OnFishLvlUp += PlayerLvlUpAct;

        return prev_PlayerVar;
    }

    /// <summary>
    /// respawn all +2 lvl fishes depending on player lvl after hes death
    /// </summary>
    void RespawnEntitiesMap() {
        for (int i = 1; i < POOL_SIZE; i++)
            if (/*pool[i].TryGetComponent(out Fish fish)*/visPool_FishComp[i] != null
                && /*fish*/visPool_FishComp[i].Level > playerFish.Level + 1)
                RespawnPrefab(visibleFishPool[i], i);
    }

    void FishDieAct(GameObject gameObject, FishLevel level) {
        DeactivatePrefab(gameObject, level, true);
    }

    void PlayerLvlUpAct(GameObject gameObject) {
        DeactivatePrefab(gameObject, playerFish.Level, false);
        Debug.LogWarning("player lvl up");
    }

    void CheckPlayerLvlAchievements(FishLevel playerLvl) {
        switch (playerLvl) {
            case FishLevel.Mackarel:
                GPG_Manager.OnTriggerAchievement(AchievementKey.SoThatsHow);
                break;
            //case FishLevel.Moray://not implement

            //    break;
            case FishLevel.Lionfish:
                GPG_Manager.OnTriggerAchievement(AchievementKey.Danger);
                break;
            case FishLevel.Barracuda:
                GPG_Manager.OnTriggerAchievement(AchievementKey.MackerelAgain);
                break;
            case FishLevel.Tuna:
                GPG_Manager.OnTriggerAchievement(AchievementKey.TrueSpeed);
                break;
            case FishLevel.HammerShark:
                GPG_Manager.OnTriggerAchievement(AchievementKey.BringNails);
                break;
            case FishLevel.TigerShark:
                GPG_Manager.OnTriggerAchievement(AchievementKey.GrrrMeow);
                break;
            case FishLevel.WhiteShark:
                GPG_Manager.OnTriggerAchievement(AchievementKey.ButItsGray);
                break;
        }
    }

    void SpawnEnemyFish(int poolIndex, int lvl, bool isRandomPos = false) {
        //Vector3 spawnPosition = GetRandSpawnPosition(lvl, isRandomPos);
        GameObject enemyPrefab;

        //if (spawnPosition == Vector3.one) //if no suitable spawn point found for lower lvl, try higher level
        //    enemyPrefab = Instantiate(prefabs[lvl + 1], GetRandSpawnPosition(lvl, isRandomPos), Quaternion.identity);
        //else if (spawnPosition == -Vector3.one) //if no suitable spawn point found for higher level, try lower level
        //    enemyPrefab = Instantiate(prefabs[lvl - 1], GetRandSpawnPosition(lvl, isRandomPos), Quaternion.identity);
        //else

        enemyPrefab = GetFishFromPool(lvl);//Instantiate(prefabs[lvl], GetRandSpawnPosition(lvl, isRandomPos), Quaternion.identity);
        enemyPrefab.transform.position = GetRandSpawnPosition(lvl, isRandomPos);

        Fish enemyFish = enemyPrefab.GetComponent<Fish>();
        enemyFish.IsPlayer = false;
        enemyPrefab.GetComponent<FishControl>().Init(this, enemyFish);
        //enemy.transform.position = enemy.GetComponent<Rigidbody>().position = GetRandSpawnPosition();
        enemyFish.SetRandomProgress(); //change progress every respawn
        enemyFish.OnFishDie += FishDieAct;

        enemyPrefab.name = "Enemy " + enemyFish.Level.ToString();
        enemyPrefab.tag = "Entity"; // set tag for player
        foreach (Transform t in enemyPrefab.transform) //for bubble collider
            t.gameObject.tag = "Entity";

        visPool_FishComp[poolIndex] = enemyFish;
        visibleFishPool[poolIndex] = enemyPrefab;
    }

    //void PlayerControllDelayInit() {
    //    pool[PLAYER_INDEX].GetComponent<PlayerControl>().Init(this);
    //    pool[PLAYER_INDEX].SetActive(true);
    //}

    //public GameObject GetObject() {
    //    foreach (var obj in pool) {
    //        if (!obj.activeInHierarchy) {
    //            obj.SetActive(true);
    //            return obj;
    //        }
    //    }

    //    return null;
    //}

    /// <summary>
    /// deactivates and respawn prefab
    /// </summary>
    /// <param name="prefab"></param>
    private void DeactivatePrefab(GameObject prefab, FishLevel level, bool isDie = true) {
        int i = 0;

        switch (level) {
            case FishLevel.Pufferfish:
                i = Array.IndexOf(pufferfishPool, prefab);
                if (i < 0)// if prefab not in pool, exit from method
                    return;
                RespawnPermamentFish(i, pufferfishPool, pufferfishPool_FishComp, pufferfishPointsParent /*pufferfishPoints*/);
                break;

            case FishLevel.Cuttlefish:
                i = Array.IndexOf(cuttlefishPool, prefab);
                if (i < 0)
                    return;
                RespawnPermamentFish(i, cuttlefishPool, cuttlefishPool_FishComp, cuttlefishPointsParent/* cuttlefishPoints*/);
                break;

            default://others
                i = Array.IndexOf(visibleFishPool, prefab);

                if (i < 0)
                    return;

                RespawnPrefab(visibleFishPool[i], i, isDie);
                break;
        }
    }

    private int GetRandSpawnLevel() {
        const int ALLOWED_LVLS_CNT = 3;
        int[] allowedLevels = new int[ALLOWED_LVLS_CNT] {
            Math.Max((int) playerFish.Level - 1, 0),
            (int) playerFish.Level,
            Math.Min((int)playerFish.Level + 1, (int)GameStaticInfo.MAX_FISH_LEVEL)
        };
        float random = UnityEngine.Random.value;//0 to 1
        float currentWeight = 0f;
        float[] spawnWeight = (playerFish.Level == FishLevel.SeaBass) ?
            SPAWN_WEIGHTS_EASY : SPAWN_WEIGHTS;//easy enemies for beginner players

        for (int i = 0; i < ALLOWED_LVLS_CNT; i++) {
            currentWeight += spawnWeight[i];

            if (random < currentWeight)
                return allowedLevels[i];
        }

        return allowedLevels[0];
    }
    private void InitSpawnPrefabs() {
        int currnentRandLvl = 0;

        for (int i = PLAYER_INDEX + 1; i < POOL_SIZE; i++) {
            currnentRandLvl = GetRandSpawnLevel();
            SpawnEnemyFish(i, currnentRandLvl, saveData == null);
        }

        //spawn Pufferfish
        InitSpawnPermamentFishes(pufferfishPrefab, pufferfishPointsParent /*pufferfishPoints*/, pufferfishPool, pufferfishPool_FishComp, PUFFERFISH_SPAWN_COUNT);
        //spawn cuttlefish
        InitSpawnPermamentFishes(cuttlefishPrefab, cuttlefishPointsParent /*cuttlefishPoints*/, cuttlefishPool, cuttlefishPool_FishComp, CUTTLEFISH_SPAWN_COUNT);

    }

    void InitSpawnPermamentFishes(GameObject prefab, Transform/*[]*/ spPoints, GameObject[] pool, Fish[] fishCompPool, int maxCnt) {
        int currentInd = 0;
        int pointsMaxInd = spPoints.childCount/*Length*/ - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);

        while (currentInd < maxCnt) {
            if (Vector3.Distance(playerFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                GameObject prefabInst = pool[currentInd] = Instantiate(prefab, spPoints.GetChild(spawnPointIndex).position /*+ RandomXZOffset()*/, Quaternion.identity);
                Fish fish = prefabInst.GetComponent<Fish>();
                FishControl enemyControl = prefabInst.GetComponent<FishControl>();

                enemyControl.Init(this, fish);
                fish.SetRandomProgress();
                fish.OnFishDie += FishDieAct;
                fishCompPool[currentInd] = fish;
                ++currentInd;
            }
            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);//(spawnPointIndex + 1 == spPoints.Length) ? 0 : ++spawnPointIndex;
        }
    }

    /// <summary>
    /// Fish get here after death)
    /// </summary>
    /// <param name="prefab"></param>
    private void RespawnPrefab(GameObject prefab, int prefabIndex, bool isDie = true) {//Attempted to realize spawn by weight respawn
                                                                                       //int prefabIndex = Array.IndexOf(pool, prefab);
        Fish currentFish = visPool_FishComp[prefabIndex];//pool[prefabIndex].GetComponent<Fish>();

        if (prefabIndex == PLAYER_INDEX) {// if player respawn, he died (lvl down)
            FishLevel playerLevel = currentFish.Level;
            Quaternion playerRot = visibleFishPool[prefabIndex].transform.rotation;

            visibleFishPool[prefabIndex].SetActive(false);
            //Destroy(visibleFishPool[prefabIndex]);

            if (isDie) {//player and die
                playerLevel = (FishLevel)Math.Max((int)GameStaticInfo.PLAYER_INIT_LVL, (int)playerLevel - 1);
                playerFish = SpawnPlayerFish(playerFish, playerSpawnPos, Quaternion.identity, false, playerLevel, isDie);
                currentFish.OnFishDie -= FishDieAct;
                currentFish.OnFishLvlUp -= PlayerLvlUpAct;
                OnPlayerDie?.Invoke();
            }
            else //lvl up
                playerFish = SpawnPlayerFish(playerFish, prefab.transform.position, playerRot, false, playerLevel + 1);
        }
        else {// if enemy fish respawn
              //spawn die particles
            if (currentFish.IsVisible) {
                Instantiate(enemySkeletonParticle, currentFish.transform.position, Quaternion.identity);
                Instantiate(enemyDieParticles, currentFish.transform.position, Quaternion.identity);
            }
            int currnentRandLvl = GetRandSpawnLevel();
            currentFish.OnFishDie -= FishDieAct;

            visibleFishPool[prefabIndex].SetActive(false);
            //Destroy(visibleFishPool[prefabIndex]); //
            SpawnEnemyFish(prefabIndex, currnentRandLvl, false);
        }

        gameObject.SetActive(true);
        //StartCoroutine(DelayRevive(visibleFishPool[prefabIndex]));
    }

    void RespawnPermamentFish(int index, GameObject[] pool, Fish[] fishCompPool, Transform/*[]*/ spPoints) {
        int pointsCnt = spPoints.childCount/*.Length*/ - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        GameObject prefabInst = pool[index];
        Fish fish = fishCompPool[index];//prefabInst.GetComponent<Fish>();

        //spawn dead particles
        if (fish.IsVisible) {
            Instantiate(enemySkeletonParticle, fish.transform.position, Quaternion.identity);
            Instantiate(enemyDieParticles, fish.transform.position, Quaternion.identity);
        }

        fish.Heal(fish.MaxHealth);//restore init parameters 
        fish.SetRandomProgress();
        prefabInst.SetActive(true);

        for (int attempt = 0; attempt <= pool.Length; attempt++) {
            if (Vector3.Distance(playerFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                pool[index].transform.position = spPoints.GetChild(spawnPointIndex).position;

                return;
            }

            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        }

    }

    //IEnumerator DelayRevive(GameObject gameObject) {
    //    yield return /*new WaitForSeconds(.5f)*/null; // wait 1 frame
    //                                                  //Debug.Log("Respawn");
    //    if (gameObject != null)
    //        gameObject.SetActive(true);
    //}

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

        //else {//rand spawn position farthest from player by distance
        for (int i = 0; i <= pointsMaxInd; i++) {//max attempts to find suitable spawn point
            index = (int)(UnityEngine.Random.value * pointsMaxInd);
            distanceToPlayer = Vector3.Distance(/*pool[PLAYER_INDEX]*/playerFish.transform.position, spawnPoints.GetChild(index).position);

            if (distanceToPlayer < GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW/* && distanceToPlayer < MAX_DISTANCE_FROM_PLAYER*/)
                continue;
            //else /*if (i == spawnPoints.Length - 1)*/ {//if all points are too close to player, return the last point
            //return spawnPoints[index].position + RandomXZOffset();
            //Debug.LogWarning($"SpawnManager: No suitable spawn point found for level {lvl}");
            //return index == 0 ? -Vector3.one : Vector3.one;

            return spawnPoints.GetChild(index).position + RandomXZOffset();
        }
        //}
        return spawnPoints.GetChild(index).position + RandomXZOffset();

        //const int PLANE_SCALEM_MODIFIER = 4;
        //const float SPAWN_RADIUS_DEVIATION = 0.95f; //to avoid spawn on the edge of the plane
        //Vector3 basePosition = spawnPlane.position;
        //float baseRadius = spawnPlane.localScale.x * PLANE_SCALEM_MODIFIER; //square plane
        //float edgeSideRand = (baseRadius - baseRadius * SPAWN_RADIUS_DEVIATION * UnityEngine.Random.value) * RandomSign();//to avoid spawn on the edge of the plane
        //float edgePosRand = UnityEngine.Random.Range(-baseRadius, baseRadius);
        //float spawnHeight = spawnPlane.position.y + SPAWN_HEIGHT;

        //if (UnityEngine.Random.Range(0, 2) == 0)//vertical
        //    return new Vector3(edgeSideRand, spawnHeight, edgePosRand);
        //else //horizontal
        //    return new Vector3(edgePosRand, spawnHeight, edgeSideRand);
        ////Vector3 farthestPos = spawnPoints[0].position;
        ////Vector3 playerPos = pool[PLAYER_INDEX].transform.position;
        ////float dist = Vector3.Distance(playerPos, farthestPos);

        ////foreach (Transform spawn in spawnPoints)
        ////    if (Vector3.Distance(playerPos, spawn.position) > dist) { //longest distance from player
        ////        dist = Vector3.Distance(playerPos, spawn.position);
        ////        farthestPos = spawn.position;
        ////    }
        ////return farthestPos;
    }

    Vector3 RandomXZOffset(float maxOffset = 10) {
        float offsetMultiplierX = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]
        float offsetMultiplierZ = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]

        return new Vector3(offsetMultiplierX * maxOffset, 0, offsetMultiplierZ * maxOffset);
    }

    //private int RandomSign() {
    //    return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
    //}

}
