using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;



//public enum AchievementKey {
//    EnglishPlease,
//    FriendNow,
//    GiveItBack,
//    AreYouSerious,
//    Respect,
//    FlashSpeed,
//    Cannibalism,
//    Impossible,
//    SoThatsHow,
//    CantReachMe,
//    ThreatSnack,
//    HereWeGo,
//    Danger,
//    NoEscape,
//    MackerelAgain,
//    DotDash,
//    TrueSpeed,
//    BigGum,
//    BringNails,
//    GrrrMeow,
//    ButItsGray,
//    KingAbyss,
//    Inevitability,
//    MarineMedicine,
//    Beauty,
//    Gourmet,
//    HardWay
//}

//public enum LederboardKey {
//    EasyModeScore,
//    MediumModeScore,
//    HardModeScore,
//    //DeadInEasyModeScore
//}

/// <summary>
/// Google Play Games Manager
/// </summary>
public class GPG_Manager : MonoBehaviour {
    public static GPG_Manager Instance { get; private set; }
    [SerializeField] MainmenuManager mainmenu;
    private bool isAuthenticated = false;

    private static readonly WaitForSeconds SECOND_WAIT = new WaitForSeconds(1f);
    //private const string FOUNDERS_REEF_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQIQ";
    //private const string EASY_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQHg";
    //private const string MEDIUM_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQFw";
    //private const string HARD_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQHw";
    //private const string EASY_DEATH_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQIA";


    void Awake() {
        if (Instance == null) {
            Instance = this;
            Instance.ReInit(mainmenu);
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            //SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { OnSign(); };//every time after scene load (game->mainmenu expmpl) - re-authenticate
        }
        else
            Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => OnSign();

    void ReInit(MainmenuManager mainmenu) {
        this.mainmenu =
            this.mainmenu == null ?
                mainmenu : this.mainmenu;
    }

    public void OnShowAchievements() {
        //Social.ShowAchievementsUI();
        if (!isAuthenticated) {
            Debug.LogWarning("Cannot show achievements, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowAchievements called");
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }

    public void OnShowLeaderboard() {
        if (!isAuthenticated) {
            Debug.LogWarning("Cannot show leaderboard, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowLeaderboard called");
        PlayGamesPlatform.Instance.ShowLeaderboardUI();
    }

    IEnumerator TryAuthenticateWithDelay() {
        yield return SECOND_WAIT; // Delay for Google Play init
        if (this == null)//in case fast open game scene
            StopAllCoroutines();

        //Debug.Log("Trying to authenticate...");
        PlayGamesPlatform.Instance.Authenticate((result) => {
            if (result == SignInStatus.Success) {
                //Debug.Log("Authentication SUCCESS");
                isAuthenticated = true;
            }
            else {
                Debug.Log("Authentication FAILED: " + result);
                isAuthenticated = false;
            }
        });

        UpdateMainMenuUI();

        //if (mainmenu == null)
        //    mainmenu = GameObject.FindWithTag("MainMenu")?.GetComponent<MainmenuManager>();
        ////Debug.Log("Main menu found: " + (mainmenu != null));

        //yield return null; // Wait for the main menu to be ready

        //mainmenu?.InitGPGServices(this);
        //mainmenu?.ChangeSignInColor(isAuthenticated);
    }

    private void UpdateMainMenuUI() {
        if (mainmenu == null) {
            mainmenu = GameObject.FindWithTag("MainMenu")?.GetComponent<MainmenuManager>();
        }

        if (mainmenu != null) {
            mainmenu.InitGPGServices(this);
            mainmenu.ChangeSignInColor(isAuthenticated);
        }
    }

    public void OnSign() {
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
        StartCoroutine(TryAuthenticateWithDelay());
        //Debug.Log("Attempting authentication...");
    }

    public void OnSavePlayerScoreToLeaderBoard(string leaderboard, long score) {
        PlayGamesPlatform.Instance.ReportScore(score, leaderboard, success => {
            Debug.Log("Reporting score: " + score);
            if (success)
                Debug.Log("Score report success");
            else
                Debug.LogWarning("Score report failed.");
        });
    }

    public void OnSavePlayerDeathsToLeaderBoard(int deaths) {
        PlayGamesPlatform.Instance.ReportScore(deaths, GPGSIds.leaderboard_easiest_mode__hardest, success => {
            Debug.Log("Reporting deaths: " + deaths);
            if (success)
                Debug.Log("Deaths report success");
            else
                Debug.LogWarning("Deaths report failed.");
        });
    }

    public void OnAddToFoundersLeaderboard() {
        PlayGamesPlatform.Instance.ReportScore(0, GPGSIds.leaderboard_the_founders_reef, success => {
            Debug.Log("Reporting founders...");
            if (success)
                Debug.Log("Founders report success");
            else
                Debug.LogWarning("Foundersreport failed.");
        });

    }

    public static void OnTriggerAchievement(/*AchievementKey*/string achievementKey) =>        //Debug.LogWarning(key.ToString());
        PlayGamesPlatform.Instance.UnlockAchievement(/*achievements[achievementKey]*/achievementKey);

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded/*(Scene scene, LoadSceneMode mode) => { OnSign(); }*/;
    }
}

