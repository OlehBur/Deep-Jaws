using UnityEngine;

public class PufferfishController : MonoBehaviour {
    /*[SerializeField]*/
    //[SerializeField] AudioSource audioSource;
    private AudioManager audioManager;
    [SerializeField] Animator animator;
    private const string PUFFERFISH_BLOW_ANIMATION = "PufferfishBlow";
    private const string PUFFERFISH_REVERSE_BLOW_ANIMATION = "PufferfishBlow_reverse";
    private float currentTimeCollision = 0f;
    private const float BITE_INTERVAL = 1f;
    private const int DAMAGE_CNT = 5;
    private bool startedAnimation = false;


    public void Init(AudioManager audioManager) {
        this.audioManager = audioManager;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        //Debug.Log(spawnManager);
        if (/*victimObj != null &&*/ GameStaticInfo.TryGetFish(victimObj, out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && !startedAnimation /*&& !audioSource.isPlaying*/) {

            animator.Play(PUFFERFISH_BLOW_ANIMATION);
            audioManager?.PlayVFX(VFX_clip.Pufferfish);
            startedAnimation = true;
            //audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        if (/*victimObj != null &&*/ GameStaticInfo.TryGetFish(victimObj, out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish && startedAnimation) {
            startedAnimation = false;
            animator.Play(PUFFERFISH_REVERSE_BLOW_ANIMATION);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collision.collider.attachedRigidbody.gameObject;
        if (!collision.collider.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD) 
            /*&& victimObj != null */&& GameStaticInfo.TryGetFish(victimObj, out Fish fish)
            && fish.Level != FishLevel.Pufferfish)
            fish.TakeDamage(DAMAGE_CNT);
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.collider.attachedRigidbody == null)
            return;

        GameObject victimObj = collision.collider.attachedRigidbody.gameObject;
        if (/*victimObj != null &&*/ GameStaticInfo.TryGetFish(victimObj, out Fish fish)
            && fish.Level != FishLevel.Pufferfish) {
            currentTimeCollision += Time.fixedDeltaTime;

            if (currentTimeCollision > BITE_INTERVAL) {
                currentTimeCollision = 0f;
                fish.TakeDamage(DAMAGE_CNT);
            }
        }

    }
}
