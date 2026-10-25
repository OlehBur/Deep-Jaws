using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFishPoolService : IEnumerable<Fish> {
    event Action<Fish> OnFishDie;

    int Count { get; }
    Fish GetAt(int index);

    void Init(Transform PoolParent);
    void InitialSpawnPrefabs(/*FishLevel playerLvl*/);
    //int GetObjIndexFromPool(FishLevel level, GameObject obj);
    bool TryGetFish(GameObject fishObject, out Fish fish);
    //Fish GetFishByIndex(int index);
    //void PrefabNamingTaging(GameObject prefab, FishLevel level);
}

public interface IPlayerFishPoolService : IFishPoolService {
    Fish PlayerCurrentFish { get; }
    event Action<Fish, bool> OnPlayerRespawn;
    //event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void SavePlayerData();
    public void RespawnPlayer(bool isDie, bool isRevive = false);
}

public interface IEnemyFishPoolService : IFishPoolService {
    void RespawnAllEnemies();
    void RespawnEnemy(Fish currentFish, int index);
    //void SetPlayerProvider(IPlayerPoolService _playerSpawnService);
    void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent, Transform sharkSpawnPointsParent);
    //bool IsPlayerPoolProviderValid();
    bool IsSpawnPointsValid();
}

public interface IPermamentFishPoolService : IFishPoolService {

}