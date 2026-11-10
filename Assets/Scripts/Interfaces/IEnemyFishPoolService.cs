using UnityEngine;


public interface IEnemyFishPoolService : IFishPoolService {
    void RespawnAllEnemies();
    void RespawnEnemy(Fish currentFish, int index);
    //void SetPlayerProvider(IPlayerPoolService _playerSpawnService);
    void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent, Transform sharkSpawnPointsParent);
    //bool IsPlayerPoolProviderValid();
    bool IsSpawnPointsValid();
}
