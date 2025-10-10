using UnityEngine;

public class EffectArea : MonoBehaviour {
    public FishEffectType effectType;
    public float duration;

    private void OnTriggerEnter(Collider other) {
        if (other.transform.parent.TryGetComponent(out EffectManager fem)) { //if component on fish exists - apply effect
            fem.ApplyEffect(effectType, duration);//transform.parent. because collider is child of fish prefab

            if (other.transform.parent.CompareTag("Player")) //if player fish, check for achievements
                CheckPlayerEffectAchievents(effectType);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.transform.parent.TryGetComponent(out EffectManager fem)) {
            fem.ApplyEffect(effectType, duration);
        }
    }

    void CheckPlayerEffectAchievents(FishEffectType effect) {
        if (effect == FishEffectType.Shield)
            GPG_Manager.OnTriggerAchievement(AchievementKey.BigGum);
        else if (effect == FishEffectType.StunInitiator)
            GPG_Manager.OnTriggerAchievement(AchievementKey.DotDash);
    }
}
