using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


/// <summary>
/// відповідає за активніть графічного інтерфейсу пов'язану з грою
/// responsible for the activity of the graphical interface related to the game
/// </summary>
public class MainmenuManager : MonoBehaviour {
    [SerializeField] AudioManager audioManager;
    [SerializeField] GraphicManager graphicManager;
    [SerializeField] Light mainMenuLight;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] GameObject pauseWnd;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] GameObject continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] FaderScene faderScene;
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject guideBookWnd;
    [SerializeField] GameObject respawnWnd;
    [SerializeField] TextMeshProUGUI respawnTimer;
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] GameObject[] gamemodePauseTitles;
    [SerializeField] Image gameModeWndBackground;
    [SerializeField] Image signInIcon;

    public static readonly string GAME_SCENE_NAME = "Game";
    public static readonly string MAINMENU_SCENE_NAME = "MainMenu";
    public static readonly string LOADING_SCENE_NAME = "LoadScene";

    private int previousLocaleIndex = 0;
    private bool isGMneedAnimation = true;
    private Color targetGMwndColor;
    private Color targetLightGMcolor;
    private GPG_Manager gpg_manager;
    private int respawnTime = 0;

    private readonly Color EASY_GM_COLOR = new Color(0.7058824f, 1f, 0.5411765f, 1f);
    private readonly Color MEDIUM_GM_COLOR = new Color(0.8207547f, 0.5232103f, 0f, 1f);
    private readonly Color HARD_GM_COLOR = new Color(0.6698113f, 0.09033603f, 0f, 1f);
    private readonly Color LIGHT_DEFAULT_COLOR = new Color(1f, 0.8671192f, 0.504717f, 1f);
    private readonly Color LIGHT_MEDIUM_COLOR = new Color(0.4544322f, 0.4999117f, 0.8679245f, 1f);
    private readonly Color LIGHT_HARD_COLOR = new Color(0f, 0f, 0f, 1f);
    private readonly Color failedSignInColor = new Color(1f, 0.4764151f, 0.4863637f, 1f);
    private readonly Color successSignInColor = new Color(0.678066f, 1f, 0.5707547f, 1f);

    private const string LOCALE_PREF_KEY = "LocaleIndex";
    private const string GM_KEY = "GameMode";
    private const int RESPAWN_TIME = 5;


    private void Awake() {
        GameModeInit();
    }

    private void Start() {
        if (faderScene != null)        // Ensure the fader scene is not destroyed on load
            faderScene?.gameObject.SetActive(true);

        if (continueButton != null)// Initialize the continue button visibility based on saved data
            continueButton?.SetActive(ProgressSaver.IsDataExist());

        LanguageInit();

        if (spawnManager != null)
            spawnManager.OnPlayerDied += ActivateRespawnWnd;
    }

    void ActivateRespawnWnd() {
        respawnWnd.SetActive(true);
        respawnTime = RESPAWN_TIME;

        InvokeRepeating(nameof(RespawnWndTimer), 0f, 1f);
    }

    void RespawnWndTimer() {
        respawnTimer.text = respawnTime.ToString();
        --respawnTime;

        if (GameAdsManager.Instance.IsAdStartShow || !respawnWnd.activeInHierarchy) {
            CancelInvoke(nameof(RespawnWndTimer));
            return;
        }

        if (respawnTime < 0)
            GameAdsManager.Instance.OnRetryClicked();
    }

    public void ActivatePauseWnd(bool isActivate) {
        pauseWnd.SetActive(isActivate);
    }

    public void ResetAndReloadGame_Hard() {
        ResetGameData();
        PlayerPrefs.SetInt(GM_KEY, (int)GameMode.Hard);
        faderScene.ChangeLoadScene(SceneType.Game);
        faderScene.gameObject.SetActive(true);
    }

    async void LanguageInit() {
        await LocalizationSettings.InitializationOperation.Task; // wait readiness
        previousLocaleIndex = PlayerPrefs.GetInt(LOCALE_PREF_KEY, (int)GameStaticInfo.LocalizationID.English);

        if (previousLocaleIndex < 0 || previousLocaleIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
            previousLocaleIndex = 0;

        if (LocalizationSettings.AvailableLocales.Locales.Count > previousLocaleIndex)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[previousLocaleIndex];
        if (languageDropdown != null) {
            languageDropdown.value = previousLocaleIndex;
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
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
        PlayerPrefs.SetInt(LOCALE_PREF_KEY, index); // Save the selected locale index

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

    public void InitGPGServices(GPG_Manager gpg_manager) {
        this.gpg_manager = gpg_manager;
    }

    public void OnServicesButton(int buttonIndex) {
        switch (buttonIndex) {
            case 0:
            gpg_manager.OnShowLeaderboard();
            break;
            case 1:
            gpg_manager.OnShowAchievements();
            break;
            case 2:
            gpg_manager.OnSign();
            break;
        }
    }

    //open ar u sur wnd if player already has saved game data
    public void OnNewGameClick() {
        if (ProgressSaver.IsDataExist())
            areUSureWnd?.SetActive(true);
        else
            guideBookWnd?.SetActive(true);
    }

    public void ChangeSignInColor(bool isSignIn) {
        if (signInIcon != null)
            signInIcon.color = (isSignIn) ?
                successSignInColor : failedSignInColor;
    }

    public static void OnGameSceneLoad() {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(GAME_SCENE_NAME);
    }

    public static void OnMainMenuLoad() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MAINMENU_SCENE_NAME);
    }

    public static void OnLoadingSceneLoad() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(LOADING_SCENE_NAME);
    }

    public void ResetGameData() {
        PlayerLeaderboardScore.LoadSavedData();
        PlayerPrefs.DeleteAll();//delete all player prefs data 
        PlayerPrefs.SetInt(LOCALE_PREF_KEY, LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale)); //except settings data 
        audioManager.SaveAudioData(); // Save current audio settings
        graphicManager.SaveGraphicsData(); // Save current graphic settings
        PlayerLeaderboardScore.SaveLoadedData();
        ProgressSaver.Delete();
    }

    public void OnSkipGuide() {
        if (!ProgressSaver.IsDataExist())
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_are_you_serious);
    }

    public void OnCompleteGuide() {
        if (ProgressSaver.IsDataExist())
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_you_have_my_respect);
    }


    public void DeactivateRespawnWnd() => respawnWnd.SetActive(false);
    public void OnExitButtonClicked() => Application.Quit();
}