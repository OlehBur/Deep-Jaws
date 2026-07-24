using UnityEngine;


[RequireComponent(typeof(Collider))]
public class CantCatchAchieveTrig : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (other.attachedRigidbody.transform.CompareTag("Player")) {
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_they_cant_reach_me_here);
            //Debug.Log("Achievement triggered for CantReachMe");
        }
    }
}
