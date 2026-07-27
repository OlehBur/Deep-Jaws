using System;
using UnityEngine;

public class SlowdownPearlController : MonoBehaviour {
    [SerializeField] Animator floawerAnimator;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Collider pearlCollider;
    [SerializeField] MeshRenderer pearlMeshRenderer;
    [SerializeField] Light pearlLight;
    [SerializeField] GameObject freezeTimeParticles;

    private const string FLOWER_ANIM_NAME = "Nymphae_Flower";
    private const string FLOWER_REVERSE_ANIM_NAME = "Nymphae_Flower_reverse";
    private const float MAX_RESPAWN_TIME = 120; //2 minutes
    private const float MAX_AXIS_ANGLE_DIR = 45f;

    private Vector3 randomRotateDir;
    private static event Action OnPearlActivate;
    private static event Action OnPearlDeactivate;
    private static bool IsActivationInvokeStarted = false;


    void Start() {
        //Debug.LogWarning("Changed flower time");
        pearlCollider.enabled = false;

        if (!IsActivationInvokeStarted) {
            Invoke(nameof(ActivatePearlEvent), MAX_RESPAWN_TIME);
            IsActivationInvokeStarted = true;
        }

        OnPearlActivate += ActivatePearl;
        OnPearlDeactivate += DeactivatePearl;

        //gameObject.SetActive(false);
        pearlMeshRenderer.enabled = false;
        pearlLight.enabled = false;
    }

    void ActivatePearlEvent() => OnPearlActivate?.Invoke();

    // Update is called once per frame
    void Update() {
        if (pearlMeshRenderer.enabled)
            transform.Rotate(randomRotateDir * Time.deltaTime);
    }

    private void OnTriggerStay(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform parentFish = other.attachedRigidbody.transform;

        if (parentFish.CompareTag("Player") && !other.isTrigger && parentFish.TryGetComponent(out FishAnim fam)) {
            fam.ActivateBiteAnimation(false);
            audioManager.PlayVFX(VFX_clip.FreezeTime);
            //pearlAudioSource.PlayOneShot(pearlSoundClip);        
            OnPearlDeactivate?.Invoke();

            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_a_fragile_beauty_in_a_sea_of_decay);
        }
    }

    private void DeactivatePearl() {
        pearlCollider.enabled = false;
        //gameObject.SetActive(false);
        pearlMeshRenderer.enabled = false;
        pearlLight.enabled = false;
        freezeTimeParticles.SetActive(true);

        if (IsActivationInvokeStarted) {
            Invoke(nameof(ActivatePearlEvent), MAX_RESPAWN_TIME);
            IsActivationInvokeStarted = false;
        }

        floawerAnimator.Play(FLOWER_REVERSE_ANIM_NAME);
    }

    private void ActivatePearl() {
        //floawerAnimator.speed = 1f;
        floawerAnimator.Play(FLOWER_ANIM_NAME);
        randomRotateDir = new Vector3(
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR), 
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR), 
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR));
        //gameObject.SetActive(true);
        pearlMeshRenderer.enabled = true;
        pearlCollider.enabled = true;
        pearlLight.enabled = true;
    }
}
