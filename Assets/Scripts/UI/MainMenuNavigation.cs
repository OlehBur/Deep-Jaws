using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public interface IMainMenuNavigation {
    void ActivateMainMenuContainer(bool isActive);
    void OnOpenGameInBrowser();
    void OnSanctuaryClick();
    void OnNewGameClick();
    void OnSkipGuide();
    void OnCompleteGuide();
    void OnExitButtonClicked();
}

public class MainMenuNavigation : MonoBehaviour, IMainMenuNavigation {
    [Inject] private readonly IPlayerFishPoolService _playerSpawnService;
    [Inject] private readonly IGameModeController _gameModeController;
    [Inject] private readonly ISceneLoader _sceneLoader;
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;

    [SerializeField] GameObject GPG_Services_UI;
    [SerializeField] GameObject continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] Button merineSanctuary;                                //[SerializeField] Button merineSanctuary;
    [SerializeField] GameObject mainMenuContainer;

    private bool _isWorldDataExist = false;

    private const string REPORT_EMAIL = "bovv.company@gmail.com";
    private const string REPORT_SUBJECT = "Bug Report: Deep Jaws";
    private const string GAME_FULL_URL = "https://play.google.com/store/apps/details?id=com.bovvcompany.deepjaws&referrer=utm_source%3Ditchio%26utm_medium%3Dpc_demo";


    private void Awake() {
        if (GPG_Services_UI != null) {
            GPG_Services_UI.SetActive(!GameManager.IS_PC_DEMO);
            merineSanctuary.interactable = !GameManager.IS_PC_DEMO;
        }
    }

    private void Start() {
        _isWorldDataExist = ProgressSaver.IsWorldDataExist;
        if (continueButton != null) {// Initialize the continue button visibility based on saved data
            continueButton?.SetActive(_isWorldDataExist);
            GameModeController.CurrentMode = _isWorldDataExist ?
                ProgressSaver.LoadWorldData().gameMode : GameMode.None;
        }

        if (GameManager.IsInternetNotAvailable)
            _alertWindowsManager.FirstNoConnectionOpen();

    }

    public void ActivateMainMenuContainer(bool isActive) =>
        mainMenuContainer.SetActive(isActive);

    public void OnOpenGameInBrowser() =>
        Application.OpenURL(GAME_FULL_URL);
    public void OnSanctuaryClick() {
        //if (GameManager.IsInternetNotAvailable)
        //    ActivateNoInternetConnectionWnd(true);
        //else
        _sceneLoader.OnLoad_SanctuaryScene();
    }

    //open ar u sur wnd if player already has saved game data
    public void OnNewGameClick() {
        if (ProgressSaver.IsWorldDataExist)
            _alertWindowsManager.ActivateAreUSureWnd(true);
        else
            _gameModeController.ActivateGamemodesWnd(true);
        //guideBookWnd?.SetActive(true);
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
