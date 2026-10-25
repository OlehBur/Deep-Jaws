using UnityEngine;

public class StunEffect : Effect {
    private Rigidbody _rb;


    public StunEffect(Rigidbody rb) {
        _rb = rb;
        Type = FishEffectType.Stun;
    }

    protected override void OnApply() {
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    protected override void OnExpire() {
        _rb.constraints = GameStaticInfo.DEFAULT_FISH_CONSTRAINS_RIQIDBODY;
    }
}
