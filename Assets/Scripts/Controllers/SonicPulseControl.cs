
using UnityEngine;
using VContainer;

public class SonicPulseControl : MonoBehaviour {
    [SerializeField] Light lightIndicator;
    [SerializeField] Material blinkIndicatorMat;
    [SerializeField] Transform pulseTransform;
    [SerializeField] Collider triggerCollider;
    
    private const float PULSE_RECHARGE_TIME = 30f; // time to recharge the pulse

    private float _curentRechargeTime = 0f; // current recharge time for pulse
    private bool _playAnim = false;

    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    [Inject] private readonly IAudioManager _audioManager;


    private void Update() {
        if (triggerCollider != null && !triggerCollider.enabled) {
            _curentRechargeTime = _curentRechargeTime - DJTime.DeltaTime; // decrease recharge time

            if (_curentRechargeTime <= 0f)
                triggerCollider.enabled = true;
        }

        IndicatorBlinking();
        if (_playAnim)
            PulseScalingAnim();

    }

    void PulseScalingAnim() {
        pulseTransform.localScale = pulseTransform.localScale + Vector3.one * GameStaticInfo.STUN_SONAR_SPEED * DJTime.DeltaTime;

        if (pulseTransform.localScale.x >= GameStaticInfo.STUN_SONAR_MAX_SCALE) {
            _playAnim = false;
            pulseTransform.localScale = Vector3.zero;
        }
    }

    void IndicatorBlinking() {
        const float EMISSION_SCALE = 2f;
        const float BLINKING_SPEED = 3f;
        float isBlinking = (_curentRechargeTime <= 0f) ?
            1f : 0f; // if pulse is ready to use, then blink

        float progress = Mathf.Sin(Time.time * BLINKING_SPEED) * .5f + .5f; //[0;1]

        lightIndicator.color = Color.red * progress * isBlinking;
        blinkIndicatorMat.SetColor("_EmissionColor", Color.red * progress * EMISSION_SCALE * isBlinking);
    }

    private void OnTriggerStay(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform fishParent = other.attachedRigidbody.transform;

        if (triggerCollider.enabled && !other.isTrigger /*&& GameManager.IsSceneReady*/ && Vector3.Distance(_playerFishPoolService.PlayerCurrentFish.transform.position, transform.position) < GameStaticInfo.PLAYER_MAX_DISTANCE_ACTIVITY
            && fishParent.TryGetComponent(out EffectManager fem) && !_playAnim && _curentRechargeTime <= 0f) {
            _playAnim = true;
            _curentRechargeTime = PULSE_RECHARGE_TIME; // reset recharge time
            triggerCollider.enabled = false;
            //Debug.Log("Bingo");
            //if (!pulseAudioSource.isPlaying)
            _audioManager.PlayVFX(VFX_clip.Ship);
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.DrawWireSphere(transform.position, GameStaticInfo.STUN_SONAR_RADIUS);
    //}
}
