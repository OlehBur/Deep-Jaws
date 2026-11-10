using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [SerializeField] Button sprintButton; // button for sprint effect activation

    [Header("Player Info")]
    [SerializeField] ScoreBarView scoreBar;
    [SerializeField] GrowthBarView growthBar;
    [SerializeField] HealthBarView healthBar;
    [SerializeField] GameObject playebleAreaUI;//to take away the player's ability to move 
    [SerializeField] Image sprintProgressImage;
    [SerializeField] RectTransform sprintProgressPC;
    [SerializeField] RectTransform sprintPC_Parent;

    private EffectManager _playerEffectManager;
    private float _currentSprintCooldownTime = 0;
    private Fish _playerFish; // reference to the player fish component

    [Inject] private readonly IGameTip _gameTips;
    [Inject] private readonly IGameHudController _gameHudController;
    [Inject] private readonly IAudioPlayService _audioManager;
    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    [Inject] private readonly IEffectsVisualizationService _effectsVisualizationService;
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;

    public const float VIEW_HUD_TIME = 1f;
    public const float VIEW_HEALTH_TIME = 2f;
    public const float VIEW_JAW_TIME = .8f;
    public const float FADE_HUD_TIME = 1.5f;

    private const float SPRINT_BUTTON_COOLDOWN = 20f;
    private const float SPRINT_EFFECT_DURATION = 5f;
    private const float SPRINT_PROGRESS_PC_LOADING_ALPHA = 0.3f;



    void Start() {
        _playerFishPoolService.OnPlayerRespawn += PlayerReConnect;
        PlayerLeaderboardScore.OnChanged += scoreBar.UpdateScoreInfo;
        SettingsWnd.OnHudHideChanged += UpdateHUDbars;

        if (GameManager.IsRunOnPC) {
            playebleAreaUI.SetActive(false);
            sprintProgressPC.transform.parent.gameObject.SetActive(true);
        }
        else {
            sprintProgressPC.transform.parent.gameObject.SetActive(false);
            playebleAreaUI.SetActive(true);
        }
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

    void OnDestroy() {
        if (_playerEffectManager != null && _effectsVisualizationService != null)
            _playerEffectManager.OnEffectChanges -= PlayerVisualEffectChangeHandler;

        //PlayerReConnect(_playerFish, false);
        PlayerLeaderboardScore.OnChanged -= scoreBar.UpdateScoreInfo;
        SettingsWnd.OnHudHideChanged -= UpdateHUDbars;
        StopAllCoroutines();
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
        if (_alertWindowsManager.IsOpenedAnyAlertWnd && GameManager.IsGamePaused)
            return;

        if (pauseMenu.activeInHierarchy == GameManager.IsGamePaused)
            _gameHudController.ActivatePauseWnd(!GameManager.IsGamePaused);
    }

    public void OnSpintButtonClick() {
        _playerEffectManager.ApplyEffect(FishEffectType.SprintSpeed, SPRINT_EFFECT_DURATION);
        sprintButton.interactable = false; // disable the button after applying the effect
        _currentSprintCooldownTime = 0f; // reset the cooldown timer
    }

    // Update is called once per frame
    void Update() {
        //if (_playerEffectManager != null && _playerEffectManager.ActiveEffects.Count > 0)
        //    foreach (KeyValuePair<FishEffectType, Effect> eff in _playerEffectManager.ActiveEffects)
        //        if (eff.Value == null || !eff.Value.Enabled)
        //            _effectsVisualizationService.EffectsVisualizationsUpdate(eff.Key, _playerEffectManager, false);

        SprintButtonBehaviour();

        if (GameManager.IsRunOnPC)
            WASD_MovingBehaviour();
    }

    void WASD_MovingBehaviour() {
        if (Keyboard.current == null || GameManager.IsGamePaused)
            return;

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        JoyAxis.Axis = moveInput.normalized; // set the joystick axis to the normalized input vector
    }

    void UpdateProgressBar(ProgressArgs args = null) {
        int lvlProgress = (int)(_playerFish.Progress * GameStaticInfo.PERCENT_MULTIPLIER);

        growthBar.UpdateProgressInfo(lvlProgress, _playerFish);

        if (args != null && args.IsCaviarEaten)
            _gameTips.OnEffectTip();
    }

    void UpdateHealthInfo() {
        float health = _playerFish.Health;
        if (health < 0)
            health = 0f;
        if (healthBar.Coroutine != null)
            StopCoroutine(healthBar.Coroutine);

        healthBar.CurrProgress.text = health.ToString();
        healthBar.MaxProgress.text = _playerFish.DefaultInfo.MaxHealth.ToString();
        healthBar.Filler.fillAmount = (float)_playerFish.Health / _playerFish.DefaultInfo.MaxHealth;

        if (gameObject.activeInHierarchy)
            healthBar.Coroutine = StartCoroutine(healthBar.Animation(healthBar.Filler.fillAmount));
        else
            healthBar.ChangeTransparancy(SettingsWnd.IsAutoHideUI ? 0f : 1f);
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



    void ActivateDamageOverlay() {
        _audioManager.PlayVFX(VFX_clip.Damage);
        _effectsVisualizationService.ActivateDamageVisualization();
    }


    void PlayerReConnect(Fish fish, bool isDie) {
        //Debug.LogWarning("PlayerRespawn");
        PlayerReconnectEventsHandler(fish);


        if (_playerFishPoolService.PlayerCurrentFish == null)
            Debug.LogWarning("_playerFishPoolService.PlayerCurrentFish is unexpectedly equal null");

        sprintButton.interactable = true;
        sprintButton.image.fillAmount = 1f; // reset the button fill amount
        SetSprintProgressPC(1);
        sprintProgressImage.color = new Color(sprintProgressImage.color.r, sprintProgressImage.color.g, sprintProgressImage.color.b, 1f);
        _currentSprintCooldownTime = 0f;

        _effectsVisualizationService.ResetAllEffectsVisualization();
        //upd icons pos of spawn effects
        foreach (FishEffectType effectType in Enum.GetValues(typeof(FishEffectType)))
            if (_playerEffectManager.ActiveEffects.TryGetValue(effectType, out Effect value) && value != null)
                _effectsVisualizationService.EffectsVisualizationsUpdate(effectType, _playerEffectManager, true);

        UpdateHUDbars();
        growthBar.ActivateIcon(/*currentProgressIco*/fish.Level);
        PlayerReconectTipsHandler(fish, isDie);
    }

    void PlayerReconnectEventsHandler(Fish fish) {
        if (_playerFish != null && _playerEffectManager != null) {
            _playerFish.OnProgressChanged -= UpdateProgressBar;
            _playerFish.OnHealthChanged -= UpdateHealthInfo;
            _playerFish.OnFishDamage -= ActivateDamageOverlay;
            _playerEffectManager.OnEffectChanges -= PlayerVisualEffectChangeHandler;
        }

        if (fish == null || sprintProgressPC == null || sprintButton == null)//for debug
            return;

        _playerFish = fish;
        _playerEffectManager = fish.GetComponent<EffectManager>(); //get player transform

        //syubscribe to events of the new player fish
        _playerFish.OnProgressChanged += UpdateProgressBar;
        _playerFish.OnHealthChanged += UpdateHealthInfo;
        _playerFish.OnFishDamage += ActivateDamageOverlay;
        _playerEffectManager.OnEffectChanges += PlayerVisualEffectChangeHandler;
    }

    void PlayerVisualEffectChangeHandler(FishEffectType type, bool isAdded) {
        if (_playerEffectManager != null)
            _effectsVisualizationService.EffectsVisualizationsUpdate(type, _playerEffectManager, isAdded);
        else
            Debug.LogWarning("Player Effet Manager is Null.", this);
    }

    void PlayerReconectTipsHandler(Fish fish, bool isDie) {
        if (!isDie) {//lvl upgr & unlock new biomes
            if (fish.Level == FishLevel.Mackerel)
                _gameTips.OnStageTip();
            else if (fish.Level == FishLevel.Moray)
                _gameTips.OnClearCoralTip();
            //ActivateZoneOverlay(coralReefClear.GetLocalizedString(), true);
            else if (fish.Level == FishLevel.HammerShark)
                _gameTips.OnClearSeaweedTip();
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
        UpdateProgressBar();
        scoreBar.UpdateScoreInfo(PlayerLeaderboardScore.Score);
    }

}
