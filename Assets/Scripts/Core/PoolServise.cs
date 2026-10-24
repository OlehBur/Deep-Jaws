using UnityEngine;
using VContainer;



public abstract class PoolServise {
    protected IObjectResolver _resolver;
    protected WorldData _savedWorldData;
    protected Transform _fishPoolParent;

    protected /*readonly*/ FishPrefabsInfo _fishPrefabs;

    public PoolServise(FishPrefabsInfo fishPrefabs) {
        _fishPrefabs = fishPrefabs;
    }

    public void PrefabNamingTaging(GameObject prefab, FishLevel level, string mainName, string tag) {
        prefab.name = mainName + level.ToString();
        prefab.tag = tag;

        foreach (Transform t in prefab.transform) //for bubble collider
            t.gameObject.tag = prefab.tag;
    }
}