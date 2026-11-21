using UnityEngine;

public interface ISpawnOrchestrator {
    bool TryGetFish(GameObject fishObject, out FishStatsController fish);
    bool IsCollideWithFishMouth(Collider possibleMouth, out FishStatsController fish);
}