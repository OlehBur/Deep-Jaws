using UnityEngine;

public class ShieldEffect : Effect {
    public BubbleShieldWrapper ShieldWrap { get; private set; }

    private const float SHIELD_SCALE_MULTIPLIER = 1.2f;

    private readonly IVFXPoolService _fishAddComp;
    private readonly IAudioManager _audioManager;
    private readonly Vector3 _shieldLocalPos;
    private readonly Vector3 _shieldLocalScale;
    private readonly Transform _shieldParent;

    public ShieldEffect(IAudioManager audioManager, IVFXPoolService fishAddComp, Transform parent, Vector3 shieldLocPos, Vector3 shieldLocScale) {
        _audioManager = audioManager;
        _fishAddComp = fishAddComp;
        _shieldLocalPos = shieldLocPos;
        _shieldLocalScale = shieldLocScale;
        _shieldParent = parent;

        Type = FishEffectType.Shield;
    }

    void UpdateShieldSize(Vector3 fishScale) {
        if (ShieldWrap.collider != null) {
            ShieldWrap.collider.height = fishScale.y * _shieldLocalScale.z / SHIELD_SCALE_MULTIPLIER;
            ShieldWrap.collider.radius = fishScale.z * _shieldLocalScale.y / SHIELD_SCALE_MULTIPLIER;
            ShieldWrap.visualTransform.localScale = new Vector3(
                fishScale.x * _shieldLocalScale.x,
                fishScale.y * _shieldLocalScale.y,
                fishScale.z * _shieldLocalScale.z) * SHIELD_SCALE_MULTIPLIER;
            //Debug.LogWarning("TODO: erase shield loc scale field");
        }
    }

    protected override void OnApply() {
        //shieldObj?.SetActive(true);
        if (_fishAddComp != null)
            ShieldWrap = _fishAddComp.ActivateShieldFromPool(_shieldParent, _shieldLocalPos);

        if (ShieldWrap != null) {
            UpdateShieldSize(targetFish.VisualTransformScale);
            targetFish.OnFishUpdateSize += UpdateShieldSize;
        }

        if (_audioManager != null)
            _audioManager.PlayVFX(VFX_clip.BubblePop);
        //Debug.Log($"{target.name} has applied Shield Effect for {duration} s.");
    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired Shield Effect after {duration} s.");
        _audioManager?.PlayVFX(VFX_clip.BubblePop);
        if (ShieldWrap != null)
            ShieldWrap.obj.SetActive(false);

        targetFish.OnFishUpdateSize -= UpdateShieldSize;
    }
}
