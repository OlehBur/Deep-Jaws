using System;
using UnityEngine;

public class SlowdownPearlController : MonoBehaviour {
    [SerializeField] Animator floawerAnimator;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Collider pearlCollider;
    //[SerializeField] Collider flowerCollider;
    [SerializeField] MeshRenderer pearlMeshRenderer;
    [SerializeField] Light pearlLight;
    [SerializeField] GameObject freezeTimeParticles;

    private const string FLOWER_ANIM_NAME = "Nymphae_Flower";
    private const string FLOWER_REVERSE_ANIM_NAME = "Nymphae_Flower_reverse";
    private const float MAX_RESPAWN_TIME = 180f; //3 minutes
    private const float MAX_AXIS_ANGLE_DIR = 45f;

    private Vector3 randomRotateDir;
    private static event Action OnPearlActivate;
    private static event Action OnPearlDeactivate;
    private static bool IsActivationInvokeStarted = false;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        IsActivationInvokeStarted = false; //auto restart every start
        OnPearlActivate = null;
        OnPearlDeactivate = null;
    }

    void Start() {
        //Debug.LogWarning("Changed flower time");
        pearlCollider.enabled = false;

        if (!IsActivationInvokeStarted) {
            Invoke(nameof(ActivatePearlEvent), MAX_RESPAWN_TIME);
            //Debug.LogWarning("ACTivate fast slowPearl inoke" + Time.time);
            IsActivationInvokeStarted = true;
        }
        //flowerCollider.enabled = true;
        pearlMeshRenderer.enabled = false;
        pearlLight.enabled = false;
    }

    void ActivatePearlEvent() => OnPearlActivate?.Invoke();

    // Update is called once per frame
    void Update() {
        if (pearlMeshRenderer.enabled)
            transform.Rotate(randomRotateDir * DJTime.DeltaTime);
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
#if UNITY_ANDROID && !UNITY_EDITOR
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_a_fragile_beauty_in_a_sea_of_decay);
#endif
        }
    }

    private void DeactivatePearl() {
        //Debug.LogWarning("DeacTivate pearl " + gameObject.name);
        //flowerCollider.enabled = true;
        pearlCollider.enabled = false;
        pearlMeshRenderer.enabled = false;
        pearlLight.enabled = false;
        freezeTimeParticles.SetActive(true);

        if (IsActivationInvokeStarted) {
            Invoke(nameof(ActivatePearlEvent), MAX_RESPAWN_TIME);
            //Debug.LogWarning("ACTivate  slowPearl inoke" + Time.time);
            IsActivationInvokeStarted = false;
        }

        floawerAnimator.Play(FLOWER_REVERSE_ANIM_NAME);
    }

    private void ActivatePearl() {
        if (gameObject == null)
            return;
        //Debug.LogWarning("ACTivate pearl " + gameObject.name);
        floawerAnimator.Play(FLOWER_ANIM_NAME);
        randomRotateDir = new Vector3(
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR),
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR),
            UnityEngine.Random.Range(-MAX_AXIS_ANGLE_DIR, MAX_AXIS_ANGLE_DIR));
        //flowerCollider.enabled = false;
        pearlMeshRenderer.enabled = true;
        pearlCollider.enabled = true;
        pearlLight.enabled = true;
    }

    void OnDestroy() {
        IsActivationInvokeStarted = false;
        OnPearlActivate -= ActivatePearl;
        OnPearlDeactivate -= DeactivatePearl;
    }

    private void OnEnable() {
        OnPearlActivate += ActivatePearl;
        OnPearlDeactivate += DeactivatePearl;
    }

    private void OnDisable() {
        OnPearlActivate -= ActivatePearl;
        OnPearlDeactivate -= DeactivatePearl;
    }
}
