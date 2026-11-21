using UnityEngine;



public class GroundbaitController : MonoBehaviour {
    private ISpawnOrchestrator _spawnOrchestrator;


    public void Init(ISpawnOrchestrator spawnOrchestrator) {
        _spawnOrchestrator = spawnOrchestrator;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (gameObject.activeInHierarchy && _spawnOrchestrator != null
            && _spawnOrchestrator.IsCollideWithFishMouth(other, out FishStatsController fish)) {
            fish.FeedGroundbait();

            gameObject.SetActive(false);
        }
    }
}
