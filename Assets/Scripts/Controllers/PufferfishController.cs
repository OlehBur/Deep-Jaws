using UnityEngine;

public class PufferfishController : MonoBehaviour {
    /*[SerializeField]*/
    //[SerializeField] AudioSource audioSource;
    [SerializeField] Animator animator;
    private const string PUFFERFISH_BLOW_ANIMATION = "PufferfishBlow";
    private const string PUFFERFISH_REVERSE_BLOW_ANIMATION = "PufferfishBlow_reverse";
    private float currentTimeCollision = 0f;
    private const float BITE_INTERVAL = 1f;
    private const int DAMAGE_CNT = 5;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start() {
    //    Fish fish = GetComponent<Fish>();
    //    fish.SetRandomProgress();
    //    GetComponent<EnemyControl>().Init(spawnManager);
    //}

    // Update is called once per frame
    //void Update() {
    //    if (Vector3.Distance(spawnManager.PlayerFish.transform.position, transform.position) return;
    //}

    private void OnTriggerEnter(Collider other) {
        Transform parent = other.transform.parent;

        //Debug.Log(spawnManager);
        if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)//other.transform.parent.TryGetComponent(out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish /*&& !audioSource.isPlaying*/) {

            animator.Play(PUFFERFISH_BLOW_ANIMATION);
            //audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private void OnTriggerExit(Collider other) {
        Transform parent = other.transform.parent;

        if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)//parent.TryGetComponent<Fish>(out Fish fish)
            && fish.IsVisible && fish.Level != FishLevel.Pufferfish)
            animator.Play(PUFFERFISH_REVERSE_BLOW_ANIMATION);
    }

    private void OnCollisionEnter(Collision collision) {
        Transform parent = collision.collider.transform.parent;

        if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)//parent.TryGetComponent<Fish>(out Fish fish) 
            && fish.Level != FishLevel.Pufferfish)
            fish.TakeDamage(DAMAGE_CNT);
    }

    private void OnCollisionStay(Collision collision) {
        Transform parent = collision.collider.transform.parent;

        if (parent != null && GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)// parent.TryGetComponent<Fish>(out Fish fish) 
            && fish.Level != FishLevel.Pufferfish) {
            currentTimeCollision += Time.fixedDeltaTime;

            if (currentTimeCollision > BITE_INTERVAL) {
                currentTimeCollision = 0f;
                fish.TakeDamage(DAMAGE_CNT);
            }
        }

    }
}
