using System;


public interface IPlayerFishPoolService : IFishPoolService {
    Fish PlayerCurrentFish { get; }
    event Action<Fish, bool> OnPlayerRespawn;
    //event Action OnPlayerDied;
    event Action<FishLevel> OnPlayerLVLup;

    void SavePlayerData();
    void RespawnPlayer(bool isDie, bool isRevive = false);
}