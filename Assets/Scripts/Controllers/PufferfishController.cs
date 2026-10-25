using UnityEngine;
using VContainer;

public class PufferfishController : MonoBehaviour {
    /*[SerializeField]*/
    //[SerializeField] AudioSource audioSource;
    //private AudioManager audioManager;
    [SerializeField] Animator animator;

    [Inject] private readonly IAudioManager _audioManager;
    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;

    private const string PUFFERFISH_BLOW_ANIMATION = "PufferfishBlow";
    private const string PUFFERFISH_REVERSE_BLOW_ANIMATION = "PufferfishBlow_reverse";
    private const float BITE_INTERVAL = 1f;
    private const int DAMAGE_CNT = 5;

    private float _currentTimeCollision = 0f;
    private bool _startedAnimation = false;



    //public void Init(AudioManager audioManager) {
    //    this.audioManager = audioManager;
    //}

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        //Debug.Log(spawnManager);
        if (/*victimObj != null &&*/ _spawnOrchestrator.TryGetFish(victimObj, out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && !_startedAnimation /*&& !audioSource.isPlaying*/) {

            animator.Play(PUFFERFISH_BLOW_ANIMATION);
            _audioManager?.PlayVFX(VFX_clip.Pufferfish);
            _startedAnimation = true;
            //audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        if (/*victimObj != null &&*/ _spawnOrchestrator.TryGetFish(victimObj, out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && _startedAnimation) {
            _startedAnimation = false;
            animator.Play(PUFFERFISH_REVERSE_BLOW_ANIMATION);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collision.collider.attachedRigidbody.gameObject;
        if (!collision.collider.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD)
            /*&& victimObj != null */&& _spawnOrchestrator.TryGetFish(victimObj, out Fish fish)
            && fish.Level != FishLevel.Pufferfish)
            fish.TakeDamage(DAMAGE_CNT);
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collision.collider.attachedRigidbody.gameObject;
        if (/*victimObj != null &&*/ _spawnOrchestrator.TryGetFish(victimObj, out Fish fish)
            && fish.Level != FishLevel.Pufferfish) {
            _currentTimeCollision += DJTime.FixedDeltaTime;

            if (_currentTimeCollision > BITE_INTERVAL) {
                _currentTimeCollision = 0f;
                fish.TakeDamage(DAMAGE_CNT);
            }
        }

    }
}
