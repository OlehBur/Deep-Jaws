using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


/// <summary>
/// відповідає за активніть графічного інтерфейсу пов'язану з грою
/// responsible for the activity of the graphical interface related to the game
/// </summary>
public class MainmenuManager : MonoBehaviour {
    public static readonly string GAME_SCENE_NAME = "Game";
    public static readonly string MAINMENU_SCENE_NAME = "MainMenu";
    public static readonly string LOADING_SCENE_NAME = "LoadScene";
    private const string LOCALE_PREF_KEY = "LocaleIndex";
    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject continueButton; //for ensuring the visibility of the continue game button if there is saved data
    [SerializeField] GameObject faderScene;
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject guideBookWnd;
    [SerializeField] TMP_Dropdown languageDropdown;
    int previousLocaleIndex = 0;

    [SerializeField] Image signInIcon;
    private readonly Color failedSignInColor = new Color(1f, 0.4764151f, 0.4863637f, 1f);
    private readonly Color successSignInColor = new Color(0.678066f, 1f, 0.5707547f, 1f);
    private GPG_Manager gpg_manager;

    private void Awake() {
        // Ensure the fader scene is not destroyed on load
        if (faderScene != null)
            faderScene?.SetActive(true);

        // Initialize the continue button visibility based on saved data
        if (continueButton != null)
            continueButton?.SetActive(ProgressSaver.IsDataExist());

        //init locale
        previousLocaleIndex = PlayerPrefs.GetInt(LOCALE_PREF_KEY, (int)GameStaticInfo.LocalizationID.English);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[previousLocaleIndex]; // Set default locale
        languageDropdown.value = previousLocaleIndex;
        languageDropdown?.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index) {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt(LOCALE_PREF_KEY, index); // Save the selected locale index

        if (previousLocaleIndex != index)
            GPG_Manager.OnTriggerAchievement(AchievementKey.EnglishPlease);

        previousLocaleIndex = index;
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

    public /*static*/ void ResetGameData() {
        PlayerPrefs.DeleteAll();//delete all player prefs data 
        PlayerPrefs.SetInt(LOCALE_PREF_KEY, LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale)); //except settings data 
        audioManager.SaveAudioData(); // Save current audio settings
        ProgressSaver.Delete();
    }

    public void OnExitButtonClicked() {
        // Exit the application
        Application.Quit();
    }

    public void OnSkipGuide() {
        if (!ProgressSaver.IsDataExist())
            GPG_Manager.OnTriggerAchievement(AchievementKey.AreYouSerious);
    }

    public void OnCompleteGuide() {
        if (ProgressSaver.IsDataExist())
            GPG_Manager.OnTriggerAchievement(AchievementKey.Respect);
    }

}