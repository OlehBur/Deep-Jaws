using UnityEngine;

public class EffectArea : MonoBehaviour {
    public FishEffectType effectType;
    public float duration;

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        if (!other.isTrigger/* && victimObj != null*/ && !other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD) && victimObj.TryGetComponent(out EffectManager fem)) { //if component on fish exists - apply effect
            fem.ApplyEffect(effectType, duration);

            if (victimObj.CompareTag(GameStaticInfo.TAG_PLAYER)) //if player fish, check for achievements
                CheckPlayerEffectAchievents(effectType);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        GameObject victimObj = other.attachedRigidbody.gameObject;

        if (!GameManager.IsGamePaused && !other.isTrigger/* && victimObj != null */
            && !other.CompareTag(GameStaticInfo.TAG_BUBBLESHIELD) && victimObj.TryGetComponent(out EffectManager fem)) {
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
