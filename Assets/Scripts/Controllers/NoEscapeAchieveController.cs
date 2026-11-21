using UnityEngine;
using VContainer;



[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveController : MonoBehaviour {
    [Inject]private readonly ISpawnOrchestrator _spawnOrchestrator;
    [Inject] private readonly IGPGManager _gpgManager;

    private void OnCollisionEnter(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (_spawnOrchestrator.IsCollideWithFishMouth(collision.collider, out FishStatsController fish)
            && fish.Level == FishLevel.SeaBass) {
#if UNITY_ANDROID && !UNITY_EDITOR
            _gpgManager.OnTriggerAchievement(GPGSIds.achievement_youre_not_escaping_this);
            //Debug.LogWarning("Achievement triggered for NoEscape");
#endif
        }
    }
}
