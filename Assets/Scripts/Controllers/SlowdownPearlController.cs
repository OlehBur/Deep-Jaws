using UnityEngine;

public class SlowdownPearlController : MonoBehaviour {
    [SerializeField] Animator floawerAnimator;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Collider pearlCollider;
    [SerializeField] MeshRenderer pearlMeshRenderer;
    [SerializeField] Light pearlLight;
    [SerializeField] GameObject freezeTimeParticles;
    const string FLOWER_ANIM_NAME = "Nymphae_Flower";
    const string FLOWER_REVERSE_ANIM_NAME = "Nymphae_Flower_reverse";
    private const float MAX_RESPAWN_TIME = 120; //2 minutes
    const float MAX_AXIS_ANGLE_DIR = 45f;
    private Vector3 randomRotateDir;

    void Start() {
        //Debug.LogWarning("Changed flower time");
        pearlCollider.enabled = false;
        Invoke(nameof(ActivatePearl), MAX_RESPAWN_TIME);
        //gameObject.SetActive(false);
        pearlMeshRenderer.enabled = false;
        pearlLight.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        if (pearlMeshRenderer.enabled)
            transform.Rotate(randomRotateDir * Time.deltaTime);
    }

    private void OnTriggerStay(Collider other) {
        Transform parentFish = other.transform.parent;

        if (parentFish.CompareTag("Player") && !other.isTrigger && parentFish.TryGetComponent(out FishAnimManager fam)) {
            fam.ActivateBiteAnimation(false);
            audioManager.PlayVFX(VFX_clip.FreezeTime);
            //pearlAudioSource.PlayOneShot(pearlSoundClip);
            pearlCollider.enabled = false;
            //gameObject.SetActive(false);
            pearlMeshRenderer.enabled = false;
            pearlLight.enabled = false;
            freezeTimeParticles.SetActive(true);
            Invoke(nameof(ActivatePearl), MAX_RESPAWN_TIME);
            floawerAnimator.Play(FLOWER_REVERSE_ANIM_NAME);

            GPG_Manager.OnTriggerAchievement(AchievementKey.Beauty);
        }
    }

    private void ActivatePearl() {
        //floawerAnimator.speed = 1f;
        floawerAnimator.Play(FLOWER_ANIM_NAME);
        randomRotateDir = new Vector3(Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR), Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR), Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR));
        //gameObject.SetActive(true);
        pearlMeshRenderer.enabled = true;
        pearlCollider.enabled = true;
        pearlLight.enabled = true;
    }
}
