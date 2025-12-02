using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveTrig : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        //Transform parent = collision.collider.transform.parent;
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb.CompareTag("Player") && GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
            GPG_Manager.OnTriggerAchievement(AchievementKey.NoEscape);
            //Debug.LogWarning("Achievement triggered for NoEscape");
        }
    }
}
