using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer;


public enum ZoneType {
    //CoralRocks,
    AlgaesForest,
    DeepCorals
}

public class ZoneController : MonoBehaviour {
    //[SerializeField] SpawnManager spawnManager;
    [SerializeField] ZoneType zoneType;
    [SerializeField] Material zoneBarrierMat;
    [SerializeField] Collider barrierCollider;
    [SerializeField] PlayerUIController playerUIController;

    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;

    private static bool isBarrierAnimationActive { get; set; } = false;
    private const float BARRIER_TOUCH_ANIMATION_INTERVAL = 1.6f;


    private void Start() {//work onlu once per game install (only for old players)
        _playerFishPoolService.OnPlayerRespawn += BiomeByLvlBehaviour;

        if (_playerFishPoolService.PlayerCurrentFish != null)
            BiomeByLvlBehaviour(_playerFishPoolService.PlayerCurrentFish, false);

        zoneBarrierMat.SetFloat(GameStaticInfo.ZONE_PROGRESS_PROP, 0);//resert previous mat
    }

    void BiomeByLvlBehaviour(Fish fish, bool isDie) {
        if (fish == null)
            return;

        switch (zoneType) {
            //case ZoneType.CoralRocks:
            //    zoneCollidersParent.SetActive(fish.Level < FishLevel.Moray)

            //        break;
            case ZoneType.AlgaesForest:
            barrierCollider.enabled = fish.Level < FishLevel.Moray;
            break;
            case ZoneType.DeepCorals:
            barrierCollider.enabled = fish.Level < FishLevel.HammerShark;
            break;
        }
    }

    private void OnCollisionStay/*Enter*/(Collision collision) {
        Rigidbody victimRb = collision.collider.attachedRigidbody;

        if (victimRb != null && victimRb.CompareTag(GameStaticInfo.TAG_PLAYER)) {
            //playerUIController.ActivateZoneOverlay(fishLvlRequirMsg);
            playerUIController.ActivateBiomeTip(zoneType);

            if (!isBarrierAnimationActive) {
                StartCoroutine(BarrierTouchAnimation());
                zoneBarrierMat.SetVector(GameStaticInfo.ZONE_TOUCH_POS_PROP, collision.GetContact(0).point);
            }
        }
    }

    IEnumerator BarrierTouchAnimation() {
        isBarrierAnimationActive = true;
        float currProg = zoneBarrierMat.GetFloat(GameStaticInfo.ZONE_PROGRESS_PROP);

        while (currProg < 1f) {
            currProg = Mathf.Clamp01(currProg + DJTime.DeltaTime * BARRIER_TOUCH_ANIMATION_INTERVAL);
            zoneBarrierMat.SetFloat(GameStaticInfo.ZONE_PROGRESS_PROP, currProg);
            currProg = zoneBarrierMat.GetFloat(GameStaticInfo.ZONE_PROGRESS_PROP);
            yield return null;
        }
        while (currProg > 0f) {
            currProg = Mathf.Clamp01(currProg - DJTime.DeltaTime * BARRIER_TOUCH_ANIMATION_INTERVAL * 2);//double speed
            zoneBarrierMat.SetFloat(GameStaticInfo.ZONE_PROGRESS_PROP, currProg);
            currProg = zoneBarrierMat.GetFloat(GameStaticInfo.ZONE_PROGRESS_PROP);
            yield return null;
        }

        isBarrierAnimationActive = false;
    }
}
