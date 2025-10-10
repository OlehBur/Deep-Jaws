using UnityEngine;

public class StunInitiatorEffet : Effect {
    private const float STUN_RADIUS = 100f;
    private int collidedObjCnt = 0;
    private Collider[] collideObjects = new Collider[15];


    protected override void OnApply() {
        //bool isPlayer = CompareTag("Player");
        type = FishEffectType.StunInitiator;

        if (collidedObjCnt > 0)//previous collided objects if the effect has not expired, but refreshed
            ClearCollidedObjects();

        collidedObjCnt = Physics.OverlapSphereNonAlloc(target.transform.position, STUN_RADIUS, collideObjects, GameStaticInfo.ENTITY_LAYER_MASK);
        for (int i = 0; i < collidedObjCnt; i++) {
            Transform parent = collideObjects[i].transform.parent;

            if (collideObjects[i] != null && parent.gameObject != target.gameObject) {
                if (parent.TryGetComponent(out EffectManager effectManager))
                    effectManager.ApplyEffect(FishEffectType.Stun, Duration);

                if (GameStaticInfo.TryGetFish(parent.gameObject, out Fish fish)//parent.TryGetComponent<Fish>(out Fish fish)
                   && fish.IsPlayer && fish.Level == FishLevel.Pufferfish)
                    GPG_Manager.OnTriggerAchievement(AchievementKey.Inevitability);
            }
        }
    }

    protected override void OnExpire() {
        //disable icon
        ClearCollidedObjects();
    }

    void ClearCollidedObjects() {
        for (int i = 0; i < collidedObjCnt; i++)
            collideObjects[i] = null;
        collidedObjCnt = 0;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;

        if (target)
            Gizmos.DrawWireSphere(target.transform.position, STUN_RADIUS);
    }
}
