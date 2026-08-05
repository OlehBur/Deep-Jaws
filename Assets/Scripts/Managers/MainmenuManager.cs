using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization; //  for Uri.EscapeDataString


/// <summary>
/// відповідає за активніть графічного інтерфейсу пов'язану з грою
/// responsible for the activity of the graphical interface related to the game
/// </summary>
public class MainmenuManager : MonoBehaviour {
    [SerializeField] AudioManager audioManager;
    [SerializeField] GraphicManager graphicManager;
    [SerializeField] GameObject loadingPanel;

    [Header("MainMenu Scene")]
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] Light mainMenuLight;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] GameObject continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] SceneLoader faderScene;
    [SerializeField] GameResultWnd gameResult;
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject guideBookWnd;
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] TMP_Dropdown framerateDropdown;
    [SerializeField] Image gameModeWndBackground;
    [SerializeField] Image signInIcon;
    [SerializeField] GameObject kingOfDeepsObj;
    [SerializeField] TextMeshProUGUI kingOfDeepsTxt;
    [SerializeField] LocalizedString kingOfDeepsLocalized;
    [SerializeField] Animator kingOfDeepsAnimator;
    //[SerializeField] LocalizedString settingsMaxFpsLocalized;

    [Header("Game Scene")]
    /// <summary>
    /// only for game scene
    /// </summary>
    [SerializeField] GameObject[] gamemodePauseTitles;
    [SerializeField] GameObject pauseWnd;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] RespawnWnd respawnWnd;

    private int previousLocaleIndex = 0;
    private Framerate currentFramerateIndex;
    private enum Framerate {
        Max,
        F30,
        F60,
        F120
    }
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
    private const string GM_KEY = "GameMode";
    private const string REPORT_EMAIL = "bovv.company@gmail.com";
    private const string REPORT_SUBJECT = "Bug Report: Deep Jaws";


    private void Awake() {
        GameModeInit();
    }

    private void Start() {
        isWorldDataExist = ProgressSaver.IsWorldDataExist;

        if (faderScene != null)        // Ensure the fader scene is not destroyed on load
            faderScene?.gameObject.SetActive(true);

        if (continueButton != null)// Initialize the continue button visibility based on saved data
            continueButton?.SetActive(isWorldDataExist);

        if (framerateDropdown != null) {//if any settings field setted
            SettingsData savedSettings = ProgressSaver.LoadSettingsData();
            LanguageInit(savedSettings);
            FramerateInit(savedSettings);
        }

        if (GameManager.IsInternetNotAvailable)
            FirstNoConnectionOpen();

        if (kingOfDeepsObj != null) {
            kingOfDeepsObj.SetActive(false);
            kingOfDeepsAnimator.enabled = false;
        }
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

    void KingOfDeepsUpdate(string playerNick) {
        if (kingOfDeepsObj == null)
            return;

        kingOfDeepsObj.SetActive(true);
        kingOfDeepsTxt.text = kingOfDeepsLocalized.GetLocalizedString() + " " + playerNick;
        kingOfDeepsAnimator.enabled = true;
    }

    public void ActivateNoInternetConnectionWnd(bool isActive) =>
        noInternetConnectionWnd.SetActive(isActive);
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
        pauseWnd.SetActive(isActivate);
    }

    public void ResetAndReloadGame_Hard() {
        ResetWorldData();
        PlayerPrefs.SetInt(GM_KEY, (int)GameMode.Hard);
        gameResult.AddShellsWithoutWnd();
        faderScene.OnLoad_GameScene();
    }

    async void LanguageInit(SettingsData savedSettings) {
        await LocalizationSettings.InitializationOperation.Task; // wait readiness

        previousLocaleIndex = savedSettings == null ?
            (int)GameStaticInfo.LocalizationID.English : savedSettings.language;
        //PlayerPrefs.GetInt(LOCALE_PREF_KEY, (int)GameStaticInfo.LocalizationID.English);

        if (previousLocaleIndex < 0 || previousLocaleIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
            previousLocaleIndex = 0;

        if (LocalizationSettings.AvailableLocales.Locales.Count > previousLocaleIndex)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[previousLocaleIndex];

        languageDropdown.value = previousLocaleIndex;
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    void FramerateInit(SettingsData savedSettings) {
        List<string> fpsListing = new();
        QualitySettings.vSyncCount = 0;
        int screenMaxFramerate = GameManager.ScreenMaxFramerate;
        //framerateDropdown.ClearOptions();
        //Debug.LogWarning("DROPDOWN ALWAYS CLEANING UP");

        //Load and set Def val
        currentFramerateIndex = savedSettings == null ? Framerate.Max : (Framerate)savedSettings.framerate;
        //currentFramerateIndex = (Framerate)PlayerPrefs.GetInt(FPS_KEY, 0);
        Application.targetFrameRate = currentFramerateIndex switch {
            Framerate.Max => screenMaxFramerate,
            Framerate.F30 => GameStaticInfo.FRAMERATE_30,
            Framerate.F60 => GameStaticInfo.FRAMERATE_60,
            Framerate.F120 => GameStaticInfo.FRAMERATE_120,
            _ => GameStaticInfo.FRAMERATE_30
        };

        //init dropdown
        if (framerateDropdown.options.Count == 0) {
            fpsListing.Add(/*settingsMaxFpsLocalized.GetLocalizedString() +*/ $"{screenMaxFramerate} Hz");//max
            //Debug.LogWarning(screenMaxFramerate);
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_30)
                fpsListing.Add(GameStaticInfo.FRAMERATE_30.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_60)
                fpsListing.Add(GameStaticInfo.FRAMERATE_60.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_120)
                fpsListing.Add(GameStaticInfo.FRAMERATE_120.ToString() + " Hz");

            framerateDropdown.AddOptions(fpsListing);
        }

        framerateDropdown.SetValueWithoutNotify((int)currentFramerateIndex);
        framerateDropdown.onValueChanged.RemoveAllListeners();
        framerateDropdown.onValueChanged.AddListener(OnFramerateChanged);
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

    private void OnFramerateChanged(int index) {
        currentFramerateIndex = (Framerate)index;
        SettingsData savedSettings = ProgressSaver.LoadSettingsData();
        if (savedSettings == null)
            savedSettings = new SettingsData();
        savedSettings.framerate = index;

        ProgressSaver.SaveData(savedSettings);
        //PlayerPrefs.SetInt(FPS_KEY, index);

        Application.targetFrameRate = currentFramerateIndex switch {
            Framerate.Max => GameManager.ScreenMaxFramerate,
            Framerate.F30 => 30,
            Framerate.F60 => 60,
            Framerate.F120 => 120,
            _ => 30
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void GameModeInit() {//init gamemode buttons        
        GameStaticInfo.Gamemode = (GameMode)PlayerPrefs.GetInt(GM_KEY, 0);
        //init gamemode
        if (gamemodePauseTitles != null && gamemodePauseTitles.Length == GameStaticInfo.GAMEMODES_COUNT) {
            foreach (GameObject tit in gamemodePauseTitles)
                tit.SetActive(false);

            gamemodePauseTitles[(int)GameStaticInfo.Gamemode].SetActive(true);
        }
        //Debug.LogWarning(GameStaticInfo.Gamemode);
    }

    IEnumerator ChangeColorGMwnd() {
        //Debug.Log("gm changing color started");
        while (gameModeWndBackground.color != targetGMwndColor) {
            gameModeWndBackground.color = Color.Lerp(gameModeWndBackground.color, targetGMwndColor, Time.deltaTime/* * GM_COLOR_CHANGING_SPEED*/);
            yield return null;
        }

        //Debug.Log("gm Color changed");
    }

    IEnumerator ChangeColorGMmainMenu() {
        while (mainMenuLight.color != targetLightGMcolor) {
            mainMenuLight.color = Color.Lerp(mainMenuLight.color, targetLightGMcolor, Time.deltaTime);
            yield return null;
        }
    }

    private void OnLanguageChanged(int index) {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        SettingsData savedSettings = ProgressSaver.LoadSettingsData();
        if (savedSettings == null)
            savedSettings = new SettingsData();
        savedSettings.language = index;

        ProgressSaver.SaveData(savedSettings);
        //PlayerPrefs.SetInt(LOCALE_PREF_KEY, index); // Save the selected locale index

        if (previousLocaleIndex != index)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_dont_understand);

        previousLocaleIndex = index;
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

        PlayerPrefs.SetInt(GM_KEY, gamemode);

        if (isGMneedAnimation) {
            gmWndAnimator.SetTrigger("Anim");
            isGMneedAnimation = false;
        }
    }

    //public void InitGPGServices(GPG_Manager gpg_manager) {
    //    this.gpg_manager = gpg_manager;

    //    if (gpg_manager != null && kingOfDeepsObj != null)
    //        gpg_manager.OnLoadKingOfDeeps += KingOfDeepsUpdate;
    //}

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

    //public static void OnGameSceneLoad() {
    //    // Load the game scene
    //    UnityEngine.SceneManagement.SceneManager.LoadScene(GameStaticInfo.GAME_SCENE_NAME);
    //}

    //public static void OnMainMenuLoad() {
    //    UnityEngine.SceneManagement.SceneManager.LoadScene(GameStaticInfo.MAINMENU_SCENE_NAME);
    //}

    //public static void OnLoadingSceneLoad() {
    //    UnityEngine.SceneManagement.SceneManager.LoadScene(GameStaticInfo.LOADING_SCENE_NAME);
    //}

    public void ResetWorldData() {
        PlayerLeaderboardScore.LoadSavedData();
        PlayerPrefs.DeleteAll();//delete all player prefs data 
        //PlayerPrefs.SetInt(LOCALE_PREF_KEY, LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale)); //except settings data 
        audioManager.SaveAudioData(); // Save current audio settings
        graphicManager.SaveGraphicsData(); // Save current graphic settings
        PlayerLeaderboardScore.SaveLoadedData();
        //ProgressSaver.Delete();
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