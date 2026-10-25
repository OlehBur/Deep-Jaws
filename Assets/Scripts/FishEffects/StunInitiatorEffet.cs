using UnityEngine;

public class StunInitiatorEffet : Effect {
    private int _collidedObjCnt = 0;
    private Collider[] _collideObjects = new Collider[15];


    protected override void OnApply() {
        Type = FishEffectType.StunInitiator;

        _collidedObjCnt = Physics.OverlapSphereNonAlloc(targetFish.transform.position,
            GameStaticInfo.STUN_SONAR_MAX_SCALE,
            _collideObjects,
            GameStaticInfo.GetEntityLayerMask(),
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _collidedObjCnt; i++) {
            Rigidbody victimRb = _collideObjects[i].attachedRigidbody;

            if (_collideObjects[i] != null && victimRb != null
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
        if (_collidedObjCnt > 0)//previous collided objects if the effect has not expired, but refreshed
            ClearCollidedObjects();
    }

    protected override void OnExpire() {
        //disable icon
        //if (collidedObjCnt > 0)
        //    ClearCollidedObjects();
    }

    void ClearCollidedObjects() {
        for (int i = 0; i < _collidedObjCnt; i++)
            _collideObjects[i] = null;
        _collidedObjCnt = 0;
    }

    //void OnDrawGizmos() {
    //    Gizmos.color = Color.white;

    //    if (target)
    //        Gizmos.DrawWireSphere(target.transform.position, /*STUN_RADIUS*/GameStaticInfo.STUM_SONAR_RADIUS);
    //}
}
