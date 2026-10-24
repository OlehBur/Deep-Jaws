using System;
using UnityEngine;

public interface IFishPoolService {
    void Init(Transform PoolParent);
    void InitialSpawnPrefabs(/*FishLevel playerLvl*/);
    //int GetObjIndexFromPool(FishLevel level, GameObject obj);
    bool TryGetFish(GameObject fishObject, out Fish fish);
    //Fish GetFishByIndex(int index);
    //void PrefabNamingTaging(GameObject prefab, FishLevel level);
}

public interface IPlayerPoolService : IFishPoolService {
    Fish PlayerCurrentFish { get; }
    event Action<Fish, bool> OnPlayerRespawn;
    event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void SavePlayerData();
}

public interface IEnemyPoolService : IFishPoolService {
    void RespawnAllEnemies();
    void RespawnEnemy(Fish currentFish, int index);
    //void SetPlayerProvider(IPlayerPoolService _playerSpawnService);
    void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent, Transform sharkSpawnPointsParent);
    //bool IsPlayerPoolProviderValid();
    bool IsSpawnPointsValid();
}

public interface IPermamentPoolService : IFishPoolService {


}