using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;


public static class EnemyPoolExtensions {
    public static GameObject Get(this GameObject[,] pool, FishLevel level, int index)
        => pool[(int)level, index];
    public static GameObject Set(this GameObject[,] pool, FishLevel level, int index, GameObject value)
        => pool[(int)level, index] = value;
    public static Fish Get(this Fish[,] pool, FishLevel level, int index)
        => pool[(int)level, index];
    public static Fish Set(this Fish[,] pool, FishLevel level, int index, Fish value)
        => pool[(int)level, index] = value;
}


public class EnemyPoolService : PoolServise, IEnemyFishPoolService {
    private GameObject[,] _enemyPool;
    private Fish[,] _enemyPoolComp;
    private Transform _weakFishSpawnPointsParent;
    private Transform _strongFishSpawnPointsParent;
    private Transform _sharkSpawnPointsParent;
    //private VFXpoolService _vfxPoolManager;
    public event Action<Fish> OnFishDie;

    private readonly IPlayerFishPoolService _playerPoolProvider;

    private static readonly float[] SPAWN_WEIGHTS_EASY = { .5f, .35f, .15f };
    private static readonly float[] SPAWN_WEIGHTS_EASY_START = { .65f, .33f, .02f };
    private static readonly float[] SPAWN_WEIGHTS_MEDIUM = { .4f, .4f, .2f };
    private static readonly float[] SPAWN_WEIGHTS_HARD = { .34f, .33f, .33f };

    private int _levels;
    private int _perLevel;
    public int Count => _levels * _perLevel;


    [Inject]
    public EnemyPoolService(IPlayerFishPoolService playerPoolProvider, FishPrefabsInfo fishPrefabs/*, VFXpoolService vfxPoolManager*/) : base(fishPrefabs) {
        //_fishPrefabs = fishPrefabs;
        playerPoolProvider = _playerPoolProvider;
        //_vfxPoolManager = vfxPoolManager;

        _playerPoolProvider.OnPlayerRespawn += PlayerRespawnHandler;
    }

    public Fish GetAt(int index) {
        int level = index / _perLevel;
        int idx = index % _perLevel;

        return _enemyPoolComp[level, idx];
    }

    public IEnumerator<Fish> GetEnumerator() {
        for (int l = 0; l < _levels; l++)
            for (int i = 0; i < _perLevel; i++)
                yield return _enemyPoolComp[l, i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void PlayerRespawnHandler(Fish playerFish, bool isDie) {
        if (isDie)
            RespawnAllEnemies();
    }

    public void Init(Transform fishPoolParent) {
        _fishPoolParent = fishPoolParent;
        _savedWorldData = ProgressSaver.LoadWorldData();
        GameObject prefab;
        Fish enemyFish;

        for (FishLevel level = FishLevel.Crab; level <= GameStaticInfo.MAX_FISH_LEVEL; level++)
            for (int j = 0; j < GameStaticInfo.ENEMY_POOL_SIZE; j++) {
                prefab = _resolver.Instantiate(_fishPrefabs.GetFishPrefabByType(level), _fishPoolParent);
                _enemyPool.Set(level, j, prefab);
                enemyFish = prefab.GetComponent<Fish>();
                _enemyPoolComp.Set(level, j, enemyFish);
                enemyFish.OnFishDie += FishDieAct;

                if (prefab.TryGetComponent(out FishAnim fishAnim))
                    fishAnim.Init();

                prefab.SetActive(false);
            }
        //Debug.LogWarning("mega pool enable and spawn disable");

        _levels = _enemyPoolComp.GetLength(0);
        _perLevel = _enemyPoolComp.GetLength(1);
    }

    //public void SetPlayerProvider(IPlayerPoolService playerPoolProvider) =>
    //    _playerPoolProvider = playerPoolProvider;

    public void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent, Transform sharkSpawnPointsParent) {
        _weakFishSpawnPointsParent = weakFishSpawnPointsParent;
        _strongFishSpawnPointsParent = strongFishSpawnPointsParent;
        _sharkSpawnPointsParent = sharkSpawnPointsParent;
    }

    public void InitialSpawnPrefabs() {
        FishLevel currnentRandLvl;

        for (int i = 0; i < GameStaticInfo.ENEMY_POOL_SIZE; i++) {
            currnentRandLvl = GetRandSpawnLevel();
            SpawnEnemyFish(currnentRandLvl, i, _savedWorldData == null);
        }
    }

    public void RespawnAllEnemies(/*FishLevel playerFishLVL*/) {
        FishLevel playerFishLVL = _playerPoolProvider.PlayerCurrentFish.Level;

        //Debug.LogWarning("Disabled Reinit Entities map");
        for (FishLevel level = FishLevel.Crab; level <= GameStaticInfo.MAX_FISH_LEVEL; level++)
            for (int i = 0; i < GameStaticInfo.ENEMY_POOL_SIZE; i++) {
                Fish enemyBuff = _enemyPoolComp.Get(level, i);

                if (enemyBuff.IsVisible && enemyBuff.Level > playerFishLVL) {
                    enemyBuff.Die();
                    //Debug.LogWarning("Respawn entity: " + visible_poolFishComponents[i]);
                }
            }
    }

    //public void PrefabNamingTaging(GameObject prefab, FishLevel level) {
    //    prefab.name = GameStaticInfo.NAME_ENEMY + level.ToString();
    //    prefab.tag = GameStaticInfo.TAG_ENTITY;

    //    foreach (Transform t in prefab.transform) //for bubble collider
    //        t.gameObject.tag = prefab.tag;
    //}


    void SpawnEnemyFish(FishLevel lvl, int index, bool isRandomPos = false) {
        GameObject enemyPrefab = GetFishFromPool(lvl, out int globalIndex);
        enemyPrefab.transform.position = GetRandSpawnPosition(lvl, isRandomPos);

        FishControl enemyControl = enemyPrefab.GetComponent<FishControl>();
        Fish enemyFish = _enemyPoolComp.Get(lvl, index);//enemyPrefab.GetComponent<Fish>();
        enemyFish.IsPlayer = false;
        enemyFish.SetRandomProgress();
        enemyControl.Init();

        PrefabNamingTaging(enemyPrefab, enemyFish.Level, GameStaticInfo.NAME_ENEMY, GameStaticInfo.TAG_ENTITY);

        enemyPrefab.SetActive(true);
    }

    //public bool IsPlayerPoolProviderValid() {
    //    if (_playerPoolProvider == null) {
    //        Debug.LogWarning("Player Pool Provider are not initialized! It is not possible to generate spawn positions for enemies; call     public SetPlayerProvider() to resolve the issue.");
    //        return false;
    //    }

    //    return true;
    //}
    public bool IsSpawnPointsValid() {
        if (_weakFishSpawnPointsParent == null || _sharkSpawnPointsParent == null || _sharkSpawnPointsParent == null) {
            Debug.LogWarning("Spawn points are not initialized! It is not possible to select spawn positions; call SetSpawnPoints() to resolve the issue.");
            return false;
        }

        return true;
    }

    private Vector3 GetRandSpawnPosition(FishLevel lvl, bool isRandomPos = false) {//mwx lvl - 9
                                                                                   //const float MAX_DISTANCE_FROM_PLAYER = 200f; //to avoid spawn too far to player
        const float SPAWN_POINTS_WEIGHT = 0.33f; //to get index in range [0; 2] for spawn points
        const float WEIGHT_FIXER = 0.01f; //to control some fish spawn points distribution
        float distanceToPlayer = 0f;
        int currentSpawnPointIndex = (int)((int)lvl * SPAWN_POINTS_WEIGHT + WEIGHT_FIXER);
        //if (!IsPlayerPoolProviderValid())
        //    return Vector3.zero;
        if (!IsSpawnPointsValid())
            return Vector3.zero;

        Vector3 playerPos = _playerPoolProvider.PlayerCurrentFish.transform.position;
        Transform spawnPoints = currentSpawnPointIndex switch {
            0 => _weakFishSpawnPointsParent,
            1 => _strongFishSpawnPointsParent,
            2 => _sharkSpawnPointsParent,
            _ => _weakFishSpawnPointsParent
        };

        int pointsMaxInd = spawnPoints.childCount/*Length*/ - 1;
        int index = (int)(UnityEngine.Random.value * pointsMaxInd);
        if (isRandomPos) //initial spawn in random position
            return spawnPoints.GetChild(index).position + GameStaticInfo.RandomXZOffset();

        //rand spawn position farthest from player by distance
        for (int i = 0; i <= pointsMaxInd; i++) {//max attempts to find suitable spawn point
            index = (int)(UnityEngine.Random.value * pointsMaxInd);
            distanceToPlayer = Vector3.Distance(/*pool[PLAYER_INDEX]*/playerPos, spawnPoints.GetChild(index).position);

            if (distanceToPlayer < GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW/* && distanceToPlayer < MAX_DISTANCE_FROM_PLAYER*/)
                continue;

            return spawnPoints.GetChild(index).position + GameStaticInfo.RandomXZOffset();
        }

        return spawnPoints.GetChild(index).position + GameStaticInfo.RandomXZOffset();
    }

    GameObject GetFishFromPool(/*GameObject alreadyDeadObj,*/ FishLevel level, out int index) {
        index = -1;

        for (int i = 0; i < GameStaticInfo.ENEMY_POOL_SIZE; i++) {
            GameObject obj = _enemyPool.Get(level, i);

            if (!obj.activeInHierarchy /*&& obj != alreadyDeadObj*/
                && !_enemyPoolComp.Get(level, i).IsAlreadySelected) {

                index = i;
                _enemyPoolComp.Get(level, i).IsAlreadySelected = true;

                return obj;
            }
        }
        return null;
    }

    public bool TryGetFish(GameObject fishObject, out Fish fish) {
        fish = null;

        for (FishLevel level = FishLevel.Crab; level <= GameStaticInfo.MAX_FISH_LEVEL; level++)
            for (int i = 0; i < GameStaticInfo.ENEMY_POOL_SIZE; i++)
                if (_enemyPool.Get(level, i) == fishObject) {
                    fish = _enemyPoolComp.Get(level, i);
                    return true;
                }

        return false;
    }

    void FishDieAct(GameObject gameObject, FishLevel level) {
        int prefabIndex = GetObjIndexFromPool(level, gameObject);
        Fish enemyFish = _enemyPoolComp.Get(level, prefabIndex);

        if (prefabIndex >= 0 && enemyFish.IsVisible)
            RespawnEnemy(enemyFish, prefabIndex);
    }

    /*public*/
    int GetObjIndexFromPool(FishLevel level, GameObject obj) {
        for (int i = 0; i < GameStaticInfo.ENEMY_POOL_SIZE; i++) {
            if (_enemyPool.Get(level, i) == obj)
                return i;
        }

        return -1;
        //Array.IndexOf(_enemyPool, obj);
    }

    public void RespawnEnemy(Fish currentFish, int index) {//Attempted to realize spawn by weight respawn
        //spawn die particles
        OnFishDie?.Invoke(currentFish);
        //if (currentFish.IsVisible) {
        //    _vfxPoolManager.ActivateObjectFromPool(FishVFX.SkeletonParticle, _fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
        //    _vfxPoolManager.ActivateObjectFromPool(FishVFX.DeadBubblesParticle, _fishPoolParent, currentFish.transform.localPosition, Vector3.one, Quaternion.identity);
        //}

        //if (IsPlayerPoolProviderValid()) {
        FishLevel currnentRandLvl = GetRandSpawnLevel();
        SpawnEnemyFish(currnentRandLvl, index, false);
        //}
    }

    private FishLevel GetRandSpawnLevel() {
        FishLevel playerLvl = _playerPoolProvider.PlayerCurrentFish.Level;

        //if (playerLvl == null)
        //    return (int)GameStaticInfo.PLAYER_INIT_LVL;
        const int ALLOWED_LVLS_CNT = 3;
        int[] allowedLevels = new int[ALLOWED_LVLS_CNT] {
            Math.Max((int) playerLvl - 1, 0),
            (int) playerLvl,
            Math.Min((int)playerLvl + 1, (int)GameStaticInfo.MAX_FISH_LEVEL)
        };
        float random = UnityEngine.Random.value;//0 to 1
        float currentWeight = 0f;
        float[] spawnWeight = (playerLvl == FishLevel.SeaBass) ?
            SPAWN_WEIGHTS_EASY_START : SPAWN_WEIGHTS_EASY;//easy enemies for beginner players
                                                          //weight by gamemode

        if (GameStaticInfo.Gamemode == GameMode.None) {
            Debug.LogWarning("Activate fast game scene start!");

            if (ProgressSaver.IsWorldDataExist)
                GameStaticInfo.Gamemode = ProgressSaver.LoadWorldData().gameMode;

            if (GameStaticInfo.Gamemode == GameMode.None) {
                GameStaticInfo.Gamemode = GameMode.Easy;
                Debug.LogWarning("Gamemode selection System doesnt work properly!");
            }
        }
        spawnWeight = GameStaticInfo.Gamemode switch {
            GameMode.Easy => spawnWeight,
            GameMode.Medium => SPAWN_WEIGHTS_MEDIUM,
            GameMode.Hard => SPAWN_WEIGHTS_HARD,
            _ => SPAWN_WEIGHTS_EASY,
        };

        for (int i = 0; i < ALLOWED_LVLS_CNT; i++) {
            currentWeight += spawnWeight[i];

            if (random < currentWeight)
                return (FishLevel)allowedLevels[i];
        }

        return (FishLevel)allowedLevels[0];
    }
}
