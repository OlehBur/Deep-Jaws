using UnityEngine;

public class ShieldEffect : Effect {
    public BubbleShieldWrapper ShieldWrap { get; private set; }
    private FishAdditionalCompManager fishAddComp;
    private AudioManager audioManager;
    private Vector3 shieldLocalPos;
    private Vector3 shieldLocalScale;
    private const float SHIELD_SCALE_MULTIPLIER = 1.2f;
    private Transform shieldParent;

    public ShieldEffect(AudioManager audioManager, FishAdditionalCompManager fishAddComp, Transform parent, Vector3 shieldLocPos, Vector3 shieldLocScale) {
        this.audioManager = audioManager;
        this.fishAddComp = fishAddComp;
        shieldLocalPos = shieldLocPos;
        shieldLocalScale = shieldLocScale;
        shieldParent = parent;
        //shieldObj = shield;
        type = FishEffectType.Shield;
    }

    void UpdateShieldSize(Vector3 fishScale) {
        if (ShieldWrap.collider != null) {
            ShieldWrap.collider.height = fishScale.y * shieldLocalScale.z / SHIELD_SCALE_MULTIPLIER;
            ShieldWrap.collider.radius = fishScale.z * shieldLocalScale.y / SHIELD_SCALE_MULTIPLIER;
            ShieldWrap.visualTransform.localScale = new Vector3(
                fishScale.x * shieldLocalScale.x,
                fishScale.y * shieldLocalScale.y,
                fishScale.z * shieldLocalScale.z) * SHIELD_SCALE_MULTIPLIER;
            //Debug.LogWarning("TODO: erase shield loc scale field");
        }
    }

    protected override void OnApply() {
        //shieldObj?.SetActive(true);
        if (fishAddComp != null)
            ShieldWrap = fishAddComp.ActivateShieldFromPool(/*FishComponentType.ShieldBubble,*/ shieldParent, shieldLocalPos/*, shieldLocalScale, Quaternion.identity*/);

        if (ShieldWrap != null) {
            UpdateShieldSize(targetFish.VisualTransformScale);
            targetFish.OnFishUpdateSize += UpdateShieldSize;
        }

        if (audioManager != null)
            audioManager.PlayVFX(VFX_clip.BubblePop);
        //Debug.Log($"{target.name} has applied Shield Effect for {duration} s.");
    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired Shield Effect after {duration} s.");
        audioManager?.PlayVFX(VFX_clip.BubblePop);
        if (ShieldWrap != null)
            ShieldWrap.obj.SetActive(false);

        targetFish.OnFishUpdateSize -= UpdateShieldSize;
    }
}
