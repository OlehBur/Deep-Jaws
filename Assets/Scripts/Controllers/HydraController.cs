using UnityEngine;
using VContainer;



//only for scale animation during a poison attack
public class HydraController : MonoBehaviour {
    [SerializeField] Animator animator;

    private const string ANIMATION_NAME = "HydraBite";

    [Inject] private readonly IAudioPlayService _audioManager;
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;


    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (!other.isTrigger && _spawnOrchestrator.IsCollideWithFishMouth(other, out FishStatsController fish)
            && fish.IsVisible) {

            animator.Play(ANIMATION_NAME);

            if (fish.IsPlayer)
                _audioManager.PlayVFX(VFX_clip.Hydra);
        }
    }
}
