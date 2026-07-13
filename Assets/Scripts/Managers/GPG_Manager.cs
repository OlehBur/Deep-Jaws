using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Reflection;
using UnityEngine.SocialPlatforms;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using TMPro;



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
    [SerializeField] GamePurchaseManager purchaseManager;
    private bool isAuthenticated = false;

    private static readonly WaitForSeconds SECOND_WAIT = new WaitForSeconds(1f);


    void Awake() {
        if (Instance == null) {
            Instance = this;
            Instance.ReInit(mainmenu, purchaseManager);
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => OnSign();

    void ReInit(MainmenuManager mainmenu, GamePurchaseManager purchaseManager) {
        this.mainmenu =
            this.mainmenu == null ?
                mainmenu : this.mainmenu;
        this.purchaseManager = this.purchaseManager == null ?
            purchaseManager : this.purchaseManager;
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

    public void CheckFounderStatus(System.Action<bool> onComplete) {
        if (!isAuthenticated) {
            Debug.Log("I can't check the status: the user is not logged in.");
            onComplete(false);
            return;
        }

        Debug.LogWarning("TODO: change founder status date condition");
        PlayGamesPlatform.Instance.LoadAchievements((achievements) => {
            string targetAchievementId = GPGSIds.achievement_youre_my_friend_now;
            var achievement = achievements.FirstOrDefault(a => a.id == targetAchievementId);

            if (achievement != null && achievement.completed) {
                //  REFLECTION
                PropertyInfo prop = achievement.GetType().GetProperty("lastReportedDate");

                if (prop != null) {
                    DateTime achievementDate = (DateTime)prop.GetValue(achievement, null);
                    DateTime releaseDate = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);
                    Debug.LogWarning(achievementDate);

                    if (achievementDate < releaseDate) {
                        Debug.Log($"Founder exist: {achievementDate}");
                        onComplete(true);
                        return;
                    }
                }
            }
            onComplete(false);
        });
    }

    IEnumerator TryAuthenticateWithDelay() {
        yield return SECOND_WAIT; // Delay for Google Play init
        if (this == null)//in case fast open game scene
            StopAllCoroutines();

        if (mainmenu != null)
            mainmenu.loadingPanel.SetActive(true);
        //Debug.Log("Trying to authenticate...");
        PlayGamesPlatform.Instance.Authenticate((result) => {
            if (result == SignInStatus.Success) {
                //Debug.Log("Authentication SUCCESS");
                isAuthenticated = true;
                mainmenu.ChangeSignInColor(true);

                CheckFounderStatus((isFounder) => {
                    if (isFounder) {
                        SaveData currentData = ProgressSaver.IsDataExist() ?
                        ProgressSaver.Load() : new SaveData();

                        currentData.isFounder = true;//only upd data in save
                        currentData.isPremiumUser = true;
                        currentData.isNoAD = true;

                        OnAddToFoundersLeaderboard();
                        ProgressSaver.Save(currentData);

                        purchaseManager.RefreshUI();
                    }
                });
            }
            else {
                Debug.Log("Authentication FAILED: " + result);
                isAuthenticated = false;
            }

            if (mainmenu != null)
                mainmenu.loadingPanel.SetActive(false);
        });

        UpdateMainMenuUI();
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

    public static void OnTriggerAchievement(string achievementKey) =>
        PlayGamesPlatform.Instance.UnlockAchievement(achievementKey);

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

