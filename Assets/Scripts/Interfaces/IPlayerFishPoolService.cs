using System;


public interface IPlayerFishPoolService : IFishPoolService {
    FishStatsController PlayerCurrentFish { get; }
    event Action<FishStatsController, bool> OnPlayerRespawn;
    //event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void SavePlayerData();
    void RespawnPlayer(bool isDie, bool isRevive = false);
}