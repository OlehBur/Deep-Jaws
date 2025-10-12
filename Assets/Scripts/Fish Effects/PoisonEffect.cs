using UnityEngine;

public class PoisonEffect : Effect {
    private const float DAMAGE_INTERVAL = 1f;
    private const int DAMAGE_AMOUNT = 2;
    private float currentTime = 0f;
    const float PLAYER_SPEED_MULTIPLIER = .5f;

    protected override void OnApply() {
        target.ChangeSpeedMultiplier(PLAYER_SPEED_MULTIPLIER, FishSpeedModifierType.Poison);
        //if (target.IsPlayer)
        //    Debug.LogWarning("Player has poison effect");
    }

    protected override void OnExpire() {
        target.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Poison);
    }

    public override void UpdateTimer() {
        base.UpdateTimer();
        currentTime += Time.unscaledDeltaTime;

        if (currentTime >= DAMAGE_INTERVAL) {
            currentTime = 0f;

            target.TakeDamage(DAMAGE_AMOUNT);

            if (target.Health <= 0)
                Enabled = false;
            //DeactivateEffect();
            //Destroy(this);
        }
    }
}
