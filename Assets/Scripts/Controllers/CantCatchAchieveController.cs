using UnityEngine;
using VContainer;


[RequireComponent(typeof(Collider))]
public class CantCatchAchieveController : MonoBehaviour {
    [Inject] private readonly IGPGManager _gpgManager;

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (other.attachedRigidbody.transform.CompareTag("Player")) {
#if !UNITY_STANDALONE_WIN
            _gpgManager.OnTriggerAchievement(GPGSIds.achievement_they_cant_reach_me_here);
            //Debug.Log("Achievement triggered for CantReachMe");
#endif
        }
    }
}
