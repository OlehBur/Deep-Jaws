using UnityEngine;

public class PoisonEffect : Effect {
    private const float DAMAGE_INTERVAL = 1f;
    private const int DAMAGE_AMOUNT = 2;
    private float currentTime = 0f;
    const float PLAYER_SPEED_MULTIPLIER = .5f;

    protected override void OnApply() {
        targetFish.ChangeSpeedMultiplier(PLAYER_SPEED_MULTIPLIER, FishSpeedModifierType.Poison);
        //if (target.IsPlayer)
        //    Debug.LogWarning("Player has poison effect");
        targetFish.TakeDamage(DAMAGE_AMOUNT);

        if (targetFish.Health <= 0)
            Enabled = false;
    }

    protected override void OnExpire() {
        targetFish.ChangeSpeedMultiplier(NORMAL_SPEED_MULTIPLIER, FishSpeedModifierType.Poison);
    }

    public override void UpdateTimer() {
        base.UpdateTimer();
        currentTime += DJTime.DeltaTime;

        if (currentTime >= DAMAGE_INTERVAL && !GameManager.IsGamePaused) {
            currentTime = 0f;

            targetFish.TakeDamage(DAMAGE_AMOUNT);

            if (targetFish.Health <= 0)
                Enabled = false;
            //DeactivateEffect();
            //Destroy(this);
        }
    }
}
