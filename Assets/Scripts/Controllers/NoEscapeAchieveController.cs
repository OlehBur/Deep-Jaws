using UnityEngine;
using VContainer;



[RequireComponent(typeof(Collider))]
public class NoEscapeAchieveController : MonoBehaviour {
    [Inject]private readonly ISpawnOrchestrator _spawnOrchestrator;
    [Inject] private readonly IGPGManager _gpgManager;

    private void OnCollisionEnter(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb.CompareTag("Player") && _spawnOrchestrator.TryGetFish(victimRb.gameObject, out FishController fish)//parent.TryGetComponent(out Fish fish)
            && fish.Level == FishLevel.SeaBass) {
#if UNITY_ANDROID && !UNITY_EDITOR
            _gpgManager.OnTriggerAchievement(GPGSIds.achievement_youre_not_escaping_this);
            //Debug.LogWarning("Achievement triggered for NoEscape");
#endif
        }
    }
}
