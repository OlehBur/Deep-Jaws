using System;


public interface IPlayerFishPoolService : IFishPoolService {
    FishController PlayerCurrentFish { get; }
    event Action<FishController, bool> OnPlayerRespawn;
    //event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void SavePlayerData();
    void RespawnPlayer(bool isDie, bool isRevive = false);
}