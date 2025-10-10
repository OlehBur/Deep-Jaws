using UnityEngine;

public class TestTriggCollider : MonoBehaviour
{

    private void OnTriggerEnter(Collider other) {
        Debug.Log("OnTriggerEnter: " + other.name);
    }
}
