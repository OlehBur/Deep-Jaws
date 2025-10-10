using UnityEngine;

public class SprintEffect : Effect {
    GameObject trailObj;
    FishAnimManager fishAnim;
    const float SPRINT_SPEED_MULTIPLIER = 1.5f;

    public SprintEffect(GameObject trailObj, FishAnimManager fishAnim) {
        this.trailObj = trailObj;
        this.fishAnim = fishAnim;
        type = FishEffectType.SprintSpeed;
    }

    protected override void OnApply() {        
        trailObj.SetActive(true);
        target.ChangeSpeedMultiplier(SPRINT_SPEED_MULTIPLIER, FishSpeedModifierType.Sprint);
        //Debug.Log($"{target.name} has applied Speed Effect for {duration} s.");
        fishAnim?.SprintAnimation(); // enable sprint animation boost
    }

    protected override void OnExpire() {
        const float NORMAL_SPEED_MULTIPLIER = 1f;

        trailObj.SetActive(false);
        target.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Sprint);
        //Debug.Log($"{target.name} has expired Speed Effect after {duration} s.");
        fishAnim?.SprintAnimation(false); // disable sprint animation speed
    }
}



