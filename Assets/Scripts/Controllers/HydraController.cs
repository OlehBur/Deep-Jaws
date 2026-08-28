using System.Collections;
using UnityEngine;


//only for scale animation during a poison attack
public class HydraController : MonoBehaviour {
    [SerializeField] AudioManager audioManager;
    [SerializeField] Animator animator;

    private const string ANIMATION_NAME = "HydraBite";


    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        Transform parentFish = other.attachedRigidbody.transform;

        if (!other.isTrigger && GameStaticInfo.TryGetFish(parentFish.gameObject, out Fish fish)
            && fish.IsVisible) {

            animator.Play(ANIMATION_NAME);

            if (fish.IsPlayer)
                audioManager.PlayVFX(VFX_clip.Hydra);
        }
    }
}
