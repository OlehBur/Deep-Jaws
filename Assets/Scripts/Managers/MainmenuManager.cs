using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Localization;
using System.Runtime.CompilerServices; //  for Uri.EscapeDataString


/// <summary>
/// відповідає за активніть графічного інтерфейсу пов'язану з грою
/// responsible for the activity of the graphical interface related to the game
/// </summary>
public class MainmenuManager : MonoBehaviour {
    [SerializeField] AudioManager audioManager;
    [SerializeField] GraphicManager graphicManager;
    [SerializeField] GameObject loadingPanel;

    [Header("MainMenu Scene")]
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] Light mainMenuLight;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] GameObject continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] SceneLoader faderScene;
    [SerializeField] GameResultWnd gameResult;
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject guideBookWnd;
    [SerializeField] GameObject newUpdWnd;

    [SerializeField] Image gameModeWndBackground;
    [SerializeField] Image signInIcon;
    [SerializeField] GameObject kingOfDeepsObj;
    [SerializeField] TextMeshProUGUI kingOfDeepsTxt;
    [SerializeField] LocalizedString kingOfDeepsLocalized;
    [SerializeField] Animator kingOfDeepsAnimator;
    //[SerializeField] LocalizedString settingsMaxFpsLocalized;

    [Header("Game Scene")]
    //only for game scene
    [SerializeField] GameObject[] gamemodePauseTitles;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject pauseButton;
    [SerializeField] GameObject pauseWnd;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] RespawnWnd respawnWnd;

    private bool isGMneedAnimation = true;
    private bool isWorldDataExist = false;
    private Color targetGMwndColor;
    private Color targetLightGMcolor;
    //private GPG_Manager gpg_manager;


    private readonly Color EASY_GM_COLOR = new Color(0.7058824f, 1f, 0.5411765f, 1f);
    private readonly Color MEDIUM_GM_COLOR = new Color(0.8207547f, 0.5232103f, 0f, 1f);
    private readonly Color HARD_GM_COLOR = new Color(0.6698113f, 0.09033603f, 0f, 1f);
    private readonly Color LIGHT_DEFAULT_COLOR = new Color(1f, 0.8671192f, 0.504717f, 1f);
    private readonly Color LIGHT_MEDIUM_COLOR = new Color(0.4544322f, 0.4999117f, 0.8679245f, 1f);
    private readonly Color LIGHT_HARD_COLOR = new Color(0f, 0f, 0f, 1f);
    private readonly Color failedSignInColor = new Color(1f, 0.4764151f, 0.4863637f, 1f);
    private readonly Color successSignInColor = new Color(0.678066f, 1f, 0.5707547f, 1f);

    //private const string LOCALE_PREF_KEY = "LocaleIndex";
    //private const string FPS_KEY = "FPS";
    private const string REPORT_EMAIL = "bovv.company@gmail.com";
    private const string REPORT_SUBJECT = "Bug Report: Deep Jaws";


    private void Awake() {
        if (gamemodePauseTitles != null)
            GameModeTitlesInit();

        AccountData accountData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        if (accountData.firstUpdOpen1_7 && newUpdWnd != null) {
            Debug.Log("Confirmed first open on New Update 1.7");
            newUpdWnd.SetActive(true);
            accountData.firstUpdOpen1_7 = false;
            ProgressSaver.SaveData(accountData);
        }
    }

    private void Start() {
        isWorldDataExist = ProgressSaver.IsWorldDataExist;

        if (faderScene != null)        // Ensure the fader scene is not destroyed on load
            faderScene?.gameObject.SetActive(true);

        if (continueButton != null) {// Initialize the continue button visibility based on saved data
            continueButton?.SetActive(isWorldDataExist);
            GameStaticInfo.Gamemode = isWorldDataExist ?
                ProgressSaver.LoadWorldData().gameMode : GameMode.None;
        }

        if (GameManager.IsInternetNotAvailable)
            FirstNoConnectionOpen();

        if (kingOfDeepsObj != null) {
            kingOfDeepsObj.SetActive(false);
            kingOfDeepsAnimator.enabled = false;
        }

        if (pauseButton != null)
            pauseButton.SetActive(!GameManager.IsRunOnPC);
    }

    private void OnEnable() {
        if (spawnManager != null)
            spawnManager.OnPlayerDied += ActivateRespawnWnd;


        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
            GPG_Manager.Instance.OnLoadKingOfDeeps += KingOfDeepsUpdate;
    }

    private void OnDisable() {
        if (spawnManager != null)
            spawnManager.OnPlayerDied -= ActivateRespawnWnd;

        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
            GPG_Manager.Instance.OnLoadKingOfDeeps -= KingOfDeepsUpdate;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void GameModeTitlesInit() {//init gamemode buttons        
        if (gamemodePauseTitles.Length == GameStaticInfo.GAMEMODES_COUNT) {
            foreach (GameObject tit in gamemodePauseTitles)
                tit.SetActive(false);

            gamemodePauseTitles[(int)GameStaticInfo.Gamemode].SetActive(true);
        }
        //Debug.LogWarning(GameStaticInfo.Gamemode);
    }

    void KingOfDeepsUpdate(string playerNick) {
        if (kingOfDeepsObj == null)
            return;

        kingOfDeepsObj.SetActive(true);
        kingOfDeepsTxt.text = kingOfDeepsLocalized.GetLocalizedString() + " " + playerNick;
        kingOfDeepsAnimator.enabled = true;
    }

    public void ActivateNoInternetConnectionWnd(bool isActive) {
        if (noInternetConnectionWnd != null)
            noInternetConnectionWnd.SetActive(isActive);
    }

    public void ActivateLoadingPanelWnd(bool isActive) =>
        loadingPanel.SetActive(isActive);

    void ActivateRespawnWnd() {
        if (GameStaticInfo.Gamemode == GameMode.Hard) {//witout ui
            spawnManager.RespawnPlayer(true, false);
            return;
        }
        //GameAdsManager.Instance.UpdateDailyRevivesTxt();
        respawnWnd.gameObject.SetActive(true);
    }

    public void ActivatePauseWnd(bool isActivate) {
        if (gameManager != null)
            gameManager.GameSetPause(isActivate);
        pauseWnd.SetActive(isActivate);
    }

    public void ResetAndReloadGame_Hard() {
        ResetWorldData();
        GameStaticInfo.Gamemode = GameMode.Hard;
        gameResult.AddShellsWithoutWnd();
        faderScene.OnLoad_GameScene();
    }

    void FirstNoConnectionOpen() {
        AccountData accountData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        if (accountData.isConfirmedNoInternetDisadvantages)
            return;

        ActivateNoInternetConnectionWnd(true);
        accountData.isConfirmedNoInternetDisadvantages = true;
        ProgressSaver.SaveData(accountData);
    }

    public void OnSanctuaryClick() {
        if (GameManager.IsInternetNotAvailable)
            ActivateNoInternetConnectionWnd(true);
        else
            sceneLoader.OnLoad_SanctuaryScene();
    }

    IEnumerator ChangeColorGMwnd() {
        //Debug.Log("gm changing color started");
        while (gameModeWndBackground.color != targetGMwndColor) {
            gameModeWndBackground.color = Color.Lerp(gameModeWndBackground.color, targetGMwndColor, DJTime.DeltaTime);
            yield return null;
        }

        //Debug.Log("gm Color changed");
    }

    IEnumerator ChangeColorGMmainMenu() {
        while (mainMenuLight.color != targetLightGMcolor) {
            mainMenuLight.color = Color.Lerp(mainMenuLight.color, targetLightGMcolor, DJTime.DeltaTime);
            yield return null;
        }
    }

    public void OnGameModeSet(int gamemode) {
        GameStaticInfo.Gamemode = (GameMode)gamemode;

        targetGMwndColor = GameStaticInfo.Gamemode switch {
            GameMode.Easy => EASY_GM_COLOR,
            GameMode.Medium => MEDIUM_GM_COLOR,
            GameMode.Hard => HARD_GM_COLOR,
            _ => Color.white
        };

        targetLightGMcolor = GameStaticInfo.Gamemode switch {
            GameMode.Easy => LIGHT_DEFAULT_COLOR,
            GameMode.Medium => LIGHT_MEDIUM_COLOR,
            GameMode.Hard => LIGHT_HARD_COLOR,
            _ => Color.white
        };

        StopCoroutine(ChangeColorGMwnd());
        StartCoroutine(ChangeColorGMwnd());
        StopCoroutine(ChangeColorGMmainMenu());
        StartCoroutine(ChangeColorGMmainMenu());

        //PlayerPrefs.SetInt("GameMode", gamemode);
        //Debug.Log("DELETED WORDL AUTO SAVE");

        if (isGMneedAnimation) {
            gmWndAnimator.SetTrigger("Anim");
            isGMneedAnimation = false;
        }
    }

    public void OnServicesButton(int buttonIndex) {
        if (GPG_Manager.Instance != null)
            switch (buttonIndex) {
                case 0:
                /*gpg_manager*/
                GPG_Manager.Instance.OnShowLeaderboard();
                break;
                case 1:
                GPG_Manager.Instance.OnShowAchievements();
                break;
                case 2:
                GPG_Manager.Instance.OnSign();
                break;
            }
    }

    //open ar u sur wnd if player already has saved game data
    public void OnNewGameClick() {
        if (isWorldDataExist)
            areUSureWnd?.SetActive(true);
        else
            guideBookWnd?.SetActive(true);
    }

    public void ChangeSignInColor(bool isSignIn) {
        if (signInIcon != null)
            signInIcon.color = (isSignIn) ?
                successSignInColor : failedSignInColor;
    }

    public void ResetWorldData() {
        PlayerLeaderboardScore.LoadSavedData();
        PlayerPrefs.DeleteAll();//delete all player prefs data 
        audioManager.SaveAudioData(); // Save current audio settings
        graphicManager.SaveGraphicsData(); // Save current graphic settings
        PlayerLeaderboardScore.SaveLoadedData();
        ProgressSaver.DeleteWorldData();//DeleteOnlyWorldData();
    }

    public void OnSkipGuide() {
        if (!isWorldDataExist)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_are_you_serious);
    }

    public void OnCompleteGuide() {
        if (isWorldDataExist)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_you_have_my_respect);
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

    public void DeactivateRespawnWnd() => respawnWnd.gameObject.SetActive(false);
    public void OnExitButtonClicked() => Application.Quit();
}