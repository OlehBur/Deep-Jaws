using UnityEngine;

public class SprintEffect : Effect {
    GameObject trailObj;
    FishAnimManager fishAnim;
    FishAdditionalCompManager fishAddComp;
    Vector3 trailLocalPos;
    Transform trailParent;
    const float SPRINT_SPEED_MULTIPLIER = 1.5f;

    public SprintEffect(FishAdditionalCompManager fishAddComp, FishAnimManager fishAnim, Transform trailParent, Vector3 trailLocalPos) {
        //this.trailObj = trailObj;
        this.fishAddComp = fishAddComp;
        this.trailLocalPos = trailLocalPos;
        this.fishAnim = fishAnim;
        this.trailParent = trailParent;
        type = FishEffectType.SprintSpeed;
    }

    protected override void OnApply() {
        //trailObj.SetActive(true);
        trailObj = fishAddComp.ActivateObjectFromPool(FishComponentType.PlayerSprintLine, trailParent, trailLocalPos, Vector3.one, Quaternion.identity);
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



