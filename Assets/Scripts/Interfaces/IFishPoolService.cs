using System;
using System.Collections.Generic;
using UnityEngine;


public interface IFishPoolService : IEnumerable<FishStatsController> {
    event Action<FishStatsController> OnFishDie;
    event Action OnPoolInitialSpawnCompleted;

    bool IsInitialSpawnCompleted { get; }
    int Count { get; }
    FishStatsController GetAt(int index);

    void Init(Transform PoolParent);
    void InitialSpawnPrefabs(/*FishLevel playerLvl*/);
    //int GetObjIndexFromPool(FishLevel level, GameObject obj);
    bool TryGetFish(GameObject fishObject, out FishStatsController fish);
    //Fish GetFishByIndex(int index);
    //void PrefabNamingTaging(GameObject prefab, FishLevel level);

    void OnDestroy();
}

