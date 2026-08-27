using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;


/// <summary>
/// game hud
/// </summary>
/// 
public class PlayerUIController : MonoBehaviour {
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference cancelAction;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] MainmenuManager mainmenu;

    [Header("Icons for effects (UI)")]
    [SerializeField] RectTransform iconsParent;
    [SerializeField] Image shieldIcon;
    [SerializeField] Image stunIcon;
    [SerializeField] Image sprintIcon;
    [SerializeField] Image currentIcon;
    [SerializeField] Image timeIcon;
    [SerializeField] Image poisonIcon;
    [SerializeField] Image blindnessIcon;
    [SerializeField] Button sprintButton; // button for sprint effect activation
                                          //[SerializeField] AudioClip damageAudio;
    [SerializeField] AudioManager audioManager;

    [Header("Overlays")]
    [SerializeField] Image damageOverlay; // red overlay for damage effect
    [SerializeField] Image stopTimeOverlay; // viol overlay for stop time effect
    [SerializeField] Image stunOverlay;
    [SerializeField] Image poisonOverlay;
    [SerializeField] Image blindnessOverlay;
    [SerializeField] Image zoneOverlay;
    [SerializeField] TextMeshProUGUI zoneOverlayMsg;
    [SerializeField] LocalizedString coralReefClear;
    [SerializeField] LocalizedString seaweedClear;

    [Header("Player Info")]
    //[SerializeField] TextMeshProUGUI healthProgresCurr_txt;
    //[SerializeField] TextMeshProUGUI healthProgresMax_txt;
    //[SerializeField] TextMeshProUGUI lvlProgres_txt;
    //[SerializeField] TextMeshProUGUI lvl_txt;
    //[SerializeField] TextMeshProUGUI score_txt;
    //[SerializeField] Image healthBar; // image to display player health bar
    //[SerializeField] Image lvlBar; // image to display player level bar
    //[SerializeField] Transform playerProgressIconsParent;
    [SerializeField] ScoreBar scoreBar;
    [SerializeField] PlayerGrowthBar growthBar;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GameObject playebleAreaUI;//to take away the player's ability to move 
    [SerializeField] Image sprintProgressImage;
    [SerializeField] RectTransform sprintProgressPC;
    [SerializeField] RectTransform sprintPC_Parent;

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
    public const float VIEW_HUD_TIME = 1f;
    public const float VIEW_HEALTH_TIME = 2f;
    public const float VIEW_JAW_TIME = .8f;
    public const float FADE_HUD_TIME = 1.5f;

    private SpawnManager spawnManager;
    private EffectManager playerEffectManager;
    private float currentSprintCooldownTime = 0;
    //private FishLevel currentProgressIco = FishLevel.SeaBass;
    private Fish playerFish; // reference to the player fish component
    //private bool isPlayableAreaActive = true;
    private Coroutine zoneCoroutine;

    private readonly Dictionary<FishEffectType, RectTransform> effectsSlotsMatch = new Dictionary<FishEffectType, RectTransform> { };//private Dictionary<FishEffectType, Coroutine> overlayCoroutines = new Dictionary<FishEffectType, Coroutine>();
    private static readonly WaitForSecondsRealtime OVERLAY_UPD_WAIT = new WaitForSecondsRealtime(OVERLAYS_UPDATE_INTERVAL);
    private static readonly WaitForSecondsRealtime HALF_OVERLAY_UPD_WAIT = new WaitForSecondsRealtime(OVERLAYS_UPDATE_INTERVAL * .5f);
    private static readonly WaitForSecondsRealtime BLINDNESS_DARK_WAIT = new WaitForSecondsRealtime(1.4f);
    private static readonly WaitForSecondsRealtime FOUR_SECONDS_WAIT = new WaitForSecondsRealtime(4f);
    //private static readonly Vector3 SPRINT_BAR_SCALE_MULTIPLIER = new Vector3(0f, 1f, 1f);

    private const string DEFAULT_FISH_ICONS_PARENT_NAME = "effects_icons";
    private const float SPRINT_BUTTON_COOLDOWN = 20f;
    private const float SPRINT_EFFECT_DURATION = 5f;
    private const float OVERLAYS_UPDATE_INTERVAL = 0.1f;
    private const float OVERLAYS_UPDATE_VALUE = OVERLAYS_UPDATE_INTERVAL * .2f;
    private const float OVERLAY_ZONE_UPDATE_INTERVAL = 0.05f;
    private const float SPRINT_PROGRESS_PC_LOADING_ALPHA = 0.3f;


    public void Init(SpawnManager sp) {
        spawnManager = sp;
        //Debug.LogWarning(spawnManager);

        effectsSlotsMatch[FishEffectType.Shield] = shieldIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Stun] = stunIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.SprintSpeed] = sprintIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.CurrentSpeed] = currentIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.FreezeTime] = timeIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Poison] = poisonIcon.transform.parent.GetComponent<RectTransform>();
        effectsSlotsMatch[FishEffectType.Blindness] = blindnessIcon.transform.parent.GetComponent<RectTransform>();

        spawnManager.OnPlayerRespawn += PlayerReConnect;
        PlayerLeaderboardScore.OnChanged += UpdateScoreInfo;
        SettingsUI.OnHudHideChanged += UpdateHUDbars;

        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 1f);
        stopTimeOverlay.color = new Color(stopTimeOverlay.color.r, stopTimeOverlay.color.g, stopTimeOverlay.color.b, 1f);
        stunOverlay.color = new Color(stunOverlay.color.r, stunOverlay.color.g, stunOverlay.color.b, 1f);
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 1f);
        zoneOverlay.color = new Color(zoneOverlay.color.r, zoneOverlay.color.g, zoneOverlay.color.b, 1f);
        blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);

        if (GameManager.IsRunOnPC) {
            playebleAreaUI.SetActive(false);
            sprintProgressPC.transform.parent.gameObject.SetActive(true);
        }
        else {
            sprintProgressPC.transform.parent.gameObject.SetActive(false);
            playebleAreaUI.SetActive(true);
        }
    }

    private void Start() {
        PlayerReConnect(spawnManager.PlayerFish, false);//in start for access to objects corutines etc.
    }

    void OnEnable() {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        jumpAction.action.performed += OnSprint;
        cancelAction.action.Enable();
        cancelAction.action.performed += OnPauseWnd;
    }

    void OnDisable() {
        moveAction.action.Disable();
        jumpAction.action.performed -= OnSprint;
        jumpAction.action.Disable();
        cancelAction.action.performed -= OnPauseWnd;
        cancelAction.action.Disable();
    }

    void OnSprint(InputAction.CallbackContext ctx) {
        if (sprintButton.interactable) {
            //for Googlr Play Games PC
            //if (GameManager.IsRunOnPC && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)//pc debug
            OnSpintButtonClick();

            return;
        }
    }

    void OnPauseWnd(InputAction.CallbackContext ctx) {
        if (pauseMenu.activeInHierarchy == GameManager.IsGamePaused)
            mainmenu.ActivatePauseWnd(!GameManager.IsGamePaused);
    }

    //public void SetPlayableAreaActive(bool isActive) {
    //    //#if !UNITY_EDITOR && !PLAY_GAMES_PC
    //    //        playebleAreaUI.SetActive(isActive);
    //    //#endif
    //    //isPlayableAreaActive = isActive;
    //    Debug.LogWarning("Playable area always active!");
    //    //JoyAxis.Axis = Vector2.zero; // reset joystick input
    //}

    public void OnSpintButtonClick() {
        playerEffectManager.ApplyEffect(FishEffectType.SprintSpeed, SPRINT_EFFECT_DURATION);
        sprintButton.interactable = false; // disable the button after applying the effect
        currentSprintCooldownTime = 0f; // reset the cooldown timer
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
    }

    // Update is called once per frame
    void Update() {
        //activate and draw progress for all effects icons that have player
        EffectsProgressReset(); // reset all icons progress

        if (playerEffectManager != null && playerEffectManager.activeEffects.Count > 0)
            foreach (KeyValuePair<FishEffectType, Effect> eff in playerEffectManager.activeEffects)
                if (eff.Value == null || !eff.Value.Enabled)
                    EffectsBehaviour(eff.Key, false);
                else  //!null & enabled
                    SetEffectProgress(eff.Key, eff.Value);

        //if (isPlayableAreaActive)
        SprintButtonBehaviour();

        if (GameManager.IsRunOnPC)
            WASD_MovingBehaviour();
    }

    void WASD_MovingBehaviour() {
        if (Keyboard.current == null)
            return;

        //Vector2 moveInput = Vector2.zero;
        //if (Keyboard.current.wKey.isPressed)
        //    moveInput.y += 1f;
        //if (Keyboard.current.sKey.isPressed)
        //    moveInput.y -= 1f;
        //if (Keyboard.current.aKey.isPressed)
        //    moveInput.x -= 1f;
        //if (Keyboard.current.dKey.isPressed)
        //    moveInput.x += 1f;

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        JoyAxis.Axis = moveInput.normalized; // set the joystick axis to the normalized input vector
    }

    void UpdateProgressInfo() {
        int lvlProgress = (int)(playerFish.Progress * GameStaticInfo.PERCENT_MULTIPLIER);
        if (growthBar.Coroutine != null)
            StopCoroutine(growthBar.Coroutine);

        if (gameObject.activeInHierarchy)
            growthBar.Coroutine = StartCoroutine(growthBar.Animation());
        else
            growthBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);

        growthBar.progress.text = lvlProgress.ToString();
        growthBar.GetProgress(playerFish.Level).fillAmount = playerFish.Progress;
    }

    void UpdateHealthInfo() {
        float health = playerFish.Health;
        if (health < 0)
            health = 0f;
        if (healthBar.Coroutine != null)
            StopCoroutine(healthBar.Coroutine);

        healthBar.currProgress.text = health.ToString();
        healthBar.maxProgress.text = playerFish.DefaultInfo.MaxHealth.ToString();
        healthBar.filler.fillAmount = (float)playerFish.Health / playerFish.DefaultInfo.MaxHealth;

        if (gameObject.activeInHierarchy)
            healthBar.Coroutine = StartCoroutine(healthBar.Animation(healthBar.filler.fillAmount));
        else
            healthBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);
    }

    void UpdateScoreInfo(long score) {
        if (scoreBar.Coroutine != null)
            StopCoroutine(scoreBar.Coroutine);

        if (gameObject.activeInHierarchy)
            scoreBar.Coroutine = StartCoroutine(scoreBar.Animation());
        else
            scoreBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);

        scoreBar.score.text = score.ToString();
    }

    void SprintButtonBehaviour() {
        if (sprintButton.interactable) {
            //    //for Googlr Play Games PC
            //    if (GameManager.IsRunOnPC && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)//pc debug
            //        OnSpintButtonClick();

            return;
        }

        if (currentSprintCooldownTime >= SPRINT_BUTTON_COOLDOWN) {
            sprintButton.interactable = true;
            currentSprintCooldownTime = 0f;
            sprintButton.image.fillAmount = 1f; // reset the button fill amount
            //sprintProgressPC.transform.localScale = Vector3.one;//fillAmount = 1f;
            SetSprintProgressPC(1f);

            sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, 1f);
        }
        else {
            sprintButton.image.fillAmount = currentSprintCooldownTime / SPRINT_BUTTON_COOLDOWN;
            //sprintProgressPC.transform.localScale = Vector3.right * sprintButton.image.fillAmount + SPRINT_BAR_SCALE_MULTIPLIER;//fillAmount = sprintButton.image.fillAmount;
            SetSprintProgressPC(sprintButton.image.fillAmount);
            sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, SPRINT_PROGRESS_PC_LOADING_ALPHA);
            currentSprintCooldownTime += Time.deltaTime;
        }
    }

    void SetSprintProgressPC(float fillAmount) {
        const float OFFSET_AMOUNT = 20f;
        fillAmount = Mathf.Clamp01(fillAmount);

        float parentWidth = sprintPC_Parent.rect.width;
        float targetHalf = Mathf.Lerp(parentWidth - OFFSET_AMOUNT, 0f, fillAmount) / 2f;

        sprintProgressPC.offsetMin = new Vector2(targetHalf, sprintProgressPC.offsetMax.y);//left
        sprintProgressPC.offsetMax = new Vector2(-targetHalf, sprintProgressPC.offsetMax.y);//right
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
        shieldIcon?.transform.parent.gameObject.SetActive(false);
        stunIcon?.transform.parent.gameObject.SetActive(false);
        currentIcon?.transform.parent.gameObject.SetActive(false);
        sprintIcon?.transform.parent.gameObject.SetActive(false);
        timeIcon?.transform.parent.gameObject.SetActive(false);
        poisonIcon?.transform.parent.gameObject.SetActive(false);
        blindnessIcon?.transform.parent.gameObject.SetActive(false);

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
        audioManager.PlayVFX(VFX_clip.Damage);
        StartCoroutine(OverlayFader(damageOverlay, FishEffectType.None));
    }

    IEnumerator BlindnessOverlayAnimation(Effect effect) {
        const float OVERLAY_MAX_ALPHA = 1f;
        const float OVERLAY_MIN_ALPHA = .2f;

        while (effect != null && effect.Enabled) {

            while (blindnessOverlay.color.a < OVERLAY_MAX_ALPHA) {//blink
                blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, blindnessOverlay.color.a + OVERLAYS_UPDATE_INTERVAL);

                if (effect == null || !effect.Enabled)
                    break;

                yield return HALF_OVERLAY_UPD_WAIT;
            }

            if (effect != null && effect.Enabled)
                yield return BLINDNESS_DARK_WAIT;

            while (blindnessOverlay.color.a > OVERLAY_MIN_ALPHA) {
                if (effect == null || !effect.Enabled)
                    break;

                blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, blindnessOverlay.color.a - OVERLAYS_UPDATE_INTERVAL);
                yield return HALF_OVERLAY_UPD_WAIT;
            }
        }

        while (blindnessOverlay.color.a > 0f) {//final fade out
            blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, blindnessOverlay.color.a - OVERLAYS_UPDATE_INTERVAL);
            yield return HALF_OVERLAY_UPD_WAIT;
        }

        blindnessOverlay.gameObject.SetActive(false);

        blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);

    }

    IEnumerator OverlayFader(Image overlay, FishEffectType type, float updateInterval = OVERLAYS_UPDATE_VALUE, TextMeshProUGUI textElement = null, bool isLongDuration = false) {
        if (isLongDuration)
            yield return FOUR_SECONDS_WAIT;

        while (overlay.color.a > 0 && overlay.gameObject.activeInHierarchy) {
            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, overlay.color.a - updateInterval);

            if (textElement != null)
                textElement.color = new Color(textElement.color.r, textElement.color.g, textElement.color.b, textElement.color.a - updateInterval);

            yield return OVERLAY_UPD_WAIT;
        }

        overlay.gameObject.SetActive(false);
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
    }

    void PlayerReConnect(Fish fish, bool isDie) {
        //Debug.LogWarning("PlayerRespawn");
        if (playerFish != null && playerEffectManager != null) {
            playerFish.OnProgressChanged -= UpdateProgressInfo;
            playerFish.OnHealthChanged -= UpdateHealthInfo;
            playerFish.OnFishDamage -= ActivateDamageOverlay;
            playerEffectManager.OnEffectChanges -= EffectsBehaviour;
        }

        if (/*spawnManager == null || spawnManager.PlayerFish == null ||*/
            fish == null || sprintProgressPC == null || sprintButton == null)//for debug
            return;

        playerFish = fish;
        playerEffectManager = fish.GetComponent<EffectManager>(); //get player transform

        //syubscribe to events of the new player fish
        playerFish.OnProgressChanged += UpdateProgressInfo;
        playerFish.OnHealthChanged += UpdateHealthInfo;
        playerFish.OnFishDamage += ActivateDamageOverlay;
        playerEffectManager.OnEffectChanges += EffectsBehaviour;

        sprintButton.interactable = true;
        sprintButton.image.fillAmount = 1f; // reset the button fill amount
                                            //sprintProgressPC.transform.localScale = Vector3.one;//fillAmount = 1f;
        SetSprintProgressPC(1);
        sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, 1f);
        currentSprintCooldownTime = 0f;

        DisactivateDefaultIcons();
        EffectsProgressReset();

        ResetOverlays();

        //upd icons pos of spawn effects
        foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
            if (playerEffectManager.activeEffects.TryGetValue(effectType, out Effect value) && value != null)
                EffectsBehaviour(effectType, true);

        UpdateHUDbars();
        growthBar.ActivateIcon(/*currentProgressIco*/fish.Level);
        //growthBar.GetProgress(fish.Level).fillAmount = fish.Progress;

        if (!isDie) {//lvl upgr & unlock new biomes
            if (fish.Level == FishLevel.Moray)
                ActivateZoneOverlay(coralReefClear.GetLocalizedString(), true);
            else if (fish.Level == FishLevel.HammerShark)
                ActivateZoneOverlay(seaweedClear.GetLocalizedString(), true);
        }
    }

    void UpdateHUDbars() {
        UpdateHealthInfo();
        UpdateProgressInfo();
        UpdateScoreInfo(PlayerLeaderboardScore.Score);
        //UpdateProgressIcon();
    }

    //void UpdateProgressIcon() {
    //FishLevel maxIndex = FishLevel.WhiteShark;//playerProgressIconsParent.childCount - 1;

    //if (currentProgressIco <= maxIndex)
    //    playerProgressIconsParent.GetChild(currentProgressIco).gameObject.SetActive(false);


    //currentPlayerProgressIconInd = (int)playerFish.Level - 1;
    //if (currentProgressIco <= maxIndex)
    //    playerProgressIconsParent.GetChild(currentProgressIco).gameObject.SetActive(true);
    //}

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
        zoneOverlay.gameObject.SetActive(false);
        zoneOverlay.color = new Color(zoneOverlay.color.r, zoneOverlay.color.g, zoneOverlay.color.b, 1f);

        StopAllCoroutines();
    }

    public void ActivateZoneOverlay(string msg, bool isLongDuration = false) {
        zoneOverlayMsg.color = new Color(zoneOverlayMsg.color.r, zoneOverlayMsg.color.g, zoneOverlayMsg.color.b, 1f);
        if (zoneOverlay.gameObject.activeInHierarchy)
            return;//corutine is already running (wait for next call)

        //Debug.LogWarning("ActivateZone");
        zoneOverlay.gameObject.SetActive(true);
        zoneOverlayMsg.text = msg;
        if (zoneCoroutine != null)
            StopCoroutine(zoneCoroutine);
        zoneCoroutine = StartCoroutine(OverlayFader(zoneOverlay, FishEffectType.None, OVERLAY_ZONE_UPDATE_INTERVAL, zoneOverlayMsg, isLongDuration));
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
        int effectsCnt = playerEffectManager.GetActiveEffectCnt(TypeEffects.VisibleUI)/*activeEffects.Count*/;

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
                    blindnessOverlay.gameObject.SetActive(true);
                    StartCoroutine(BlindnessOverlayAnimation(effect));
                }
                else
                    return;
            }
            else {

                if (isAdded) {
                    currentOverlay.color = new Color(currentOverlay.color.r, currentOverlay.color.g, currentOverlay.color.b, 1);
                    currentOverlay.gameObject.SetActive(true);
                }
                else
                    StartCoroutine(OverlayFader(currentOverlay, type));
            }
        }
    }

    public void OnDestroy() {
        PlayerReConnect(playerFish, false);
        PlayerLeaderboardScore.OnChanged -= UpdateScoreInfo;
        SettingsUI.OnHudHideChanged -= UpdateHUDbars;
        StopAllCoroutines();
    }
}
