using Unity.VisualScripting;
using UnityEngine;

public class SonicPulseControl : MonoBehaviour {
    [SerializeField] Light lightIndicator;
    [SerializeField] Material blinkIndicatorMat;
    [SerializeField] Transform pulseTransform;
    //[SerializeField] AudioSource pulseAudioSource;
    [SerializeField] AudioManager audioManager;
    [SerializeField] Collider triggerCollider;
    [SerializeField] SpawnManager spawnManager;
    //private const float MAX_SCALE = 200;
    //private const float PULSE_SCALE_MULTIPLIER = 2f;
    private float curentRechargeTime = 0f; // current recharge time for pulse
    private const float PULSE_RECHARGE_TIME = 30f; // time to recharge the pulse
    private bool playAnim = false;


    private void Update() {
        if (!triggerCollider.enabled) {
            curentRechargeTime = curentRechargeTime - Time.deltaTime; // decrease recharge time

            if (curentRechargeTime <= 0f)
                triggerCollider.enabled = true;
        }

        IndicatorBlinking();
        if (playAnim)
            PulseScalingAnim();

    }

    void PulseScalingAnim() {
        pulseTransform.localScale = pulseTransform.localScale + Vector3.one * GameStaticInfo.STUN_SONAR_SPEED * Time.deltaTime;

        if (pulseTransform.localScale.x >= GameStaticInfo.STUN_SONAR_MAX_SCALE) {
            playAnim = false;
            pulseTransform.localScale = Vector3.zero;
        }
    }

    void IndicatorBlinking() {
        const float EMISSION_SCALE = 2f;
        const float BLINKING_SPEED = 3f;
        float isBlinking = (curentRechargeTime <= 0f) ?
            1f : 0f; // if pulse is ready to use, then blink

        float progress = Mathf.Sin(Time.time * BLINKING_SPEED) * .5f + .5f; //[0;1]

        lightIndicator.color = Color.red * progress * isBlinking;
        blinkIndicatorMat.SetColor("_EmissionColor", Color.red * progress * EMISSION_SCALE * isBlinking);
    }

    private void OnTriggerStay(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform fishParent = other.attachedRigidbody.transform;

        if (triggerCollider.enabled && !other.isTrigger /*&& GameManager.IsSceneReady*/ && Vector3.Distance(spawnManager.PlayerFish.transform.position, transform.position) < GameStaticInfo.PLAYER_MAX_DISTANCE_ACTIVITY
            && fishParent.TryGetComponent(out EffectManager fem) && !playAnim && curentRechargeTime <= 0f) {
            playAnim = true;
            curentRechargeTime = PULSE_RECHARGE_TIME; // reset recharge time
            triggerCollider.enabled = false;
            //Debug.Log("Bingo");
            //if (!pulseAudioSource.isPlaying)
            audioManager.PlayVFX(VFX_clip.Ship);
            //pulseAudioSource.PlayOneShot(pulseAudioSource.clip); // play the pulse sound effect witout pause or interrupt
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.DrawWireSphere(transform.position, GameStaticInfo.STUN_SONAR_RADIUS);
    //}
}
