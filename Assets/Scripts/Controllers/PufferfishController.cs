using UnityEngine;
using VContainer;

public class PufferfishController : MonoBehaviour {
    [SerializeField] Animator animator;

    [Inject] private readonly IAudioPlayService _audioManager;
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;

    private const string PUFFERFISH_BLOW_ANIMATION = "PufferfishBlow";
    private const string PUFFERFISH_REVERSE_BLOW_ANIMATION = "PufferfishBlow_reverse";
    private const float BITE_INTERVAL = 1f;

    private float _currentTimeCollision = 0f;
    private bool _startedAnimation = false;



    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        //Debug.Log(spawnManager);
        if (_spawnOrchestrator.IsCollideWithFishMouth(other, out FishStatsController fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && !_startedAnimation /*&& !audioSource.isPlaying*/) {

            animator.Play(PUFFERFISH_BLOW_ANIMATION);
            _audioManager?.PlayVFX(VFX_clip.Pufferfish);
            _startedAnimation = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (_spawnOrchestrator.IsCollideWithFishMouth(other, out FishStatsController fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && _startedAnimation) {
            _startedAnimation = false;
            animator.Play(PUFFERFISH_REVERSE_BLOW_ANIMATION);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        if (!collision.collider.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD)
             && _spawnOrchestrator.IsCollideWithFishMouth(collision.collider, out FishStatsController fish)
            && fish.Level != FishLevel.Pufferfish)
            fish.TakeDamage(GameStaticInfo.PUFFERFISH_DAMAGE);
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        if (_spawnOrchestrator.IsCollideWithFishMouth(collision.collider, out FishStatsController fish)
            && fish.Level != FishLevel.Pufferfish) {
            _currentTimeCollision += DJTime.FixedDeltaTime;

            if (_currentTimeCollision > BITE_INTERVAL) {
                _currentTimeCollision = 0f;
                fish.TakeDamage(GameStaticInfo.PUFFERFISH_DAMAGE);
            }
        }

    }
}
