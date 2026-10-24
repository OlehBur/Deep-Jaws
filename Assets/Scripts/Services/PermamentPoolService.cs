
using System;
using UnityEngine;
using VContainer;

public class PermamentPoolService : PoolServise, IPermamentPoolService {
    private GameObject[] _pufferfishPool;
    private GameObject[] _cuttlefishPool;
    private GameObject[] _dartfishPool;
    private Fish[] _pufferfishPoolComp;
    private Fish[] _cuttlefishPoolComp;
    private Fish[] _dartfishPoolComp;

    private readonly IPlayerPoolService _playerPoolProvider;

    [Inject]
    public PermamentPoolService(FishPrefabsInfo fishPrefabs, IPlayerPoolService playerPoolProvider) : base(fishPrefabs) {
        _playerPoolProvider = playerPoolProvider;
    }

    public void Init() {

    }

    public void InitialSpawnPrefabs() {


    }
    int GetObjIndexFromPool(FishLevel level, GameObject obj) {
    }
    void PrefabNamingTaging(GameObject prefab, FishLevel level) {
    }

    public void PrefabNamingTaging(GameObject prefab, FishLevel level) {
        prefab.name = GameStaticInfo.NAME_PERMAMENT + level.ToString();
        prefab.tag = GameStaticInfo.TAG_ENTITY;

        foreach (Transform t in prefab.transform) //for bubble collider
            t.gameObject.tag = prefab.tag;
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
}
