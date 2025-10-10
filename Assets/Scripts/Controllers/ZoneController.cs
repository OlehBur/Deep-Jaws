using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


enum ZoneType {
    //CoralRocks,
    AlgaesForest,
    DeepCorals
}

public class ZoneController : MonoBehaviour {
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] ZoneType zoneType;
    [SerializeField] Material zoneBarrierMat;
    [SerializeField] Collider barrierCollider;
    [SerializeField] PlayerUIController playerUIController;
    [SerializeField] LocalizedString morayWarnLocalizedString;
    [SerializeField] LocalizedString hammerheadWarnLocalizedString;
    private static bool isBarrierAnimationActive { get; set; } = false;
    private const float BARRIER_TOUCH_ANIMATION_INTERVAL = 1.6f;



    private string fishLvlRequirMsg;

    void OnEnable() {
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;
    }

    private void OnDestroy() {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    void OnDisable() {
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    //realtime reaction on changing locale
    void OnLanguageChanged(UnityEngine.Localization.Locale newLocale) {
        fishLvlRequirMsg = zoneType switch {
            ZoneType.AlgaesForest => morayWarnLocalizedString.GetLocalizedString(),
            ZoneType.DeepCorals => hammerheadWarnLocalizedString.GetLocalizedString(),
            _ => morayWarnLocalizedString.GetLocalizedString()
        };
        //    switch (newLocale.Identifier.Code) {
        //        case "en":
        //            fishLvlRequirMsg = "You need to reach at least the level ";
        //            fishLvlRequirMsg += zoneType switch {
        //                ZoneType.AlgaesForest => "Moray Eel",
        //                ZoneType.DeepCorals => "Hammerhead Shark",
        //                _ => "Moray Eel"
        //            };
        //            break;

        //        case "uk":
        //            fishLvlRequirMsg = "Вам потрібно досягти хоча б рівня ";
        //            fishLvlRequirMsg += zoneType switch {
        //                ZoneType.AlgaesForest => "Мурени",
        //                ZoneType.DeepCorals => "Акули-молота",
        //                _ => "Мурени"
        //            };
        //            break;
        //    }
    }

    private void Start() {//work onlu once per game install (only for old players)
        spawnManager.OnPlayerRespawn += ZoneByLvlBehaviour;
        if (spawnManager.PlayerFish != null)
            ZoneByLvlBehaviour(spawnManager.PlayerFish);

        Locale currentLocale = LocalizationSettings.SelectedLocale;
        OnLanguageChanged(currentLocale);

        zoneBarrierMat.SetFloat("_Progress", 0);//resert previous mat
    }

    void ZoneByLvlBehaviour(Fish fish) {
        switch (zoneType) {
            //case ZoneType.CoralRocks:
            //    zoneCollidersParent.SetActive(fish.Level < FishLevel.Moray)

            //        break;
            case ZoneType.AlgaesForest:
                barrierCollider.enabled = /*SetActive*/fish.Level < FishLevel./*Mackarel*/Moray;
                break;
            case ZoneType.DeepCorals:
                barrierCollider.enabled = /*SetActive*/fish.Level < FishLevel.HammerShark;
                break;
        }
    }

    private void OnCollisionStay/*Enter*/(Collision collision) {
        Transform parent = collision.collider.transform.parent;

        if (parent != null && parent.CompareTag("Player")) {
            playerUIController.ActivateZoneOverlay(fishLvlRequirMsg);
            if (!isBarrierAnimationActive) {
                //StopCoroutine(BarrierTouchAnimation());
                StartCoroutine(BarrierTouchAnimation());
                zoneBarrierMat.SetVector("_TouchPosition", collision.GetContact(0).point);
            }
        }
    }

    IEnumerator BarrierTouchAnimation() {
        isBarrierAnimationActive = true;
        float currProg = zoneBarrierMat.GetFloat("_Progress");

        while (currProg < 1f) {
            currProg = Mathf.Clamp01(currProg + Time.deltaTime * BARRIER_TOUCH_ANIMATION_INTERVAL);
            zoneBarrierMat.SetFloat("_Progress", currProg);
            currProg = zoneBarrierMat.GetFloat("_Progress");
            yield return null;
        }
        while (currProg > 0f) {
            currProg = Mathf.Clamp01(currProg - Time.deltaTime * BARRIER_TOUCH_ANIMATION_INTERVAL * 2);//double speed
            zoneBarrierMat.SetFloat("_Progress", currProg);
            currProg = zoneBarrierMat.GetFloat("_Progress");
            yield return null;
        }

        isBarrierAnimationActive = false;
    }

    //tp player if he already in the zone (examp after game update)
    //private void OnCollisionStay(Collision collision) {
    //    if (isInAllowTPfromZone) {
    //        if (sceneLoadTime - Time.time > SCENE_LOAD_TP_ALLOW_TIME) {// only if real stay instean collision stay
    //            isInAllowTPfromZone = false;
    //            return;
    //        }

    //        Transform parent = collision.collider.transform.parent;

    //        if (parent != null && parent.CompareTag("Player")) {
    //            Debug.Log("OnCollisionStay~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    //            isInAllowTPfromZone = false;
    //            parent.transform.position = Vector3.up * parent.transform.position.y;
    //        }
    //    }
    //}
}
