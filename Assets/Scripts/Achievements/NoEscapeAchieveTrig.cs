using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveTrig : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb.CompareTag("Player") && GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_not_escaping_this);
            //Debug.LogWarning("Achievement triggered for NoEscape");
        }
    }
}
