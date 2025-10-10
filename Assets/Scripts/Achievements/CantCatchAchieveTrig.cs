using UnityEngine;


[RequireComponent(typeof(Collider))]
public class CantCatchAchieveTrig : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent.CompareTag("Player")) {
            GPG_Manager.OnTriggerAchievement(AchievementKey.CantReachMe);
            //Debug.Log("Achievement triggered for CantReachMe");
        }
    }
}
