using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;



public enum AchievementKey {
    EnglishPlease,
    FriendNow,
    GiveItBack,
    AreYouSerious,
    Respect,
    FlashSpeed,
    Cannibalism,
    Impossible,
    SoThatsHow,
    CantReachMe,
    ThreatSnack,
    HereWeGo,
    Danger,
    NoEscape,
    MackerelAgain,
    DotDash,
    TrueSpeed,
    BigGum,
    BringNails,
    GrrrMeow,
    ButItsGray,
    KingAbyss,
    Inevitability,
    MarineMedicine,
    Beauty,
    Gourmet,
    HardWay
}

public enum LederboardKey {
    EasyModeScore,
    MediumModeScore,
    HardModeScore,
    //DeadInEasyModeScore
}

/// <summary>
/// Google Play Games Manager
/// </summary>
public class GPG_Manager : MonoBehaviour {
    public static GPG_Manager Instance { get; private set; }
    private bool isAuthenticated = false;
    public static readonly Dictionary<AchievementKey, string> achievements = new Dictionary<AchievementKey, string> {
            { AchievementKey.EnglishPlease, "CgkI0PmEnbsFEAIQAQ" },//
            { AchievementKey.FriendNow, "CgkI0PmEnbsFEAIQAg" },//
            { AchievementKey.GiveItBack, "CgkI0PmEnbsFEAIQAw" },//
            { AchievementKey.AreYouSerious, "CgkI0PmEnbsFEAIQBA" },//
            { AchievementKey.Respect, "CgkI0PmEnbsFEAIQBQ" },//
            { AchievementKey.FlashSpeed, "CgkI0PmEnbsFEAIQBg" },//
            { AchievementKey.Cannibalism, "CgkI0PmEnbsFEAIQBw" },//
            { AchievementKey.Impossible, "CgkI0PmEnbsFEAIQCA" },//
            { AchievementKey.SoThatsHow, "CgkI0PmEnbsFEAIQCQ" },//
            { AchievementKey.CantReachMe, "CgkI0PmEnbsFEAIQCg" },//
            { AchievementKey.ThreatSnack, "CgkI0PmEnbsFEAIQCw" },//
            { AchievementKey.HereWeGo, "CgkI0PmEnbsFEAIQDA" },//
            { AchievementKey.Danger, "CgkI0PmEnbsFEAIQDQ" },//
            { AchievementKey.NoEscape, "CgkI0PmEnbsFEAIQDg" },//
            { AchievementKey.MackerelAgain, "CgkI0PmEnbsFEAIQDw" },//
            { AchievementKey.DotDash, "CgkI0PmEnbsFEAIQEA" },//
            { AchievementKey.TrueSpeed, "CgkI0PmEnbsFEAIQEQ" },//
            { AchievementKey.BigGum, "CgkI0PmEnbsFEAIQEg" },//
            { AchievementKey.BringNails, "CgkI0PmEnbsFEAIQEw" },//
            { AchievementKey.GrrrMeow, "CgkI0PmEnbsFEAIQFA" },//
            { AchievementKey.ButItsGray, "CgkI0PmEnbsFEAIQFQ" },//
            { AchievementKey.KingAbyss, "CgkI0PmEnbsFEAIQFg" },//
            { AchievementKey.Inevitability, "CgkI0PmEnbsFEAIQGQ" },//
            { AchievementKey.MarineMedicine, "CgkI0PmEnbsFEAIQGg" },//
            { AchievementKey.Beauty, "CgkI0PmEnbsFEAIQGw"   },//
            { AchievementKey.Gourmet, "CgkI0PmEnbsFEAIQHA"   },//
            { AchievementKey.HardWay, "CgkI0PmEnbsFEAIQHQ"}
        };
    private const string FOUNDERS_REEF_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQIQ";
    private const string EASY_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQHg";
    private const string MEDIUM_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQFw";
    private const string HARD_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQHw";
    private const string EASY_DEATH_LEADERBOARD_ID = "CgkI0PmEnbsFEAIQIA";

    [SerializeField] MainmenuManager mainmenu;
    private static readonly WaitForSeconds SECOND_WAIT = new WaitForSeconds(1f);


    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { OnSign(); };//every time after scene load (game->mainmenu expmpl) - re-authenticate
        }
        else
            Destroy(gameObject);
    }

    //public bool IsAuthenticated() {
    //    return isAuthenticated;
    //}

    public void OnShowAchievements() {
        //Social.ShowAchievementsUI();
        if (!isAuthenticated) {
            //Debug.LogWarning("Cannot show achievements, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowAchievements called");
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }

    public void OnShowLeaderboard() {
        if (!isAuthenticated) {
            //    Debug.LogWarning("Cannot show leaderboard, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowLeaderboard called");
        PlayGamesPlatform.Instance.ShowLeaderboardUI();
    }

    IEnumerator TryAuthenticateWithDelay() {
        yield return SECOND_WAIT; // Delay for Google Play init
        //Debug.Log("Trying to authenticate...");
        PlayGamesPlatform.Instance.Authenticate((result) => {
            if (result == SignInStatus.Success) {
                //Debug.Log("Authentication SUCCESS");
                isAuthenticated = true;
            }
            else {
                //Debug.Log("Authentication FAILED: " + result);
                isAuthenticated = false;
            }
        });

        if (mainmenu == null)
            mainmenu = GameObject.FindWithTag("MainMenu")?.GetComponent<MainmenuManager>();
        //Debug.Log("Main menu found: " + (mainmenu != null));

        yield return null; // Wait for the main menu to be ready

        mainmenu?.InitGPGServices(this);
        mainmenu?.ChangeSignInColor(isAuthenticated);
    }

    public void OnSign() {
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
        StartCoroutine(TryAuthenticateWithDelay());
        //Debug.Log("Attempting authentication...");
        //PlayGamesPlatform.Instance.Authenticate((result) =>
        //{
        //    if (result == SignInStatus.Success) {
        //        Debug.Log("Google Play Games sign-in successful.");
        //        isAuthenticated = true;
        //    }
        //    else {
        //        Debug.LogError("Google Play Games sign-in failed: " + result);
        //        isAuthenticated = false;
        //    }

        //    mainmenu?.ChangeSignInColor(isAuthenticated);
        //});
    }

    public void OnSavePlayerScoreToLeaderBoard(LederboardKey leaderboard, long score) {
        //const int PLAYER_CNT = 1;

        //    PlayGamesPlatform.Instance.LoadScores(leaderboardId, LeaderboardStart.PlayerCentered, PLAYER_CNT, LeaderboardCollection.Public, LeaderboardTimeSpan.AllTime,
        //(data) =>
        //{
        //    long currentScore = (data.Valid && data.PlayerScore != null) ?
        //        0 : data.PlayerScore.value;
        //    long newScore = currentScore + score;
        //    //Debug.LogWarning("Current Score: " + currentScore + ", New Score: " + newScore);
        string LEADERBOARD_ID = leaderboard switch {
            LederboardKey.EasyModeScore => EASY_LEADERBOARD_ID,
            LederboardKey.MediumModeScore => MEDIUM_LEADERBOARD_ID,
            LederboardKey.HardModeScore => HARD_LEADERBOARD_ID,
            //LederboardKey.DeadInEasyModeScore => EASY_DEATH_LEADERBOARD_ID,
            _ => EASY_LEADERBOARD_ID
        };

        PlayGamesPlatform.Instance.ReportScore(/*newScore*/score, LEADERBOARD_ID, success => {
            Debug.Log("Reporting score: " + score);
            if (success)
                Debug.Log("Score report success");
            else
                Debug.LogWarning("Score report failed.");
        });

        //});

        //PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, success => {
        //    if (success) {
        //        Debug.Log("");
        //    }
        //    else {
        //        Debug.LogWarning("");
        //    }
        //});
    }

    public void OnSavePlayerDeathsToLeaderBoard(int deaths) {
        PlayGamesPlatform.Instance.ReportScore(deaths, EASY_DEATH_LEADERBOARD_ID, success => {
            Debug.Log("Reporting deaths: " + deaths);
            if (success)
                Debug.Log("Deaths report success");
            else
                Debug.LogWarning("Deaths report failed.");
        });
    }

    public void OnAddToFoundersLeaderboard() {
        PlayGamesPlatform.Instance.ReportScore(0, FOUNDERS_REEF_LEADERBOARD_ID, success => {
            Debug.Log("Reporting founders...");
            if (success)
                Debug.Log("Founders report success");
            else
                Debug.LogWarning("Foundersreport failed.");
        });
        
            }

    public static void OnTriggerAchievement(AchievementKey key) {
        //Debug.LogWarning(key.ToString());
        PlayGamesPlatform.Instance.UnlockAchievement(achievements[key]);
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= (Scene scene, LoadSceneMode mode) => { OnSign(); };
    }
}

