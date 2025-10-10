using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveTrig : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        Transform parent = collision.collider.transform.parent;

        if (parent.CompareTag("Player") && GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
            GPG_Manager.OnTriggerAchievement(AchievementKey.NoEscape);
            //Debug.LogWarning("Achievement triggered for NoEscape");
        }
    }
}
