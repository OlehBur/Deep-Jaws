using UnityEngine;
using VContainer;


public class SwimbaitController : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] CapsuleCollider baiteModelCollider;
    [SerializeField] GroundbaitController[] groundbaitControllers;

    [Inject] private readonly ISpawnOrchestrator _spawnOrchestrator;
    [Inject] private readonly IVFXPoolService _vfxPoolService;

    private FishStatsController _fishToDie = null;

    private const float BREAK_CHANCE_DEFAULT_MULT = .02f;
    private const float BREAK_CHANCE_SPRINT_MULT = .1f;


    private void OnEnable() {
        foreach (GroundbaitController groundbait in groundbaitControllers) {
            groundbait.Init(_spawnOrchestrator);
            groundbait.gameObject.SetActive(true);
        }

        baiteModelCollider.gameObject.SetActive(true);
        baiteModelCollider.enabled = true;
        animator.ResetTrigger(GameStaticInfo.GROUNDBAITE_ANIM_DISAPPEAR);
        animator.ResetTrigger(GameStaticInfo.GROUNDBAITE_ANIM_APPEAR);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody == null)
            return;

        if (baiteModelCollider.enabled
            && _spawnOrchestrator.IsCollideWithFishMouth(other, out FishStatsController fish)
            && !fish.Rb.isKinematic) {

            baiteModelCollider.enabled = false;

            if (IsCanBreakBait(fish)) {
                baiteModelCollider.gameObject.SetActive(false);
                _vfxPoolService.ActivateObjectFromPool(FishVFX.DeadBubblesParticle, null, baiteModelCollider.transform.position, Vector3.one, Quaternion.identity);
                fish.FeedSwimbait();
                fish.TakeDamage(GameStaticInfo.SWIMBAIT_DAMAGE);
            }
            else {
                fish.ActivateCinematic(true);
                fish.transform.SetParent(baiteModelCollider.transform, true);
                _fishToDie = fish;
                Debug.Log("FIsj caught");
            }

            animator.SetTrigger(GameStaticInfo.GROUNDBAITE_ANIM_DISAPPEAR);//with auto disable obj
        }
    }

    public void OnKillCaughtFish() {//call in anim event
        Debug.Log("OnKillCaughtFish");
        if (_fishToDie != null) {
            _fishToDie.Die();
            Debug.Log("Fish caught and die");
            _fishToDie = null;
        }
    }

    public void OnDeactivate() {//call in anim event
        gameObject.SetActive(false);
    }

    private bool IsCanBreakBait(FishStatsController fish) {
        fish.TryGetComponent(out EffectManager effectManager);

        float chance = 0;
        bool isHasSprint = false;

        if (effectManager != null 
            && effectManager.ActiveEffects.TryGetValue(FishEffectType.SprintSpeed, out Effect sprintEffect))
            isHasSprint = sprintEffect != null && sprintEffect.Enabled;

        if (fish.Level >= GameStaticInfo.SHARK_INDEX_BARRIER)
            chance = 1f;
        else {

            chance = (float)fish.Level;
            chance *= isHasSprint ?
                BREAK_CHANCE_SPRINT_MULT : BREAK_CHANCE_DEFAULT_MULT;
        }

        return chance > Random.value;
    }
}
