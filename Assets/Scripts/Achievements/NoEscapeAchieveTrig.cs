using UnityEngine;
using VContainer;



[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveTrig : MonoBehaviour {
    [Inject]private readonly ISpawnOrchestrator _spawnOrchestrator;


    private void OnCollisionEnter(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb.CompareTag("Player") && _spawnOrchestrator.TryGetFish(victimRb.gameObject, out Fish fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
#if UNITY_ANDROID && !UNITY_EDITOR
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_youre_not_escaping_this);
            //Debug.LogWarning("Achievement triggered for NoEscape");
#endif
        }
    }
}
