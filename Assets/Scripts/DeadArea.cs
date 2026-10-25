using UnityEngine;
using VContainer;


// kill and respawn fish that entered the dead area
public class DeadArea : MonoBehaviour {
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;

    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        if (victimObj.layer == LayerMask.NameToLayer("Entity")) {
            Fish fish;
            _spawnOrchestrator.TryGetFish(victimObj, out fish);

            fish.Die();// fish die if it enters the dead area
        }
    }
}
