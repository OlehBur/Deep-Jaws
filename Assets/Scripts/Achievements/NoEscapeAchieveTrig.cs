using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveTrig : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb.CompareTag("Player") && GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
#if UNITY_ANDROID && !UNITY_EDITOR
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_not_escaping_this);
            //Debug.LogWarning("Achievement triggered for NoEscape");
#endif
        }
    }
}
