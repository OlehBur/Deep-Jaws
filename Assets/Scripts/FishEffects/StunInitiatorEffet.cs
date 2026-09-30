using UnityEngine;

public class StunInitiatorEffet : Effect {
    private int collidedObjCnt = 0;
    private Collider[] collideObjects = new Collider[15];


    protected override void OnApply() {
        type = FishEffectType.StunInitiator;

        collidedObjCnt = Physics.OverlapSphereNonAlloc(targetFish.transform.position,
            GameStaticInfo.STUN_SONAR_MAX_SCALE,
            collideObjects,
            GameStaticInfo.LAYER_MASK_ENTITY,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < collidedObjCnt; i++) {
            Rigidbody victimRb = collideObjects[i].attachedRigidbody;

            if (collideObjects[i] != null && victimRb != null
                && victimRb.gameObject != targetFish.gameObject) {
                if (victimRb.TryGetComponent(out EffectManager effectManager))
                    effectManager.ApplyEffect(FishEffectType.Stun, /*Duration*/GameStaticInfo.STUN_DEFAULT_DURATION);

#if UNITY_ANDROID && !UNITY_EDITOR
                if (GameStaticInfo.TryGetFish(victimRb.gameObject, out Fish victimFish)
                   && targetFish.IsPlayer && victimFish.Level == FishLevel.Pufferfish)
                    GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_am_inevitability);
#endif
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
