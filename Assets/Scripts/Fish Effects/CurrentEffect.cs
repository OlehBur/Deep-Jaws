using UnityEngine;

public class CurrentEffect : Effect {
    GameObject bubblesWhirlObj;
    const float BOOSTED_SPEED_MULTIPLIER = 1.8f;

    public CurrentEffect(GameObject bubblesWhirlObj) {
        this.bubblesWhirlObj = bubblesWhirlObj;
        type = FishEffectType.CurrentSpeed;
    }

    protected override void OnApply() {
        bubblesWhirlObj.SetActive(true);
        target.ChangeSpeedMultiplier(BOOSTED_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has applied Speed Effect for {duration} s.");
    }

    protected override void OnExpire() {
        bubblesWhirlObj.SetActive(false);
        target.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Current);
        //Debug.Log($"{target.name} has expired Speed Effect after {duration} s.");
    }
}
