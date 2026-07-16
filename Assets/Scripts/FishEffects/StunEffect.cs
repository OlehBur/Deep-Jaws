using UnityEngine;

public class StunEffect : Effect {
    private Rigidbody rb;


    public StunEffect(Rigidbody rb) {
        this.rb = rb;
        type = FishEffectType.Stun;
    }

    protected override void OnApply() {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    protected override void OnExpire() {
        rb.constraints = GameStaticInfo.DEFAULT_FISH_CONSTRAINS_RIQIDBODY;
    }
}
