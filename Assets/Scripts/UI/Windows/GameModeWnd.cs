using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;



public class GameModeWnd : WindowBase, IGameModeController {
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Button easyModeButton;
    [SerializeField] Button mediumModeButton;
    [SerializeField] Button hardModeButton;
    [SerializeField] Button confirmButton;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] Image gameModeWndBackground;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly IGraphicManager _graphicManager;
    [Inject] private readonly ISceneLoader _sceneLoader;
    [Inject] private readonly UILocalizationInfo _localizationInfo;

    public static GameMode CurrentMode { get; private set; } = GameMode.None;
    public WindowBase Window => this;

    private bool _isGMneedAnimation = true;
    private Color _targetGMwndColor;
    private Color _targetLightGMcolor;


    void Awake() {
        InitButtonListeners();
    }

    void InitButtonListeners() {
        easyModeButton.onClick.RemoveAllListeners();
        mediumModeButton.onClick.RemoveAllListeners();
        hardModeButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();

        easyModeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        mediumModeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        hardModeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        confirmButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        easyModeButton.onClick.AddListener(() => GamemodeClickHandler(GameMode.Easy, easyModeButton, _localizationInfo.Gamemode_EasyDesc));
        mediumModeButton.onClick.AddListener(() => GamemodeClickHandler(GameMode.Medium, mediumModeButton, _localizationInfo.Gamemode_MediumDesc));
        hardModeButton.onClick.AddListener(() => GamemodeClickHandler(GameMode.Hard, hardModeButton, _localizationInfo.Gamemode_HardDesc));
        confirmButton.onClick.AddListener(ConfirmModeAndLoadScene);
    }

    void ConfirmModeAndLoadScene() {
        _graphicManager.ResetLightColor();
        OnClose();
        _sceneLoader.OnLoad_GameScene();
    }

    void GamemodeClickHandler(GameMode gamemode, Button clickedButton, LocalizedString localizedDesc) {
        easyModeButton.interactable = true;
        mediumModeButton.interactable = true;
        hardModeButton.interactable = true;
        clickedButton.interactable = false;

        description.text = localizedDesc.GetLocalizedString();

        OnGameModeSet((int)gamemode);
    }

    public void ActivateGamemodesWnd(bool isActivate) {
        //Debug.Log($"Activate {GetInstanceID()} " +
        //    $"this==null:{this == null} " +
        //    $"gameObject==null:{gameObject == null}");
        ////Debug.Log($"ActivateGamemodesWnd is Destroyed:{_isDestroyed}");

        SetActivation(isActivate);
    }

    IEnumerator ChangeColorGMwnd() {
        //Debug.Log("gm changing color started");
        while (gameModeWndBackground.color != _targetGMwndColor) {
            gameModeWndBackground.color = Color.Lerp(gameModeWndBackground.color, _targetGMwndColor, DJTime.DeltaTime);
            yield return null;
        }

        //Debug.Log("gm Color changed");
    }

    IEnumerator ChangeColorGMmainMenu() {
        while (_graphicManager.MainLightColor != _targetLightGMcolor) {
            _graphicManager.MainLightColor = Color.Lerp(_graphicManager.MainLightColor, _targetLightGMcolor, DJTime.DeltaTime);
            yield return null;
        }
    }

    public static void GamemodeInitSet(GameMode gamemode) => CurrentMode = gamemode;

    public void OnGameModeSet(int gamemode) {
        CurrentMode = (GameMode)gamemode;

        _targetGMwndColor = CurrentMode switch {
            GameMode.Easy => GameStaticInfo.EASY_GM_COLOR,
            GameMode.Medium => GameStaticInfo.MEDIUM_GM_COLOR,
            GameMode.Hard => GameStaticInfo.HARD_GM_COLOR,
            _ => Color.white
        };

        _targetLightGMcolor = CurrentMode switch {
            GameMode.Easy => GameStaticInfo.LIGHT_DEFAULT_COLOR,
            GameMode.Medium => GameStaticInfo.LIGHT_MEDIUM_COLOR,
            GameMode.Hard => GameStaticInfo.LIGHT_HARD_COLOR,
            _ => Color.white
        };

        StopCoroutine(ChangeColorGMwnd());
        StartCoroutine(ChangeColorGMwnd());
        StopCoroutine(ChangeColorGMmainMenu());
        StartCoroutine(ChangeColorGMmainMenu());

        if (_isGMneedAnimation) {
            gmWndAnimator.SetTrigger("Anim");
            _isGMneedAnimation = false;
        }
    }
}
