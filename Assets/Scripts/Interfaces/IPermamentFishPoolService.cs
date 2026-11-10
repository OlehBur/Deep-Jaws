using UnityEngine;


public interface IPermamentFishPoolService : IFishPoolService {
    void SetSpawnPoints(Transform weakFishSpawnPointsParent, Transform strongFishSpawnPointsParent,
            Transform sharkSpawnPointsParent, Transform pufferfishPointsParent, Transform cuttlefishPointsParent);
    bool IsSpawnPointsValid();
}