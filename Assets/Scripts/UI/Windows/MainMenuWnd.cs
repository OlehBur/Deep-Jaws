using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public interface IMainMenuNavigation {
    //void ActivateMainMenuContainer(bool isActive);
    void OnSanctuaryClick();
    void OnNewGameClick();
    void OnSkipGuide();
    void OnCompleteGuide();
    void OnExitButtonClicked();
}

public class MainMenuWnd : WindowBase, IMainMenuNavigation {
    [Inject] private readonly IGameModeController _gameModeController;
    [Inject] private readonly ISceneLoader _sceneLoader;
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;
    [Inject] private readonly ILicenseValidator _licenseValidator;
    [Inject] private readonly IAudioPlayService _audioPlayService;

    [SerializeField] GPGServicesWnd gpgServicesWnd;
    [SerializeField] StoreWnd storeWnd;
    //[SerializeField] GameModeWnd gameModeWnd;
    [SerializeField] SkinsPreviewWnd skinsPreviewWnd;
    [SerializeField] Button continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] Button newGameButton;
    [SerializeField] Button sanctuaryButton;
    [SerializeField] Button guideBookButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button bugReport;

    private bool _isWorldDataExist = false;
    private bool _immediateÑlosureFlag = false;

    private const string REPORT_EMAIL = "bovv.company@gmail.com";
    private const string REPORT_SUBJECT = "Bug Report: Deep Jaws";


    private void Awake() {
        if (gpgServicesWnd != null) {
            gpgServicesWnd.SetActivation(!GameManager.IS_PC_DEMO);
            sanctuaryButton.interactable = !GameManager.IS_PC_DEMO;
        }

        _isWorldDataExist = ProgressSaver.IsWorldDataExist;
        if (continueButton != null) {// Initialize the continue button visibility based on saved data
            continueButton.gameObject.SetActive(_isWorldDataExist);
            GameModeWnd.GamemodeInitSet(_isWorldDataExist ? ProgressSaver.LoadWorldData().gameMode : GameMode.None);
        }
    }

    private void Start() {
        if (GameManager.IsInternetNotAvailable)
            _alertWindowsManager.FirstNoConnectionOpen();

        _licenseValidator.OnLicenseCheckCompleted += SetActivation;
        _gameModeController.Window.OnChangeStage += AutoHideMainMenu;
        storeWnd.OnChangeStage += AutoHideMainMenu;
        skinsPreviewWnd.OnChangeStage += AutoHideMainMenu;
        _alertWindowsManager.OnAlerWndOpened += AutoHideMainMenu;//auto close main menu wnd if player click "yes" on are u sure wnd

        InitMMButtonsListeners();
    }

    private void OnDestroy() {
        if (_licenseValidator != null)
            _licenseValidator.OnLicenseCheckCompleted -= SetActivation;
        if (_gameModeController != null)
            _gameModeController.Window.OnChangeStage -= AutoHideMainMenu;

        storeWnd.OnChangeStage -= AutoHideMainMenu;
        skinsPreviewWnd.OnChangeStage -= AutoHideMainMenu;

        if (_alertWindowsManager != null) {
            _alertWindowsManager.OnAlerWndOpened -= AutoHideMainMenu;
        }
    }

    void AutoHideMainMenu(bool isAlertWndOpened) {
        bool isAnyWndOpened = _immediateÑlosureFlag || isAlertWndOpened || storeWnd.isActiveAndEnabled
            || skinsPreviewWnd.isActiveAndEnabled || _gameModeController.Window.isActiveAndEnabled;

        SetActivation(!isAnyWndOpened);
    }

    void InitMMButtonsListeners() {
        continueButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();
        sanctuaryButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        bugReport.onClick.RemoveAllListeners();
        guideBookButton.onClick.RemoveAllListeners();

        continueButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        newGameButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        sanctuaryButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        settingsButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        exitButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        bugReport.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        guideBookButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        continueButton.onClick.AddListener(_sceneLoader.OnLoad_GameScene);
        newGameButton.onClick.AddListener(OnNewGameClick);
        sanctuaryButton.onClick.AddListener(_sceneLoader.OnLoad_SanctuaryScene);
        guideBookButton.onClick.AddListener(() => _alertWindowsManager.ActivateGuidebookWnd(true));
        settingsButton.onClick.AddListener(() => _alertWindowsManager.ActivateSettingsWnd(true));
        exitButton.onClick.AddListener(OnExitButtonClicked);
        bugReport.onClick.AddListener(OpenEmailClientForBugReport);
    }

    public void OnSanctuaryClick() =>
        _sceneLoader.OnLoad_SanctuaryScene();

    //open ar u sur wnd if player already has saved game data
    public void OnNewGameClick() {
        if (ProgressSaver.IsWorldDataExist)
            _alertWindowsManager.ActivateAreUSureNewGameWnd(true, NewGameConfirmHandler);
        else
            NewGameConfirmHandler();
        //guideBookWnd?.SetActive(true);
    }

    void NewGameConfirmHandler() {
        PlayerPrefs.DeleteAll();
        ProgressSaver.DeleteWorldData();
        _gameModeController.Window.OnOpen();
        _immediateÑlosureFlag = true;
        AutoHideMainMenu(true);
    }

    public void OnCompleteGuide() {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (isWorldDataExist)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_you_have_my_respect);
#endif
    }

    public void OnSkipGuide() {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isWorldDataExist)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_are_you_serious);
#endif
    }

    public void OpenEmailClientForBugReport() {
        string bodyTemplate =
            "Please describe the bug below:\n\n\n" +
            "Steps to reproduce (how can we trigger this bug):\n\n\n" +
            "Additional links to videos/photos (if any):\n\n\n" +
            "--- \n" +
            "Device info (optional): " + SystemInfo.deviceModel + "\n" +
            "OS: " + SystemInfo.operatingSystem;

        //form corr link
        // with text escaping, spaces or etc. convert to %0A like
        string emailUrl = string.Format(
            "mailto:{0}?subject={1}&body={2}",
            REPORT_EMAIL,
            Uri.EscapeDataString(REPORT_SUBJECT),
            Uri.EscapeDataString(bodyTemplate)
        );

        Application.OpenURL(emailUrl);

        Debug.Log("Open Mail Client...");
    }

    public void OnExitButtonClicked() => Application.Quit();
}
