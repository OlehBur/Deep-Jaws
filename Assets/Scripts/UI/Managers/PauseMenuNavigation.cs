using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class PauseMenuNavigation : MonoBehaviour {
    [Inject] private readonly IGameManager _gameManager;
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;
    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly IGameHudController _gameHudController;
    [Inject] private readonly IPlayerFishPoolService _playerProvider;

    [SerializeField] Button resumeButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button guideBookButton;
    [SerializeField] Button exitButton;
    [SerializeField] GameObject[] gamemodePauseTitles;


    //[Inject]
    //public void Construct(
    //    IAlertWindowsManager alertWindowsManager,
    //    IAudioPlayService audioPlayService,
    //    IGameHudController gameHudController) {
    //    _alertWindowsManager = alertWindowsManager;
    //    _audioPlayService = audioPlayService;
    //    _gameHudController = gameHudController;

    //    //Debug.Log("Pause Menu Nav INject: " + _audioPlayService);

    //    InitButtonListeners();
    //}

    private void Awake() {
        if (gamemodePauseTitles != null)
            GameModeTitlesInit();
    }

    private void Start() {
        InitButtonListeners();
        _gameManager.OnGamePause += OnPauseChanged;
    }

    void OnPauseChanged(bool isPause) {
        if (_playerProvider.PlayerCurrentFish != null) {
            _playerProvider.PlayerCurrentFish.gameObject.TryGetComponent(out EffectManager playerEffManager);
            if (isPause && exitButton != null)
                //can exist only if the player is not being pursued by enemies and has no active effects
                exitButton.interactable = playerEffManager.IsNoNegativeEffects() && !GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED;
        }
    }

    void GameModeTitlesInit() {//init gamemode buttons        
        if (GameModeController.CurrentMode == GameMode.None) {
            Debug.LogWarning("Gamemode = None -> Activate gamemod auto-select (medium)");
            GameModeController.CurrentMode = GameMode.Medium;
        }

        if (gamemodePauseTitles.Length == GameStaticInfo.GAMEMODES_COUNT) {
            foreach (GameObject tit in gamemodePauseTitles)
                tit.SetActive(false);

            gamemodePauseTitles[(int)GameModeController.CurrentMode].SetActive(true);
        }
        //Debug.LogWarning(GameStaticInfo.Gamemode);
    }

    void InitButtonListeners() {
        resumeButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        guideBookButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();

        resumeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        settingsButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        guideBookButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        exitButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        resumeButton.onClick.AddListener(() => _gameHudController.ActivatePauseWnd(false));
        settingsButton.onClick.AddListener(() => _alertWindowsManager.ActivateSettingsWnd(true));
        guideBookButton.onClick.AddListener(() => _alertWindowsManager.ActivateGuidebookWnd(true));
        exitButton.onClick.AddListener(() => _alertWindowsManager.ActivateAreUSureExitGameWnd(true));
    }
}
