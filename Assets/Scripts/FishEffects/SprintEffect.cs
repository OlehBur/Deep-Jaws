using UnityEngine;

public class SprintEffect : Effect {
    private GameObject trailObj;

    private readonly FishAnim _fishAnim;
    private readonly Vector3 _trailLocalPos;
    private readonly Transform _trailParent;
    private readonly IVFXPoolService _vfxPoolService;

    const float SPRINT_SPEED_MULTIPLIER = 1.5f;

    public SprintEffect(IVFXPoolService fishAddComp, FishAnim fishAnim, Transform trailParent, Vector3 trailLocalPos) {
        //this.trailObj = trailObj;
        _vfxPoolService = fishAddComp;
        _trailLocalPos = trailLocalPos;
        _fishAnim = fishAnim;
        _trailParent = trailParent;

        Type = FishEffectType.SprintSpeed;
    }

    protected override void OnApply() {
        //trailObj.SetActive(true);
        trailObj = _vfxPoolService.ActivateObjectFromPool(FishVFX.PlayerSprintLine, _trailParent, _trailLocalPos, Vector3.one, Quaternion.identity);
        targetFish.ChangeSpeedMultiplier(SPRINT_SPEED_MULTIPLIER, FishSpeedModifierType.Sprint);
        //Debug.Log($"{target.name} has applied Speed Effect for {duration} s.");
        _fishAnim?.SprintAnimation(); // enable sprint animation boost
    }

    protected override void OnExpire() {
        const float NORMAL_SPEED_MULTIPLIER = 1f;

        if (trailObj != null)
            trailObj.SetActive(false);
        targetFish.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Sprint);
        //Debug.Log($"{target.name} has expired Speed Effect after {duration} s.");
        _fishAnim?.SprintAnimation(false); // disable sprint animation speed
    }
}



