using UnityEngine;

public class CurrentEffect : Effect {
    private GameObject _bubblesWhirlObj;

    private readonly IVFXPoolService _fishAddComp;
    private readonly Vector3 _whirlLocPos;
    private readonly Transform _whirlParent;

    const float BOOSTED_SPEED_MULTIPLIER = 1.8f;

    public CurrentEffect(IVFXPoolService fishAddComp, Transform whirlParent, Vector3 whirlLocPos) {
        _fishAddComp = fishAddComp;
        _whirlLocPos = whirlLocPos;
        _whirlParent = whirlParent;
        //this.bubblesWhirlObj = bubblesWhirlObj;
        Type = FishEffectType.CurrentSpeed;
    }

    protected override void OnApply() {
        //bubblesWhirlObj.SetActive(true);
        if (_fishAddComp != null)
            _bubblesWhirlObj = _fishAddComp.ActivateObjectFromPool(FishVFX.WhirlLine, _whirlParent, _whirlLocPos, Vector3.one, Quaternion.identity);
        targetFish.ChangeSpeedMultiplier(BOOSTED_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has applied Speed Effect for {duration} s.");
    }

    protected override void OnExpire() {
        if (_bubblesWhirlObj != null)
            _bubblesWhirlObj.SetActive(false);
        targetFish.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has expired Speed Effect after {duration} s.");
    }
}
