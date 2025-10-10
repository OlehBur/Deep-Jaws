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
    Gourmet
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
            { AchievementKey.Gourmet, "CgkI0PmEnbsFEAIQHA"   }//
        };
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
        PlayGamesPlatform.Instance.Authenticate((result) =>
        {
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

    public void OnSavePlayerScoreToLeaderBoard(long score) {
        //const int PLAYER_CNT = 1;
        const string LEADERBOARD_ID = "CgkI0PmEnbsFEAIQFw";
        //    PlayGamesPlatform.Instance.LoadScores(leaderboardId, LeaderboardStart.PlayerCentered, PLAYER_CNT, LeaderboardCollection.Public, LeaderboardTimeSpan.AllTime,
        //(data) =>
        //{
        //    long currentScore = (data.Valid && data.PlayerScore != null) ?
        //        0 : data.PlayerScore.value;
        //    long newScore = currentScore + score;
        //    //Debug.LogWarning("Current Score: " + currentScore + ", New Score: " + newScore);

        PlayGamesPlatform.Instance.ReportScore(/*newScore*/score, LEADERBOARD_ID, success =>
        {
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

    public static void OnTriggerAchievement(AchievementKey key) {
        //Debug.LogWarning(key.ToString());
        PlayGamesPlatform.Instance.UnlockAchievement(achievements[key]);
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= (Scene scene, LoadSceneMode mode) => { OnSign(); };
    }
}

