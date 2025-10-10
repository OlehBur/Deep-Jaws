using System.Collections;
using UnityEngine;


//only for scale animation during a poison attack
public class HydraController : MonoBehaviour {
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hydraBiteClip;
    [SerializeField] Animator animator;
    private const string ANIMATION_NAME = "HydraBite";
    //private bool isAllowTrigger = true;
    //private const float SCALE_SPEED = 1.6f;
    //private const float TRIGG_SCALE_ADD = .08f;
    //private float initScale;

    //private void Awake() {
    //    initScale = transform.localScale.x;
    //}

    private void OnTriggerEnter(Collider other) {
        Transform parentFish = other.transform.parent;

        if (/*isAllowTrigger &&*/ parentFish != null && GameStaticInfo.TryGetFish(parentFish.gameObject, out Fish fish)//parentFish.TryGetComponent<Fish>(out Fish fish)
            && fish.IsVisible) {
            //StartCoroutine(TriggerScale());            //InvokeRepeating("", 0f, 2f);
            //isAllowTrigger = false;
            animator.Play(ANIMATION_NAME);

            if (other.transform.parent.CompareTag("Player"))
                audioSource.PlayOneShot(hydraBiteClip);
        }
    }

    //IEnumerator TriggerScale() {
    //    //inscrease scale
    //    while (transform.localScale.x < initScale + TRIGG_SCALE_ADD) {
    //        transform.localScale += new Vector3(SCALE_SPEED, SCALE_SPEED, SCALE_SPEED) * Time.deltaTime;
    //        yield return null;
    //    }

    //    //decrease scale
    //    while (transform.localScale.x > initScale) {
    //        transform.localScale -= new Vector3(SCALE_SPEED, SCALE_SPEED, SCALE_SPEED) * Time.deltaTime;
    //        yield return null;
    //    }
    //    isAllowTrigger = true;
    //}
}
