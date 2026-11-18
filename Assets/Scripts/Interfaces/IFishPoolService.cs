using System;
using System.Collections.Generic;
using UnityEngine;


public interface IFishPoolService : IEnumerable<FishController> {
    event Action<FishController> OnFishDie;
    event Action OnPoolInitialSpawnCompleted;

    bool IsInitialSpawnCompleted { get; }
    int Count { get; }
    FishController GetAt(int index);

    void Init(Transform PoolParent);
    void InitialSpawnPrefabs(/*FishLevel playerLvl*/);
    //int GetObjIndexFromPool(FishLevel level, GameObject obj);
    bool TryGetFish(GameObject fishObject, out FishController fish);
    //Fish GetFishByIndex(int index);
    //void PrefabNamingTaging(GameObject prefab, FishLevel level);

    void OnDestroy();
}

