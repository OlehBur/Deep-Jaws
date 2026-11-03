
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PermamentPoolService : PoolServise, IPermamentFishPoolService {
    private GameObject[] _pufferfishPool;
    private GameObject[] _cuttlefishPool;
    private GameObject[] _dartfishPool;
    private Fish[] _pufferfishPoolComp;
    private Fish[] _cuttlefishPoolComp;
    private Fish[] _dartfishPoolComp;
    private Transform _weakFishSpawnPointsParent;
    private Transform _strongFishSpawnPointsParent;
    private Transform _sharkSpawnPointsParent;
    private Transform _pufferfishPointsParent;
    private Transform _cuttlefishPointsParent;

    public event Action<Fish> OnFishDie;
    public event Action OnPoolInitialSpawnCompleted;
    public bool IsInitialSpawnCompleted { get; private set; } = false;

    private readonly IPlayerFishPoolService _playerPoolProvider;

    public int Count => _pufferfishPoolComp == null ?
        0 : _pufferfishPoolComp.Length + _cuttlefishPoolComp.Length + _dartfishPoolComp.Length;


    [Inject]
    public PermamentPoolService(FishPrefabsInfo fishPrefabs, IPlayerFishPoolService playerPoolProvider, IObjectResolver resolver) : base(fishPrefabs, resolver) {
        _playerPoolProvider = playerPoolProvider;
    }

    public Fish GetAt(int index) {
        if (index < _pufferfishPoolComp.Length)
            return _pufferfishPoolComp[index];

        index -= _pufferfishPoolComp.Length;

        if (index < _cuttlefishPoolComp.Length)
            return _cuttlefishPoolComp[index];

        index -= _cuttlefishPoolComp.Length;

        return _dartfishPoolComp[index];
    }

    public IEnumerator<Fish> GetEnumerator() {
        foreach (Fish f in _pufferfishPoolComp)
            yield return f;

        foreach (Fish f in _cuttlefishPoolComp)
            yield return f;

        foreach (Fish f in _dartfishPoolComp)
            yield return f;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Init(Transform fishPoolParent) {
        _fishPoolParent = fishPoolParent;

        _pufferfishPool = new GameObject[GameStaticInfo.PUFFERFISH_SPAWN_COUNT];
        _pufferfishPoolComp = new Fish[GameStaticInfo.PUFFERFISH_SPAWN_COUNT];
        _cuttlefishPool = new GameObject[GameStaticInfo.CUTTLEFISH_SPAWN_COUNT];
        _cuttlefishPoolComp = new Fish[GameStaticInfo.CUTTLEFISH_SPAWN_COUNT];
        _dartfishPool = new GameObject[GameStaticInfo.DARTFISH_SPAWN_COUNT];
        _dartfishPoolComp = new Fish[GameStaticInfo.DARTFISH_SPAWN_COUNT];
    }

    public void InitialSpawnPrefabs() {
        //pufferfish
        InitialSpawnPermamentFish(_fishPrefabs.Pufferfish, _pufferfishPointsParent, _pufferfishPool, _pufferfishPoolComp, GameStaticInfo.PUFFERFISH_SPAWN_COUNT);
        //spawn cuttlefish
        InitialSpawnPermamentFish(_fishPrefabs.Cuttlefish, _cuttlefishPointsParent, _cuttlefishPool, _cuttlefishPoolComp, GameStaticInfo.CUTTLEFISH_SPAWN_COUNT);
        //dartfish
        Transform spawnPoints = GetRandSpawnPointsByFishLevel(_playerPoolProvider.PlayerCurrentFish.Level);
        InitialSpawnPermamentFish(_fishPrefabs.Dartfish, spawnPoints, _dartfishPool, _dartfishPoolComp, GameStaticInfo.DARTFISH_SPAWN_COUNT);
    }

    public void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent, Transform sharkSpawnPointsParent,
        Transform pufferfishPointsParent, Transform cuttlefishPointsParent) {

        _weakFishSpawnPointsParent = weakFishSpawnPointsParent;
        _strongFishSpawnPointsParent = strongFishSpawnPointsParent;
        _sharkSpawnPointsParent = sharkSpawnPointsParent;
        _pufferfishPointsParent = pufferfishPointsParent;
        _cuttlefishPointsParent = cuttlefishPointsParent;
    }

    /// <summary>
    /// for permanent fish that can spawn in any biome
    /// </summary>
    /// <param name="playerLvl"></param>
    /// <returns></returns>
    Transform GetRandSpawnPointsByFishLevel(FishLevel playerLvl) {
        Transform spawnPoints;

        switch (playerLvl) {
            case FishLevel.SeaBass:
            case FishLevel.Mackerel:
            spawnPoints = _weakFishSpawnPointsParent;
            break;
            case FishLevel.Moray:
            case FishLevel.Lionfish:
            case FishLevel.Barracuda:
            spawnPoints = _strongFishSpawnPointsParent;
            break;
            case FishLevel.HammerShark:
            case FishLevel.TigerShark:
            case FishLevel.WhiteShark:
            spawnPoints = _sharkSpawnPointsParent;
            break;
            default:
            spawnPoints = _weakFishSpawnPointsParent;
            break;
        }

        return spawnPoints;
    }

    public void InitialSpawnPermamentFish(GameObject prefab, Transform spPoints, GameObject[] pool, Fish[] fishCompPool, int maxCnt) {
        int currentInd = 0;
        int pointsMaxInd = spPoints.childCount - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);

        while (currentInd < maxCnt) {
            if (Vector3.Distance(_playerPoolProvider.PlayerCurrentFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                GameObject prefabInst = pool[currentInd] = _resolver.Instantiate(prefab, spPoints.GetChild(spawnPointIndex).position /*+ RandomXZOffset()*/, Quaternion.identity, _fishPoolParent);
                FishControl enemyPermControl = prefabInst.GetComponent<FishControl>();
                Fish fishPermament = enemyPermControl.Fish;

                //if (prefabInst.TryGetComponent(out PufferfishController pufferController))
                //    pufferController.Init(audioManager);
                //else if (prefabInst.TryGetComponent(out СuttlefishController cuttleController))
                //    cuttleController.Init(audioManager, fishAdditionalComponentManager);
                PrefabNamingTaging(prefabInst, fishPermament.Level, GameStaticInfo.NAME_PERMAMENT, GameStaticInfo.TAG_ENTITY);
                enemyPermControl.Init();
                fishPermament.SetRandomProgress();
                fishPermament.OnFishDie += DeactivateRespawnPrefab;
                fishCompPool[currentInd] = fishPermament;
                pool[currentInd].SetActive(true);

                ++currentInd;
            }
            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsMaxInd);//(spawnPointIndex + 1 == spPoints.Length) ? 0 : ++spawnPointIndex;
        }

        IsInitialSpawnCompleted = true;
        OnPoolInitialSpawnCompleted?.Invoke();
    }

    public bool IsSpawnPointsValid() {
        if (_weakFishSpawnPointsParent == null || _sharkSpawnPointsParent == null
            || _sharkSpawnPointsParent == null || _cuttlefishPointsParent == null || _pufferfishPointsParent == null) {
            Debug.LogWarning("Spawn points are not initialized! It is not possible to select spawn positions; call SetSpawnPoints() to resolve the issue.");
            return false;
        }

        return true;
    }

    void RespawnPermamentFish(int index, GameObject[] pool, Fish[] fishCompPool, Transform spPoints) {
        if (!IsSpawnPointsValid())
            return;

        int pointsCnt = spPoints.childCount - 1;
        int spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        GameObject prefabInst = pool[index];
        Fish fish = fishCompPool[index];

        OnFishDie?.Invoke(fish);
        //spawn dead particles
        //if (fish.IsVisible) {
        //    fishAdditionalComponentManager.ActivateObjectFromPool(FishVFX.SkeletonParticle, fishPoolParent, fish.transform.localPosition, Vector3.one, Quaternion.identity);
        //    fishAdditionalComponentManager.ActivateObjectFromPool(FishVFX.DeadBubblesParticle, fishPoolParent, fish.transform.localPosition, Vector3.one, Quaternion.identity);
        //}

        fish.Heal(fish./*MaxHealth*/DefaultInfo.MaxHealth);//restore init parameters 
        fish.SetRandomProgress();
        prefabInst.SetActive(true);

        for (int attempt = 0; attempt <= pool.Length; attempt++) {
            if (Vector3.Distance(_playerPoolProvider.PlayerCurrentFish.transform.position, spPoints.GetChild(spawnPointIndex).position) > GameStaticInfo.PLAYER_MAX_DISTANCE_VIEW) {
                pool[index].transform.position = spPoints.GetChild(spawnPointIndex).position;

                return;
            }

            //try new rand spawn point
            spawnPointIndex = (int)(UnityEngine.Random.value * pointsCnt);
        }

    }

    private void DeactivateRespawnPrefab(GameObject prefab, FishLevel level) {
        int visIndex = 0;
        //if (isPlayer)
        //Debug.LogWarning("Player died on level: " + level);

        switch (level) {
            case FishLevel.Pufferfish:
            visIndex = Array.IndexOf(_pufferfishPool, prefab);
            if (visIndex < 0)// if prefab not in pool, exit from method
                return;
            RespawnPermamentFish(visIndex, _pufferfishPool, _pufferfishPoolComp, _pufferfishPointsParent);
            break;

            case FishLevel.Cuttlefish:
            visIndex = Array.IndexOf(_cuttlefishPool, prefab);
            if (visIndex < 0)
                return;
            RespawnPermamentFish(visIndex, _cuttlefishPool, _cuttlefishPoolComp, _cuttlefishPointsParent);
            break;

            case FishLevel.Dartfish:
            visIndex = Array.IndexOf(_dartfishPool, prefab);
            if (visIndex < 0)
                return;
            Transform spawnPoints = GetRandSpawnPointsByFishLevel(_playerPoolProvider.PlayerCurrentFish.Level);
            RespawnPermamentFish(visIndex, _dartfishPool, _dartfishPoolComp, spawnPoints);
            break;
        }
    }

    public bool TryGetFish(GameObject fishObject, out Fish fish) {
        fish = null;
        if (TryGetPermamentFish(_pufferfishPool, _pufferfishPoolComp, fishObject, out fish)
            || TryGetPermamentFish(_dartfishPool, _dartfishPoolComp, fishObject, out fish)
            || TryGetPermamentFish(_cuttlefishPool, _cuttlefishPoolComp, fishObject, out fish))
            return true;

        return false;
    }

    bool TryGetPermamentFish(GameObject[] arrayObj, Fish[] arrayComp, GameObject fishObject, out Fish fish) {
        int index = Array.IndexOf(arrayObj, fishObject);
        if (index < 0) {
            fish = null;
            return false;
        }
        else {
            fish = arrayComp[index];
            return true;
        }
    }

    public void OnDestroy() {
        foreach (Fish f in _pufferfishPoolComp)
            f.OnFishDie -= DeactivateRespawnPrefab;
        foreach (Fish f in _cuttlefishPoolComp)
            f.OnFishDie -= DeactivateRespawnPrefab;
        foreach (Fish f in _dartfishPoolComp)
            f.OnFishDie -= DeactivateRespawnPrefab;
    }
}
