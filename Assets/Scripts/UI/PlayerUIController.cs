using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// контролюЇ прогреси ус≥х ефект≥в, €к≥ впливають на гравц€ а також ун≥кальний спринт ефект прив'€заний до в≥дпов≥дноњ клав≥ш≥ на екран≥ 
/// controller for all effects that affect the player, as well as the unique sprint effect bound to the corresponding key on the screen
/// </summary>
/// 
public class PlayerUIController : MonoBehaviour {
    [Header("Icons for effects (UI)")]
    [SerializeField] RectTransform iconsParent;
    [SerializeField] Image shieldIcon;
    [SerializeField] Image stunIcon;
    [SerializeField] Image sprintIcon;
    [SerializeField] Image currentIcon;
    [SerializeField] Image timeIcon;
    [SerializeField] Image poisonIcon;
    [SerializeField] Image blindnessIcon;

    private SpawnManager spawnManager;
    private EffectManager playerEffectManager;
    private const string DEFAULT_FISH_ICONS_PARENT_NAME = "effects_icons";

    [SerializeField] Button sprintButton; // button for sprint effect activation
    const float SPRINT_BUTTON_COOLDOWN = 20f;
    const float SPRINT_EFFECT_DURATION = 5f;
    float currentSprintCooldownTime = 0;

    public static readonly Vector2[] iconsPositionsMultipliers1 = new Vector2[1] { new Vector2(.5f, .7f) };
    public static readonly Vector2[] iconsPositionsMultipliers2 = new Vector2[2]{
            new Vector2(.35f,.7f),
            new Vector2(.65f,.7f)
    };
    public static readonly Vector2[] iconsPositionsMultipliers3 = new Vector2[3] {
            new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f)
    };
    public static readonly Vector2[] iconsPositionsMultipliers4 = new Vector2[4] {
            new Vector2(.35f, .2f),
            new Vector2(.65f, .2f),
            new Vector2(.35f, .7f),
            new Vector2(.65f, .7f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers5 = new Vector2[5] {
            new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.35f, .2f),
            new Vector2(.65f, .2f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers6 = new Vector2[6] {
        new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.17f, .2f),
            new Vector2(.5f, .2f),
            new Vector2(.83f, .2f),
    };
    public static readonly Vector2[] iconsPositionsMultipliers7 = new Vector2[7] {
        new Vector2(.17f, .7f),
            new Vector2(.5f, .7f),
            new Vector2(.83f, .7f),
            new Vector2(.17f, .2f),
            new Vector2(.5f, .2f),
            new Vector2(.83f, .2f),
            new Vector2(.5f, -.3f),
    };
    public static readonly Vector2[][] iconsPositionsMultipliers = new Vector2[7][] {
        iconsPositionsMultipliers1,
        iconsPositionsMultipliers2,
        iconsPositionsMultipliers3,
        iconsPositionsMultipliers4,
        iconsPositionsMultipliers5,
        iconsPositionsMultipliers6,
        iconsPositionsMultipliers7
    };
    private readonly Dictionary<FishEffectType, RectTransform> effectsSlotsMatch = new Dictionary<FishEffectType, RectTransform> { };

    [Header("Overlays")]
    [SerializeField] Image damageOverlay; // red overlay for damage effect
    [SerializeField] Image stopTimeOverlay; // viol overlay for stop time effect
    [SerializeField] Image stunOverlay;
    [SerializeField] Image poisonOverlay;
    [SerializeField] Image blindnessOverlay;
    [SerializeField] Image zoneOverlay;
    [SerializeField] TextMeshProUGUI zoneOverlayMsg;
    [SerializeField] AudioClip damageAudio;
    private const float OVERLAYS_UPDATE_INTERVAL = 0.1f;
    private const float OVERLAY_ZONE_UPDATE_INTERVAL = 0.05f;

    [Header("Player Info")]
    [SerializeField] TextMeshProUGUI healthProgresCurr_txt;
    [SerializeField] TextMeshProUGUI healthProgresMax_txt;
    [SerializeField] TextMeshProUGUI lvlProgres_txt;
    [SerializeField] TextMeshProUGUI lvl_txt;
    [SerializeField] Image healthBar; // image to display player health bar
    [SerializeField] Image lvlBar; // image to display player level bar
    private Fish playerFish; // reference to the player fish component
    private static readonly WaitForSeconds OVERLAY_UPD_WAIT = new WaitForSeconds(OVERLAYS_UPDATE_INTERVAL);
    private static readonly WaitForSeconds HALF_OVERLAY_UPD_WAIT = new WaitForSeconds(OVERLAYS_UPDATE_INTERVAL * .5f);
    private static readonly WaitForSeconds BLINDNESS_DARK_WAIT = new WaitForSeconds(3f);

    [SerializeField] GameObject playebleAreaUI;//to take away the player's ability to move 

    public void Init(SpawnManager sp) {
        spawnManager = sp;
        //Debug.LogWarning(spawnManager);
        PlayerReConnect(spawnManager.PlayerFish);

        effectsSlotsMatch[FishEffectType.Shield] = shieldIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Stun] = stunIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.SprintSpeed] = sprintIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.CurrentSpeed] = currentIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.FreezeTime] = timeIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Poison] = poisonIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Blindness] = blindnessIcon.transform.parent.GetComponent<RectTransform>();

        spawnManager.OnPlayerRespawn += PlayerReConnect;

        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 1f);
        stopTimeOverlay.color = new Color(stopTimeOverlay.color.r, stopTimeOverlay.color.g, stopTimeOverlay.color.b, 1f);
        stunOverlay.color = new Color(stunOverlay.color.r, stunOverlay.color.g, stunOverlay.color.b, 1f);
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 1f);
        zoneOverlay.color = new Color(zoneOverlay.color.r, zoneOverlay.color.g, zoneOverlay.color.b, 1f);
        blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);
    }

    public void SetPlayableAreaActive(bool isActive) {
        //if (UIplayebleArea != null)
        playebleAreaUI.SetActive(isActive);
        JoyAxis.Axis = Vector2.zero; // reset joystick input
    }

    public void OnSpintButtonClick() {
        playerEffectManager.ApplyEffect(FishEffectType.SprintSpeed, SPRINT_EFFECT_DURATION);
        sprintButton.interactable = false; // disable the button after applying the effect
    }

    //deactivates icons visualized above the fish , since the player has a more detailed visualization in the interface
    void DisactivateDefaultIcons() {
        GameObject child = playerEffectManager.transform.Find(DEFAULT_FISH_ICONS_PARENT_NAME).gameObject;
        child.SetActive(false);

        if (shieldIcon == null)//or eny other (only for editor debug)
            return;

        shieldIcon.transform.parent.gameObject.SetActive(false);
        stunIcon.transform.parent.gameObject.SetActive(false);
        sprintIcon.transform.parent.gameObject.SetActive(false);
        currentIcon.transform.parent.gameObject.SetActive(false);
        timeIcon.transform.parent.gameObject.SetActive(false);
        poisonIcon.transform.parent.gameObject.SetActive(false);
        blindnessIcon.transform.parent.gameObject.SetActive(false);
        //shieldIcon.fillAmount = 0;
        //stunIcon.fillAmount = 0;
        //sprintIcon.fillAmount = 0;
        //currentIcon.fillAmount = 0;
        //timeIcon.fillAmount = 0;
        //poisonIcon.fillAmount = 0;
    }

    // Update is called once per frame
    void Update() {
        if (playerEffectManager == null) {
            PlayerReConnect(spawnManager.PlayerFish);
            return;
        }

        //activate and draw progress for all effects icons that have player
        EffectsProgressReset(); // reset all icons progress
                                //foreach (FishEffectType effectType in playerEffectManager.activeEffects.Keys)

        //foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
        //    if (playerEffectManager.activeEffects.TryGetValue(effectType, out Effect value)) {
        //        if (value == null) {
        //            EffectsBehaviour(effectType, false);
        //            //playerEffectManager.RemoveEffect(effectType);
        //            //Debug.LogWarning(effectType + " " + value + " " + value.LeftTime);
        //        }
        //        SetEffectProgress(effectType, value);
        //        value = null;
        //    }
        foreach (KeyValuePair<FishEffectType, Effect> eff in playerEffectManager.activeEffects)
            if (eff.Value == null || !eff.Value.Enabled)
                EffectsBehaviour(eff.Key, false);
            else  //!null & enabled
                SetEffectProgress(eff.Key, eff.Value);

        SprintButtonBehaviour();
    }

    void UpdatePlayerInfo() {
        int lvlProgress = (int)(playerFish.Progress * 100);

        healthProgresCurr_txt.text = playerFish.Health.ToString();
        healthProgresMax_txt.text = playerFish.MaxHealth.ToString();
        lvlProgres_txt.text = lvlProgress.ToString();
        healthBar.fillAmount = (float)playerFish.Health / playerFish.MaxHealth;

        lvl_txt.text = ((int)playerFish.Level).ToString();
        lvlBar.fillAmount = playerFish.Progress;
    }

    void SprintButtonBehaviour() {
        if (sprintButton.interactable) {
#if UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)//pc debug
                OnSpintButtonClick();
#endif
            return;
        }

        if (currentSprintCooldownTime >= SPRINT_BUTTON_COOLDOWN) {
            sprintButton.interactable = true;
            currentSprintCooldownTime = 0f;
            sprintButton.image.fillAmount = 1f; // reset the button fill amount
        }
        else {
            sprintButton.image.fillAmount = currentSprintCooldownTime / SPRINT_BUTTON_COOLDOWN;
            currentSprintCooldownTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// sets the progress of the effect icon that affects the player, 
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="effect"></param>
    void SetEffectProgress(FishEffectType effectType, Effect effect) {
        float progress = (effect != null) ? effect.LeftTime / effect.Duration : 0f;

        switch (effectType) {
            case FishEffectType.Stun:
                stunIcon.fillAmount = progress;
                stunIcon.transform.parent.gameObject.SetActive(stunIcon.fillAmount != 0);
                break;
            case FishEffectType.Shield:
                shieldIcon.fillAmount = progress;
                shieldIcon.transform.parent.gameObject.SetActive(shieldIcon.fillAmount != 0);
                break;
            case FishEffectType.CurrentSpeed:
                currentIcon.fillAmount = progress;
                currentIcon.transform.parent.gameObject.SetActive(currentIcon.fillAmount != 0);
                break;
            case FishEffectType.SprintSpeed:
                sprintIcon.fillAmount = progress;
                sprintIcon.transform.parent.gameObject.SetActive(sprintIcon.fillAmount != 0);
                break;
            case FishEffectType.FreezeTime:
                timeIcon.fillAmount = progress;
                timeIcon.transform.parent.gameObject.SetActive(timeIcon.fillAmount != 0);
                break;
            case FishEffectType.Poison:
                poisonIcon.fillAmount = progress;
                poisonIcon.transform.parent.gameObject.SetActive(poisonIcon.fillAmount != 0);
                break;
            case FishEffectType.Blindness:
                blindnessIcon.fillAmount = progress;
                blindnessIcon.transform.parent.gameObject.SetActive(blindnessIcon.fillAmount != 0);
                break;
        }
    }

    void EffectsProgressReset() {
        shieldIcon.transform.parent.gameObject.SetActive(false);
        stunIcon.transform.parent.gameObject.SetActive(false);
        currentIcon.transform.parent.gameObject.SetActive(false);
        sprintIcon.transform.parent.gameObject.SetActive(false);
        timeIcon.transform.parent.gameObject.SetActive(false);
        poisonIcon.transform.parent.gameObject.SetActive(false);
        blindnessIcon.transform.parent.gameObject.SetActive(false);

        stunIcon.fillAmount = 0;
        shieldIcon.fillAmount = 0;
        currentIcon.fillAmount = 0;
        sprintIcon.fillAmount = 0;
        timeIcon.fillAmount = 0;
        poisonIcon.fillAmount = 0;
        blindnessIcon.fillAmount = 0;
    }

    void ActivateDamageOverlay() {
        damageOverlay.gameObject.SetActive(true);
        if (playerFish.gameObject.TryGetComponent<AudioSource>(out AudioSource audioSource)) {
            audioSource.PlayOneShot(damageAudio);
            //Debug.LogWarning(damageAudio);
        }

        //InvokeRepeating(nameof(DamageOverlayAnim), OVERLAYS_UPDATE_INTERVAL, OVERLAYS_UPDATE_INTERVAL);
        StartCoroutine(OverlayFader(damageOverlay));
    }

    IEnumerator BlindnessOverlayAnimation(Effect effect) {
        while (effect != null || effect.LeftTime > 0) {

            while (blindnessOverlay.color.a < 1f) {//blink
                blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, blindnessOverlay.color.a + OVERLAYS_UPDATE_INTERVAL);

                if (effect == null)
                    break;

                yield return HALF_OVERLAY_UPD_WAIT;
            }

            if (effect == null)
                break;
            yield return BLINDNESS_DARK_WAIT;

            while (blindnessOverlay.color.a > 0f) {
                if (effect == null)
                    break;

                blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, blindnessOverlay.color.a - OVERLAYS_UPDATE_INTERVAL);
                yield return HALF_OVERLAY_UPD_WAIT;
            }
            //yield return null;
        }

        blindnessOverlay.gameObject.SetActive(false);
        blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);
    }

    IEnumerator OverlayFader(Image overlay, float updateInterval = OVERLAYS_UPDATE_INTERVAL, TextMeshProUGUI textElement = null) {
        while (overlay.color.a > 0) {
            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, overlay.color.a - updateInterval);

            if (textElement != null)
                textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, textElement.color.a - updateInterval);

            yield return OVERLAY_UPD_WAIT/*new WaitForSeconds(OVERLAYS_UPDATE_INTERVAL)*/;
        }
        overlay.gameObject.SetActive(false);
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
    }

    //void DamageOverlayAnim() {
    //    if (damageOverlay.color.a <= 0) {
    //        CancelInvoke(nameof(DamageOverlayAnim));
    //        damageOverlay.gameObject.SetActive(false);
    //        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 1f);
    //        return;
    //    }

    //    damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, damageOverlay.color.a - OVERLAYS_UPDATE_INTERVAL);
    //}

    void PlayerReConnect(Fish fish) {
        //Debug.LogWarning("PlayerRespawn");
        if (playerFish != null && playerEffectManager != null) {
            playerFish.OnInfoStatsChanged -= UpdatePlayerInfo;
            playerFish.OnFishDamage -= ActivateDamageOverlay;
            playerEffectManager.OnEffectChanges -= EffectsBehaviour;
        }

        if (spawnManager == null || spawnManager.PlayerFish == null)//for debug
            return;

        ////unsubscribe from events of the previous player fish if it exists
        //PlayerEventsResub(playerFish);

        playerFish = fish/*spawnManager.PlayerFish*//*pool[SpawnManager.PLAYER_INDEX].GetComponent<Fish>()*/; //get player fish component
        playerEffectManager = /*spawnManager.PlayerFish*/fish.GetComponent<EffectManager>(); //get player transform

        //syubscribe to events of the new player fish
        playerFish.OnInfoStatsChanged += UpdatePlayerInfo; // subscribe to player fish stats change event
        playerFish.OnFishDamage += ActivateDamageOverlay;
        playerEffectManager.OnEffectChanges += EffectsBehaviour;

        sprintButton.interactable = true;
        sprintButton.image.fillAmount = 1f; // reset the button fill amount
        currentSprintCooldownTime = 0f;

        DisactivateDefaultIcons();
        //EffectsProgressReset();

        ResetOverlays();
        
        //upd icons pos of spawn effects
        foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
            if (playerEffectManager.activeEffects.TryGetValue(effectType, out Effect value) && value != null)
                EffectsBehaviour(effectType, true);

        UpdatePlayerInfo();
    }

    private void ResetOverlays() {
        poisonOverlay.gameObject.SetActive(false);
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 1f);
        stunOverlay.gameObject.SetActive(false);
        stunOverlay.color = new Color(stunOverlay.color.r, stunOverlay.color.g, stunOverlay.color.b, 1f);
        damageOverlay.gameObject.SetActive(false);
        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 1f);
        stopTimeOverlay.gameObject.SetActive(false);
        stopTimeOverlay.color = new Color(stopTimeOverlay.color.r, stopTimeOverlay.color.g, stopTimeOverlay.color.b, 1f);
        blindnessOverlay.gameObject.SetActive(false);
        blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);

        StopAllCoroutines();
    }

    public void ActivateZoneOverlay(string msg) {
        zoneOverlayMsg.color = new Color(zoneOverlayMsg.color.r, zoneOverlayMsg.color.g, zoneOverlayMsg.color.b, 1f);
        if (zoneOverlay.gameObject.activeInHierarchy)
            return;//corutine is already running (wait for next call)

        //Debug.LogWarning("ActivateZone");
        zoneOverlay.gameObject.SetActive(true);
        zoneOverlayMsg.text = msg;
        StartCoroutine(OverlayFader(zoneOverlay, OVERLAY_ZONE_UPDATE_INTERVAL, zoneOverlayMsg));
    }

    /// <summary>
    /// for any additional behaviour (in UI) when an effect is added or removed from the player
    /// </summary>
    /// <param name="type"></param>
    /// <param name="added"></param>
    void EffectsBehaviour(FishEffectType type, bool isAdded) {
        if (type == FishEffectType.StunInitiator)
            return;

        const float MAX_EFFECTS_CNT = 5; // max effects count that can be displayed in the UI
                                         //repos
        int effectsCnt = playerEffectManager./*ActiveEffectsCnt*/GetActiveEffectCnt(TypeEffects.VisibleUI)/*activeEffects.Count*/;

        if (!isAdded)//if effect expires - hide icon
            effectsSlotsMatch[type].gameObject.SetActive(false);

        if (effectsCnt > 0) {//reposing
            int effectIconInd = 0;
            foreach (FishEffectType currType in playerEffectManager.activeEffects.Keys) {
                if (!EffectManager.IsVisibleEffect(currType, true)
                    || playerEffectManager.activeEffects[currType] == null || !playerEffectManager.activeEffects[currType].Enabled)
                    continue;

                //Debug.Log(effectsCnt - 1 + " " + effectIconInd);
                Vector2 newPos = new Vector2(
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x * iconsParent.rect.width,
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y * iconsParent.rect.height);
                //Debug.LogWarning(iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x + " "
                //+ iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y);

                effectsSlotsMatch[currType].anchoredPosition = newPos;

                effectIconInd = (effectIconInd + 1 < MAX_EFFECTS_CNT) ? ++effectIconInd : /*--*/effectIconInd;
            }
        }

        //add overlays
        Image currentOverlay = type switch {
            FishEffectType.Stun => stunOverlay,
            FishEffectType.Poison => poisonOverlay,
            FishEffectType.FreezeTime => stopTimeOverlay,
            FishEffectType.Blindness => blindnessOverlay,
            _ => null
        };

        if (currentOverlay != null) {
            if (type == FishEffectType.Blindness) {
                if (isAdded) {
                    Effect effect = playerEffectManager.activeEffects[FishEffectType.Blindness];
                    blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);

                    StopCoroutine(BlindnessOverlayAnimation(effect));
                    StartCoroutine(BlindnessOverlayAnimation(effect));
                }
                else
                    return;
            }

            if (isAdded)
                currentOverlay.gameObject.SetActive(true);
            else if(currentOverlay.gameObject.activeInHierarchy)
                StartCoroutine(OverlayFader(currentOverlay));
            //InvokeRepeating(nameof(StopTimeOverlayAnimFading), OVERLAYS_UPDATE_INTERVAL, OVERLAYS_UPDATE_INTERVAL);
        }
    }

    //void StopTimeOverlayAnimFading() {
    //    if (stopTimeOverlay.color.a <= 0) {
    //        stopTimeOverlay.gameObject.SetActive(true);
    //        CancelInvoke(nameof(StopTimeOverlayAnimFading));
    //    }

    //    stopTimeOverlay.color = new Color(stopTimeOverlay.color.r, stopTimeOverlay.color.g, stopTimeOverlay.color.b, stopTimeOverlay.color.a - OVERLAYS_UPDATE_INTERVAL);
    //}

    public void OnDestroy() {
        PlayerReConnect(playerFish);
    }
}
