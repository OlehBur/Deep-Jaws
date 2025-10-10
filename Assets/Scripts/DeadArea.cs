using UnityEngine;


// kill and respawn fish that entered the dead area
public class DeadArea : MonoBehaviour {
    //[SerializeField] SpawnManager spawnManager;

    private void OnTriggerExit(Collider other) {
        Transform parent = other.transform.parent;
        if (parent.gameObject.layer == LayerMask.NameToLayer("Entity")) {
            Fish fish; /*=other.transform.parent.GetComponent<Fish>();*/
            GameStaticInfo.TryGetFish(parent.gameObject, out fish);

            fish.Die();// fish die if it enters the dead area
            //spawnManager.DeactivatePrefab(other.transform.parent.gameObject); // respawn the fish
        }
    }
}
