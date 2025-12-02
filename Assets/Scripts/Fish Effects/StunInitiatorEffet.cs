using UnityEngine;

public class StunInitiatorEffet : Effect {
    //private const float STUN_RADIUS = 100f;
    private int collidedObjCnt = 0;
    private Collider[] collideObjects = new Collider[15];


    protected override void OnApply() {
        //bool isPlayer = CompareTag("Player");
        type = FishEffectType.StunInitiator;

        collidedObjCnt = Physics.OverlapSphereNonAlloc(target.transform.position,
            /*STUN_RADIUS*/GameStaticInfo.STUN_SONAR_RADIUS,
            collideObjects,
            GameStaticInfo.ENTITY_LAYER_MASK /*Physics.AllLayers*/,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < collidedObjCnt; i++) {
            //Transform parent = collideObjects[i].transform.parent;
            Rigidbody victimRb = collideObjects[i].attachedRigidbody;

            if (collideObjects[i] != null && victimRb != null /*!collideObjects[i].isTrigger &&*/
                && victimRb.gameObject != target.gameObject) {
                if (victimRb.TryGetComponent(out EffectManager effectManager))
                    effectManager.ApplyEffect(FishEffectType.Stun, /*Duration*/GameStaticInfo.STUN_DEFAULT_DURATION);

                if (GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish victimFish)//parent.TryGetComponent<Fish>(out Fish fish)
                   && target.IsPlayer && victimFish.Level == FishLevel.Pufferfish)
                    GPG_Manager.OnTriggerAchievement(AchievementKey.Inevitability);
            }
        }
        //Debug.Log(collidedObjCnt);
        if (collidedObjCnt > 0)//previous collided objects if the effect has not expired, but refreshed
            ClearCollidedObjects();
    }

    protected override void OnExpire() {
        //disable icon
        //if (collidedObjCnt > 0)
        //    ClearCollidedObjects();
    }

    void ClearCollidedObjects() {
        for (int i = 0; i < collidedObjCnt; i++)
            collideObjects[i] = null;
        collidedObjCnt = 0;
    }

    //void OnDrawGizmos() {
    //    Gizmos.color = Color.white;

    //    if (target)
    //        Gizmos.DrawWireSphere(target.transform.position, /*STUN_RADIUS*/GameStaticInfo.STUM_SONAR_RADIUS);
    //}
}
