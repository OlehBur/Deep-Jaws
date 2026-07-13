using UnityEngine;

public class EffectArea : MonoBehaviour {
    public FishEffectType effectType;
    public float duration;

    private void OnTriggerEnter(Collider other) {
        //Transform parent = other.transform.parent;
        Rigidbody victimRb = other.attachedRigidbody;

        if (!other.isTrigger && victimRb != null && !other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD) && victimRb.TryGetComponent(out EffectManager fem)) { //if component on fish exists - apply effect
            fem.ApplyEffect(effectType, duration);//transform.parent. because collider is child of fish prefab

            if (/*other.transform.parent*/victimRb.CompareTag(GameStaticInfo.TAG_PLAYER)) //if player fish, check for achievements
                CheckPlayerEffectAchievents(effectType);
        }
    }

    private void OnTriggerStay(Collider other) {
        //Transform parent = other.transform.parent;
        Rigidbody victimRb = other.attachedRigidbody;

        if (!GameManager.IsGamePaused && !other.isTrigger && victimRb != null && !other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD) && victimRb.TryGetComponent(out EffectManager fem)) {
            fem.ApplyEffect(effectType, duration);
        }
    }

    void CheckPlayerEffectAchievents(FishEffectType effect) {
        if (effect == FishEffectType.Shield)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_just_how_big_must_that_gum_have_been);
        else if (effect == FishEffectType.StunInitiator)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_dot_dot_dot_dash_dash_dash_dot_dot_dot);
    }
}
