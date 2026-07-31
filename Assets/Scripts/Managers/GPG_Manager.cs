using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Reflection;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine.SocialPlatforms;//?!

/// <summary>
/// Google Play Games Manager
/// </summary>
public class GPG_Manager : MonoBehaviour {
    public static GPG_Manager Instance { get; private set; } = null;
    public event Action<string> OnLoadKingOfDeeps;

    [SerializeField] MainmenuManager mainmenu;
    [SerializeField] GamePurchaseManager purchaseManager;

    private static readonly WaitForSeconds SECOND_WAIT = new WaitForSeconds(1f);

    private const float WAKKE_UP_ACHIEVEMENT_TIME = 180 * 60f;

    private bool isAuthenticated = false;


    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            Invoke(nameof(OnTriggerWakeUpAchi), WAKKE_UP_ACHIEVEMENT_TIME);
        }
        else {
            Instance.ReInit(mainmenu, purchaseManager);
            Destroy(gameObject);
        }

        UpdateMainMenuUI();
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
        ShowNoConnectionIfNeed();
        //Social.ShowAchievementsUI();
        if (!isAuthenticated) {
            Debug.LogWarning("Cannot show achievements, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowAchievements called");

        if (mainmenu != null)
            mainmenu.ActivateLoadingPanelWnd(true);
        //PlayGamesPlatform.Instance.ShowAchievementsUI();
        PlayGamesPlatform.Instance.ShowAchievementsUI(OnLeaderboardAchiClosed);

    }

    public void OnShowLeaderboard() {
        ShowNoConnectionIfNeed();
        if (!isAuthenticated) {
            Debug.LogWarning("Cannot show leaderboard, not authenticated.");
            return;
        }
        //Debug.LogWarning("OnShowLeaderboard called");

        if (mainmenu != null)
            mainmenu.ActivateLoadingPanelWnd(true);
        //PlayGamesPlatform.Instance.ShowLeaderboardUI();
        PlayGamesPlatform.Instance.ShowLeaderboardUI(
            null,
            LeaderboardTimeSpan.AllTime,
            OnLeaderboardAchiClosed
        );
    }

    private void OnLeaderboardAchiClosed(UIStatus status) {
        Debug.Log($"Leaderboard or Achievement UI closed with status: {status}");

        if (mainmenu != null)
            mainmenu.ActivateLoadingPanelWnd(false);

        if (status != UIStatus.Valid)
            Debug.LogWarning($"Failed to open Leaderboards | achievements UI. Status: {status}");
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
                    DateTime releaseDate = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc);
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
        if (mainmenu != null)
            mainmenu.ActivateLoadingPanelWnd(true);

        yield return SECOND_WAIT; // Delay for Google Play init
        if (this == null)//in case fast open game scene
            StopAllCoroutines();

        //Debug.Log("Trying to authenticate...");
        PlayGamesPlatform.Instance.Authenticate((result) => {
            if (result == SignInStatus.Success) {
                //Debug.Log("Authentication SUCCESS");
                isAuthenticated = true;
                mainmenu.ChangeSignInColor(true);//DON`T DELETE  - working Hack 
                GetTop1Player();

                CheckFounderStatus((isFounder) => {
                    if (isFounder) {
                        AccountData currentData = ProgressSaver.IsAccountDataExist ?
                        ProgressSaver.LoadAccountData() : new AccountData();

                        currentData.isFounder = true;//only upd data in save
                        currentData.isPremiumUser = true;
                        currentData.isNoAD = true;

                        OnAddToFoundersLeaderboard();
                        ProgressSaver.SaveData(currentData);

                        purchaseManager.RefreshUI();
                    }
                });
            }
            else {
                Debug.Log("Authentication FAILED: " + result);
                isAuthenticated = false;
            }

            if (mainmenu != null)
                mainmenu.ActivateLoadingPanelWnd(false);
        });

        UpdateMainMenuUI();
    }

    private void UpdateMainMenuUI() {
        if (mainmenu == null)
            mainmenu = GameObject.FindWithTag("MainMenu")?.GetComponent<MainmenuManager>();

        if (mainmenu != null) {
            mainmenu.InitGPGServices(this);
            mainmenu.ChangeSignInColor(isAuthenticated);
        }
    }

    public void OnSign() {
        ShowNoConnectionIfNeed();

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        StartCoroutine(TryAuthenticateWithDelay());
        //Debug.Log("Attempting authentication...");
    }

    void ShowNoConnectionIfNeed() {
        if (Application.internetReachability == NetworkReachability.NotReachable) 
            mainmenu.ActivateNoInternetConnectionWnd(true);
    }

    public void OnSavePlayerScoreToLeaderBoard(string leaderboard, long score) {
        if (GameManager.IsInternetAvailable) {
            Debug.LogWarning("lederboard Score report failed: there is no internet!");
            return;
        }

        PlayGamesPlatform.Instance.ReportScore(score, leaderboard, success => {
            Debug.Log("Reporting score: " + score);
            if (success)
                Debug.Log("Score report success");
            else
                Debug.LogWarning("Score report failed.");
        });
    }

    public void OnSavePlayerDeathsToLeaderBoard(int deaths) {
        if (GameManager.IsInternetAvailable) {
            Debug.LogWarning("lederboard Score report failed: there is no internet!");
            return;
        }

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

    public void GetTop1Player() {
        if (!isAuthenticated) {
            Debug.LogWarning("Cannot show top player, you are not authenticated.");
            return;
        }

        //load first entry (rowCount = 1), starts fr TopScores
        PlayGamesPlatform.Instance.LoadScores(
            GPGSIds.leaderboard_deadly_maelstrom_appetite,
            LeaderboardStart.TopScores,
            1,                          // cnt of results
            LeaderboardCollection.Public,// Public board onl (social for friends - not interested))
            LeaderboardTimeSpan.AllTime,
            OnTopScoreLoaded
        );
    }

#pragma warning disable 0618
    private void OnTopScoreLoaded(LeaderboardScoreData scoreData) {
        if (scoreData.Valid && scoreData.Scores.Length > 0) {
            IScore topScore = scoreData.Scores[0];
            long scoreValue = topScore.value;
            //int rank = topScore.rank;                // must be 1
            string userId = topScore.userID;         // uniq ID fr Google Play
            Debug.Log($"Top-1 player has {scoreValue} pnts! Rank: -, UserID: {userId}");//            { rank}

            //load user profile to get nickname
            GetPlayerName(userId/*, scoreValue*/);
        }
        else
            Debug.LogError("Failed to load <Top-1 player> or leaderboard is empty.");
    }

    private void GetPlayerName(string userId/*, long score*/) {
        /*Social.LoadUsers*/
        PlayGamesPlatform.Instance.LoadUsers(new string[] { userId }, (IUserProfile[] profiles) => {
            if (profiles != null && profiles.Length > 0) {
                string userName = profiles[0].userName;
                //Texture2D avatar = profiles[0].image;   
                OnLoadKingOfDeeps?.Invoke(userName);
            }
            else {
                Debug.Log($"Nickname not found, player {userId} ");//with {score}
            }
        });
    }
#pragma warning restore 0618

    void OnTriggerWakeUpAchi() {
        OnTriggerAchievement(GPGSIds.achievement_its_time_to_wake_up);
        //Debug.LogWarning(GPGSIds.achievement_its_time_to_wake_up);
    }

    public static void OnTriggerAchievement(string achievementKey) {
        if (GameManager.IsInternetAvailable) {
            Debug.LogWarning("achievement is not included: there is no internet!");
            return;
        }

        PlayGamesPlatform.Instance.UnlockAchievement(achievementKey);
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
