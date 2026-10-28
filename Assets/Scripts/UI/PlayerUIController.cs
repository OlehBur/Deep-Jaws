using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using VContainer;


/// <summary>
/// game hud
/// </summary>
/// 
public class PlayerUIController : MonoBehaviour {
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference cancelAction;
    [SerializeField] GameObject pauseMenu;
    //[SerializeField] MainmenuManager mainmenu;

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

    [Header("Overlays")]
    [SerializeField] Volume mainVolume;
    [SerializeField] LocalizedString coralReefClear;
    [SerializeField] LocalizedString seaweedClear;

    [Header("Player Info")]
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
    private static readonly WaitForSecondsRealtime OVERLAY_UPD_WAIT = new WaitForSecondsRealtime(OVERLAYS_UPDATE_INTERVAL);
    private static readonly WaitForSecondsRealtime FOUR_SECONDS_WAIT = new WaitForSecondsRealtime(4f);
    private static readonly Color VIGNETTE_DEFAULT_COLOR = Color.black;
    private static readonly Color VIGNETTE_DAMAGE_COLOR = Color.red;

    //private SpawnManager _spawnManager;
    private EffectManager _playerEffectManager;
    private float _currentSprintCooldownTime = 0;
    private Fish _playerFish; // reference to the player fish component
    //private Coroutine _zoneCoroutine;
    private Vignette _vignette = null;
    private LensDistortion _lensDistortion = null;
    private ChromaticAberration _chromaticAberration = null;
    private ColorAdjustments _colorAdjustments = null;
    private DepthOfField _depthOfField = null;
    private Coroutine _damageCoroutine;
    private Coroutine _stunCoroutine;
    private Coroutine _poisonCoroutine;
    private Coroutine _freezeCoroutine;
    private Coroutine _blindnessCoroutine;

    [Inject] private readonly IGameTip _gameTips;
    [Inject] private readonly IGameHudController _gameHudController;
    [Inject] private readonly IAudioPlayService _audioManager;
    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    private readonly Dictionary<FishEffectType, RectTransform> _effectsSlotsMatch 
        = new Dictionary<FishEffectType, RectTransform> { };//private Dictionary<FishEffectType, Coroutine> overlayCoroutines = new Dictionary<FishEffectType, Coroutine>();

    public const float VIEW_HUD_TIME = 1f;
    public const float VIEW_HEALTH_TIME = 2f;
    public const float VIEW_JAW_TIME = .8f;
    public const float FADE_HUD_TIME = 1.5f;
    private const float VIGNETTE_INTENS_DAMAGE = 0.56f;
    private const float VIGNETTE_INTENS_DEFAULT = 0.417f;
    private const string DEFAULT_FISH_ICONS_PARENT_NAME = "effects_icons";
    private const float SPRINT_BUTTON_COOLDOWN = 20f;
    private const float SPRINT_EFFECT_DURATION = 5f;
    private const float OVERLAYS_UPDATE_INTERVAL = 0.1f;
    private const float OVERLAYS_UPDATE_VALUE = OVERLAYS_UPDATE_INTERVAL * .2f;
    private const float SPRINT_PROGRESS_PC_LOADING_ALPHA = 0.3f;

    private const float LENS_DISORTION_MAX_INTENS = -0.7f;
    private const float LENS_DISORTION_DEFAULT_INTENS = 0f;
    private const float LENS_DISORTION_OFFSET_MULT = .08f;
    private const float LENS_DISORTION_OFFSET_CENTER = .5f;
    private const float LENS_DISORTION_MAX_SCALE = 1.17f;
    private const float LENS_DISORTION_DEFAULT_SCALE = 1f;
    private const float LENS_DISORTION_OFFSET_SPEED = .5f;//in rad/s
    private const float CHROMATICABERRATION_DEFAULT_INTENSITY = 0;
    private const float CHROMATICABERRATION_MAX_INTENSITY = 1;
    private const float COLORADJUSTMENTS_DEFAULT_HUE = 0;
    private const float COLORADJUSTMENTS_RANGE_HUE_MULT = 180f;
    private const float COLORADJUSTMENTS_DEFAULT_SATURATION = 0f;
    private const float COLORADJUSTMENTS_MAX_SATURATION = 100f;
    private const float EFFECTS_FADE_DURATION = 1.5f;
    private const float DEPTHOFFIELD_DEFAULT_START = 26f;
    private const float DEPTHOFFIELD_DEFAULT_END = 30f;
    private const float DEPTHOFFIELD_MIN_STARTEND = 0f;


    void Start(/*SpawnManager sp*/) {
        //_spawnManager = sp;
        //Debug.LogWarning(spawnManager);

        _effectsSlotsMatch[FishEffectType.Shield] = shieldIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.Stun] = stunIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.SprintSpeed] = sprintIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.CurrentSpeed] = currentIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.FreezeTime] = timeIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.Poison] = poisonIcon.transform.parent.GetComponent<RectTransform>();
        _effectsSlotsMatch[FishEffectType.Blindness] = blindnessIcon.transform.parent.GetComponent<RectTransform>();

        _playerFishPoolService.OnPlayerRespawn += PlayerReConnect;
        PlayerLeaderboardScore.OnChanged += UpdateScoreInfo;
        SettingsUI.OnHudHideChanged += UpdateHUDbars;

        if (GameManager.IsRunOnPC) {
            playebleAreaUI.SetActive(false);
            sprintProgressPC.transform.parent.gameObject.SetActive(true);
        }
        else {
            sprintProgressPC.transform.parent.gameObject.SetActive(false);
            playebleAreaUI.SetActive(true);
        }

        if (mainVolume.sharedProfile.TryGet(out Vignette vignette))
            _vignette = vignette;
        if (mainVolume.sharedProfile.TryGet(out LensDistortion lensDistortion))
            _lensDistortion = lensDistortion;
        if (mainVolume.sharedProfile.TryGet(out ChromaticAberration chromaticAberration))
            _chromaticAberration = chromaticAberration;
        if (mainVolume.sharedProfile.TryGet(out ColorAdjustments colorAdjustments))
            _colorAdjustments = colorAdjustments;
        if (mainVolume.sharedProfile.TryGet(out DepthOfField depthOfField)) {
            _depthOfField = depthOfField;
            _depthOfField.mode.value = DepthOfFieldMode.Gaussian;
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
            _depthOfField.gaussianEnd.value = DEPTHOFFIELD_DEFAULT_END;
        }

        PlayerReConnect(_playerFishPoolService.PlayerCurrentFish, false);//in start for access to objects corutines etc.
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

        ResetVisualEffects();
    }

    void ResetVisualEffects() {
        if (_vignette != null) {
            _vignette.intensity.value = VIGNETTE_INTENS_DEFAULT;
            _vignette.color.value = VIGNETTE_DEFAULT_COLOR;
        }
        if (_lensDistortion != null) {
            _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
            _lensDistortion.center.value = Vector2.one * LENS_DISORTION_OFFSET_CENTER;
            _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;
        }
        if (_chromaticAberration != null) {
            _chromaticAberration.intensity.value = CHROMATICABERRATION_DEFAULT_INTENSITY;
        }
        if (_colorAdjustments != null) {
            _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
            _colorAdjustments.saturation.value = COLORADJUSTMENTS_DEFAULT_SATURATION;
        }
        if (_depthOfField != null) {
            _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
        }
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
            _gameHudController.ActivatePauseWnd(!GameManager.IsGamePaused);
    }

    public void OnSpintButtonClick() {
        _playerEffectManager.ApplyEffect(FishEffectType.SprintSpeed, SPRINT_EFFECT_DURATION);
        sprintButton.interactable = false; // disable the button after applying the effect
        _currentSprintCooldownTime = 0f; // reset the cooldown timer
    }

    //deactivates icons visualized above the fish , since the player has a more detailed visualization in the interface
    void DisactivateDefaultIcons() {
        GameObject child = _playerEffectManager.transform.Find(DEFAULT_FISH_ICONS_PARENT_NAME).gameObject;
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

        if (_playerEffectManager != null && _playerEffectManager.ActiveEffects.Count > 0)
            foreach (KeyValuePair<FishEffectType, Effect> eff in _playerEffectManager.ActiveEffects)
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

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        JoyAxis.Axis = moveInput.normalized; // set the joystick axis to the normalized input vector
    }

    void UpdateProgressInfo(ProgressArgs args = null) {
        int lvlProgress = (int)(_playerFish.Progress * GameStaticInfo.PERCENT_MULTIPLIER);
        if (growthBar.Coroutine != null)
            StopCoroutine(growthBar.Coroutine);

        if (gameObject.activeInHierarchy)
            growthBar.Coroutine = StartCoroutine(growthBar.Animation());
        else
            growthBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);

        growthBar.progress.text = lvlProgress.ToString();
        growthBar.GetProgress(_playerFish.Level).fillAmount = _playerFish.Progress;

        if (args != null && args.IsCaviarEaten)
            _gameTips.OnEffectTip();
    }

    void UpdateHealthInfo() {
        float health = _playerFish.Health;
        if (health < 0)
            health = 0f;
        if (healthBar.Coroutine != null)
            StopCoroutine(healthBar.Coroutine);

        healthBar.currProgress.text = health.ToString();
        healthBar.maxProgress.text = _playerFish.DefaultInfo.MaxHealth.ToString();
        healthBar.filler.fillAmount = (float)_playerFish.Health / _playerFish.DefaultInfo.MaxHealth;

        if (gameObject.activeInHierarchy)
            healthBar.Coroutine = StartCoroutine(healthBar.Animation(healthBar.filler.fillAmount));
        else
            healthBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);
    }

    void UpdateScoreInfo(long score) {
        if (scoreBar.Coroutine != null)
            StopCoroutine(scoreBar.Coroutine);

        if (gameObject == null)
            return;

        if (gameObject.activeInHierarchy)
            scoreBar.Coroutine = StartCoroutine(scoreBar.Animation());
        else
            scoreBar.ChangeTransparancy(SettingsUI.IsAutoHideUI ? 0f : 1f);

        scoreBar.score.text = score.ToString();
    }

    void SprintButtonBehaviour() {
        if (sprintButton.interactable)
            return;

        if (_currentSprintCooldownTime >= SPRINT_BUTTON_COOLDOWN) {
            sprintButton.interactable = true;
            _currentSprintCooldownTime = 0f;
            sprintButton.image.fillAmount = 1f; // reset the button fill amount
            SetSprintProgressPC(1f);

            sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, 1f);
        }
        else {
            sprintButton.image.fillAmount = _currentSprintCooldownTime / SPRINT_BUTTON_COOLDOWN;
            SetSprintProgressPC(sprintButton.image.fillAmount);
            sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, SPRINT_PROGRESS_PC_LOADING_ALPHA);
            _currentSprintCooldownTime += DJTime.UnscaledDeltaTime;
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
        //damageOverlay.gameObject.SetActive(true);
        _audioManager.PlayVFX(VFX_clip.Damage);
        //StartCoroutine(OverlayFader(damageOverlay, FishEffectType.None));
        if (_damageCoroutine != null)
            StopCoroutine(_damageCoroutine);
        _damageCoroutine = StartCoroutine(DamageVignette());
    }

    IEnumerator DamageVignette() {
        float deltaSmooth = 0;
        float normDelta = 0;
        _vignette.color.value = VIGNETTE_DAMAGE_COLOR;
        _vignette.intensity.value = VIGNETTE_INTENS_DAMAGE;

        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

            _vignette.color.value = Color.Lerp(VIGNETTE_DAMAGE_COLOR, VIGNETTE_DEFAULT_COLOR, normDelta);
            _vignette.intensity.value = Mathf.Lerp(VIGNETTE_INTENS_DAMAGE, VIGNETTE_INTENS_DEFAULT, normDelta);

            yield return null;
        }

        _vignette.color.value = VIGNETTE_DEFAULT_COLOR;
        _vignette.intensity.value = VIGNETTE_INTENS_DEFAULT;
        _damageCoroutine = null;
    }

    IEnumerator FreezeTimeEffect() {
        bool isFreeze = true;
        float deltaSmooth = 0;
        float normDelta = 0;
        _chromaticAberration.intensity.value = CHROMATICABERRATION_MAX_INTENSITY;

        //while (deltaSmooth < EFFECTS_FADE_DURATION) {
        //    deltaSmooth += DJTime.UnscaledDeltaTime;
        //    normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

        //    chromaticAberration.intensity.value = Mathf.Lerp(CHROMATICABERRATION_DEFAULT_INTENSITY, CHROMATICABERRATION_MAX_INTENSITY, normDelta);

        //    yield return OVERLAY_UPD_WAIT;
        //}
        //chromaticAberration.intensity.value = CHROMATICABERRATION_MAX_INTENSITY;

        while (isFreeze) {
            isFreeze = _playerEffectManager.ActiveEffects.ContainsKey(FishEffectType.FreezeTime)
                && _playerEffectManager.ActiveEffects[FishEffectType.FreezeTime].Enabled;
            yield return null;
        }

        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

            _chromaticAberration.intensity.value = Mathf.Lerp(CHROMATICABERRATION_MAX_INTENSITY, CHROMATICABERRATION_DEFAULT_INTENSITY, normDelta);

            yield return null;
        }

        _chromaticAberration.intensity.value = CHROMATICABERRATION_DEFAULT_INTENSITY;
        _freezeCoroutine = null;
    }

    IEnumerator BlindnessEffect() {
        bool isBlindness = true;
        float hueShift = 0f;
        float deltaSmooth = 0;
        float normDelta = 0;

        _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
        _depthOfField.gaussianStart.value = DEPTHOFFIELD_MIN_STARTEND;
        _depthOfField.gaussianEnd.value = DEPTHOFFIELD_MIN_STARTEND;

        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            hueShift = Mathf.Sin(Time.time) * COLORADJUSTMENTS_RANGE_HUE_MULT;
            _colorAdjustments.hueShift.value = hueShift;
            _colorAdjustments.saturation.value = Mathf.Lerp(COLORADJUSTMENTS_DEFAULT_SATURATION, COLORADJUSTMENTS_MAX_SATURATION, normDelta);

            yield return null;
        }
        _colorAdjustments.saturation.value = COLORADJUSTMENTS_MAX_SATURATION;

        while (isBlindness) {
            isBlindness = _playerEffectManager.ActiveEffects.ContainsKey(FishEffectType.Blindness)
                && _playerEffectManager.ActiveEffects[FishEffectType.Blindness].Enabled;

            hueShift = Mathf.Sin(Time.time) * COLORADJUSTMENTS_RANGE_HUE_MULT;
            _colorAdjustments.hueShift.value = hueShift;

            yield return null;
        }

        deltaSmooth = 0;
        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
            hueShift = Mathf.Lerp(hueShift, COLORADJUSTMENTS_DEFAULT_HUE, normDelta);

            _colorAdjustments.hueShift.value = hueShift;
            _colorAdjustments.saturation.value = Mathf.Lerp(COLORADJUSTMENTS_MAX_SATURATION, COLORADJUSTMENTS_DEFAULT_SATURATION, normDelta);
            _depthOfField.gaussianStart.value = Mathf.Lerp(DEPTHOFFIELD_MIN_STARTEND, DEPTHOFFIELD_DEFAULT_START, normDelta);
            _depthOfField.gaussianEnd.value = Mathf.Lerp(DEPTHOFFIELD_MIN_STARTEND, DEPTHOFFIELD_DEFAULT_END, normDelta);

            yield return null;
        }

        _depthOfField.gaussianStart.value = DEPTHOFFIELD_DEFAULT_START;
        _depthOfField.gaussianEnd.value = DEPTHOFFIELD_DEFAULT_END;
        _colorAdjustments.saturation.value = COLORADJUSTMENTS_DEFAULT_SATURATION;
        _colorAdjustments.hueShift.value = COLORADJUSTMENTS_DEFAULT_HUE;
        _blindnessCoroutine = null;
    }

    IEnumerator StunEffect() {
        bool isStun = true;
        float deltaSmooth = 0;
        float normDelta = 0f;
        float time = 0;
        float x = 0;
        float y = 0;

        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
            time = Time.time * LENS_DISORTION_OFFSET_SPEED;
            x = Mathf.Cos(time) * LENS_DISORTION_OFFSET_MULT * normDelta;
            y = Mathf.Sin(time) * LENS_DISORTION_OFFSET_MULT * normDelta;

            _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_INTENS, LENS_DISORTION_MAX_INTENS, normDelta);
            _lensDistortion.center.value = new Vector2(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);
            _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_SCALE, LENS_DISORTION_MAX_SCALE, normDelta);

            yield return null;
        }

        while (isStun) {
            isStun = _playerEffectManager.ActiveEffects.ContainsKey(FishEffectType.Stun)
                && _playerEffectManager.ActiveEffects[FishEffectType.Stun].Enabled;

            time = Time.time * LENS_DISORTION_OFFSET_SPEED;
            x = Mathf.Cos(time) * LENS_DISORTION_OFFSET_MULT;
            y = Mathf.Sin(time) * LENS_DISORTION_OFFSET_MULT;
            _lensDistortion.intensity.value = LENS_DISORTION_MAX_INTENS;
            _lensDistortion.scale.value = LENS_DISORTION_MAX_SCALE;
            _lensDistortion.center.value = new Vector2(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);
            yield return null;
        }

        //smooth fade

        deltaSmooth = 0f;
        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;
            x = Mathf.Lerp(x, 0f, normDelta);
            y = Mathf.Lerp(y, 0f, normDelta);

            if (!_playerEffectManager.ActiveEffects.TryGetValue(FishEffectType.Poison, out Effect effect)
                || !effect.Enabled) {//so check if stun not active
                _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_MAX_INTENS, LENS_DISORTION_DEFAULT_INTENS, normDelta);
                _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_MAX_SCALE, LENS_DISORTION_DEFAULT_SCALE, normDelta);
            }
            _lensDistortion.center.value.Set(LENS_DISORTION_OFFSET_CENTER + x, LENS_DISORTION_OFFSET_CENTER + y);

            yield return null;
        }

        _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
        _lensDistortion.center.value.Set(LENS_DISORTION_OFFSET_CENTER, LENS_DISORTION_OFFSET_CENTER);
        _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;

        _stunCoroutine = null;
    }

    IEnumerator PoisonEffect() {
        bool isPoison = true;
        float deltaSmooth = 0;
        float normDelta = 0;


        while (deltaSmooth < EFFECTS_FADE_DURATION) {
            deltaSmooth += DJTime.UnscaledDeltaTime;
            normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

            _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_INTENS, LENS_DISORTION_MAX_INTENS, normDelta);
            _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_DEFAULT_SCALE, LENS_DISORTION_MAX_SCALE, normDelta);

            yield return null;
        }

        while (isPoison) {
            isPoison = _playerEffectManager.ActiveEffects.ContainsKey(FishEffectType.Poison)
                && _playerEffectManager.ActiveEffects[FishEffectType.Poison].Enabled;
            _lensDistortion.intensity.value = LENS_DISORTION_MAX_INTENS;
            _lensDistortion.scale.value = LENS_DISORTION_MAX_SCALE;
            yield return null;
        }

        //smooth fade
        if (!_playerEffectManager.ActiveEffects.TryGetValue(FishEffectType.Stun, out Effect effect)
                || !effect.Enabled) {//poison and stun use same lens distortion//so check if stun not active

            deltaSmooth = 0f;
            while (deltaSmooth < EFFECTS_FADE_DURATION) {
                deltaSmooth += DJTime.UnscaledDeltaTime;
                normDelta = deltaSmooth / EFFECTS_FADE_DURATION;

                _lensDistortion.intensity.value = Mathf.Lerp(LENS_DISORTION_MAX_INTENS, LENS_DISORTION_DEFAULT_INTENS, normDelta);
                _lensDistortion.scale.value = Mathf.Lerp(LENS_DISORTION_MAX_SCALE, LENS_DISORTION_DEFAULT_SCALE, normDelta);
                yield return null;
            }
        }
        _lensDistortion.intensity.value = LENS_DISORTION_DEFAULT_INTENS;
        _lensDistortion.scale.value = LENS_DISORTION_DEFAULT_SCALE;

        _poisonCoroutine = null;
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
        if (_playerFish != null && _playerEffectManager != null) {
            _playerFish.OnProgressChanged -= UpdateProgressInfo;
            _playerFish.OnHealthChanged -= UpdateHealthInfo;
            _playerFish.OnFishDamage -= ActivateDamageOverlay;
            _playerEffectManager.OnEffectChanges -= EffectsBehaviour;
        }

        if (/*spawnManager == null || spawnManager.PlayerFish == null ||*/
            fish == null || sprintProgressPC == null || sprintButton == null)//for debug
            return;

        _playerFish = fish;
        _playerEffectManager = fish.GetComponent<EffectManager>(); //get player transform

        //syubscribe to events of the new player fish
        _playerFish.OnProgressChanged += UpdateProgressInfo;
        _playerFish.OnHealthChanged += UpdateHealthInfo;
        _playerFish.OnFishDamage += ActivateDamageOverlay;
        _playerEffectManager.OnEffectChanges += EffectsBehaviour;

        sprintButton.interactable = true;
        sprintButton.image.fillAmount = 1f; // reset the button fill amount
                                            //sprintProgressPC.transform.localScale = Vector3.one;//fillAmount = 1f;
        SetSprintProgressPC(1);
        sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, 1f);
        _currentSprintCooldownTime = 0f;

        DisactivateDefaultIcons();
        EffectsProgressReset();

        //ResetOverlays();
        //zoneOverlay.gameObject.SetActive(false);
        //zoneOverlay.color = new Color(zoneOverlay.color.r, zoneOverlay.color.g, zoneOverlay.color.b, 1f);
        ResetVisualEffects();

        //upd icons pos of spawn effects
        foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
            if (_playerEffectManager.ActiveEffects.TryGetValue(effectType, out Effect value) && value != null)
                EffectsBehaviour(effectType, true);

        UpdateHUDbars();
        growthBar.ActivateIcon(/*currentProgressIco*/fish.Level);
        //growthBar.GetProgress(fish.Level).fillAmount = fish.Progress;

        if (!isDie) {//lvl upgr & unlock new biomes
            if (fish.Level == FishLevel.Mackerel)
                _gameTips.OnStageTip();
            else if (fish.Level == FishLevel.Moray)
                _gameTips.OnClearCoralTip();
            //ActivateZoneOverlay(coralReefClear.GetLocalizedString(), true);
            else if (fish.Level == FishLevel.HammerShark)
                _gameTips.OnClearSeaweedTip();
            //ActivateZoneOverlay(seaweedClear.GetLocalizedString(), true);

        }
    }

    public void ActivateBiomeTip(ZoneType type) {
        if (type == ZoneType.AlgaesForest)
            _gameTips.OnAlgaesBarrierTip();
        else if (type == ZoneType.DeepCorals)
            _gameTips.OnDeepCoralsBarrierTip();
    }

    void UpdateHUDbars() {
        UpdateHealthInfo();
        UpdateProgressInfo();
        UpdateScoreInfo(PlayerLeaderboardScore.Score);
        //UpdateProgressIcon();
    }

    //private void ResetOverlays() {
    //    poisonOverlay.gameObject.SetActive(false);
    //    poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 1f);
    //    stunOverlay.gameObject.SetActive(false);
    //    stunOverlay.color = new Color(stunOverlay.color.r, stunOverlay.color.g, stunOverlay.color.b, 1f);
    //    damageOverlay.gameObject.SetActive(false);
    //    damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 1f);
    //    stopTimeOverlay.gameObject.SetActive(false);
    //    stopTimeOverlay.color = new Color(stopTimeOverlay.color.r, stopTimeOverlay.color.g, stopTimeOverlay.color.b, 1f);
    //    blindnessOverlay.gameObject.SetActive(false);
    //    blindnessOverlay.color = new Color(blindnessOverlay.color.r, blindnessOverlay.color.g, blindnessOverlay.color.b, 1f);
    //    zoneOverlay.gameObject.SetActive(false);
    //    zoneOverlay.color = new Color(zoneOverlay.color.r, zoneOverlay.color.g, zoneOverlay.color.b, 1f);

    //    StopAllCoroutines();
    //}

    //public void ActivateZoneOverlay(string msg, bool isLongDuration = false) {
    //    zoneOverlayMsg.color = new Color(zoneOverlayMsg.color.r, zoneOverlayMsg.color.g, zoneOverlayMsg.color.b, 1f);
    //    if (zoneOverlay.gameObject.activeInHierarchy)
    //        return;//corutine is already running (wait for next call)

    //    //Debug.LogWarning("ActivateZone");
    //    zoneOverlay.gameObject.SetActive(true);
    //    zoneOverlayMsg.text = msg;
    //    if (zoneCoroutine != null)
    //        StopCoroutine(zoneCoroutine);
    //    zoneCoroutine = StartCoroutine(OverlayFader(zoneOverlay, FishEffectType.None, OVERLAY_ZONE_UPDATE_INTERVAL, zoneOverlayMsg, isLongDuration));
    //}

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
        int effectsCnt = _playerEffectManager.GetActiveEffectCnt(TypeEffects.VisibleUI)/*activeEffects.Count*/;

        if (!isAdded)//if effect expires - hide icon
            _effectsSlotsMatch[type].gameObject.SetActive(false);

        if (effectsCnt > 0) {//reposing
            int effectIconInd = 0;
            foreach (FishEffectType currType in _playerEffectManager.ActiveEffects.Keys) {
                if (!EffectManager.IsVisibleEffect(currType, true)
                    || _playerEffectManager.ActiveEffects[currType] == null || !_playerEffectManager.ActiveEffects[currType].Enabled)
                    continue;

                //Debug.Log(effectsCnt - 1 + " " + effectIconInd);
                Vector2 newPos = new Vector2(
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x * iconsParent.rect.width,
                    iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y * iconsParent.rect.height);
                //Debug.LogWarning(iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].x + " "
                //+ iconsPositionsMultipliers[effectsCnt - 1][effectIconInd].y);

                _effectsSlotsMatch[currType].anchoredPosition = newPos;

                effectIconInd = (effectIconInd + 1 < MAX_EFFECTS_CNT) ? ++effectIconInd : /*--*/effectIconInd;
            }
        }

        ////add overlays
        //Image currentOverlay = type switch {
        //    FishEffectType.Stun => stunOverlay,
        //    FishEffectType.Poison => poisonOverlay,
        //    FishEffectType.FreezeTime => stopTimeOverlay,
        //    FishEffectType.Blindness => blindnessOverlay,
        //    _ => null
        //};

        //if (currentOverlay != null) {
        if (isAdded) {
            if (type == FishEffectType.Stun && _stunCoroutine == null)
                _stunCoroutine = StartCoroutine(StunEffect());
            else if (type == FishEffectType.Poison && _poisonCoroutine == null)
                _poisonCoroutine = StartCoroutine(PoisonEffect());
            else if (type == FishEffectType.FreezeTime && _freezeCoroutine == null)
                _freezeCoroutine = StartCoroutine(FreezeTimeEffect());
            else if (type == FishEffectType.Blindness && _blindnessCoroutine == null)
                _blindnessCoroutine = StartCoroutine(BlindnessEffect());
        }
        else
            return;
    }

    public void OnDestroy() {
        PlayerReConnect(_playerFish, false);
        PlayerLeaderboardScore.OnChanged -= UpdateScoreInfo;
        SettingsUI.OnHudHideChanged -= UpdateHUDbars;
        StopAllCoroutines();
    }
}
