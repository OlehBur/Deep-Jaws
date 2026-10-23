using UnityEngine;

public class CurrentEffect : Effect {
    GameObject bubblesWhirlObj;
    VFXpoolManager fishAddComp;
    Vector3 whirlLocPos;
    Transform whirlParent;
    const float BOOSTED_SPEED_MULTIPLIER = 1.8f;

    public CurrentEffect(VFXpoolManager fishAddComp, Transform whirlParent, Vector3 whirlLocPos) {
        this.fishAddComp = fishAddComp;
        this.whirlLocPos = whirlLocPos;
        this.whirlParent = whirlParent;
        //this.bubblesWhirlObj = bubblesWhirlObj;
        type = FishEffectType.CurrentSpeed;
    }

    protected override void OnApply() {
        //bubblesWhirlObj.SetActive(true);
        if (fishAddComp != null)
            bubblesWhirlObj = fishAddComp.ActivateObjectFromPool(FishComponentType.WhirlLine, whirlParent, whirlLocPos, Vector3.one, Quaternion.identity);
        targetFish.ChangeSpeedMultiplier(BOOSTED_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has applied Speed Effect for {duration} s.");
    }

    protected override void OnExpire() {
        if (bubblesWhirlObj != null)
            bubblesWhirlObj.SetActive(false);
        targetFish.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has expired Speed Effect after {duration} s.");
    }
}
