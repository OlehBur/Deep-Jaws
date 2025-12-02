using UnityEngine;


// kill and respawn fish that entered the dead area
public class DeadArea : MonoBehaviour {
    //[SerializeField] SpawnManager spawnManager;

    private void OnTriggerExit(Collider other) {
        //Transform parent = other.transform.parent;
        Rigidbody victimRb = other.attachedRigidbody;

        if (victimRb.gameObject.layer == LayerMask.NameToLayer("Entity")) {
            Fish fish; /*=other.transform.parent.GetComponent<Fish>();*/
            GameStaticInfo.TryGetFish(victimRb.gameObject, out fish);

            fish.Die();// fish die if it enters the dead area
            //spawnManager.DeactivatePrefab(other.transform.parent.gameObject); // respawn the fish
        }
    }
}
