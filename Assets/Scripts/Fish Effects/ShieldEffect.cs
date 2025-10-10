using UnityEngine;

public class ShieldEffect : Effect {
    private GameObject shieldObj;

    public ShieldEffect(GameObject shield) {
        shieldObj = shield;
        type = FishEffectType.Shield;
    }

    protected override void OnApply() {
        shieldObj?.SetActive(true);
        //Debug.Log($"{target.name} has applied Shield Effect for {duration} s.");
    }

    protected override void OnExpire() {
        //Debug.Log($"{target.name} has expired Shield Effect after {duration} s.");
        shieldObj?.SetActive(false);
    }
}
