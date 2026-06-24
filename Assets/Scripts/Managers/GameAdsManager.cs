using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;
using TMPro;


public enum AdTarget {
    None,
    JustRespawn,
    ExtraLife
}

public class GameAdsManager : MonoBehaviour {
    public static GameAdsManager Instance { get; private set; }

    [SerializeField] SpawnManager spawnManager;
    [SerializeField] MainmenuManager gui;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] List<TextMeshProUGUI> limitTxtUI = null;
    [Header("Revive Buttons (Rewarded)")]
    [SerializeField] List<Button> reviveButtons = null;
    [Header("Retry Buttons (Interstitial)")]
    [SerializeField] List<Button> restartButtons = null;
    [Header("Show Ad (Interstitial)")]
    [SerializeField] List<Button> simpleAdButtons = null;

    //public SpawnManager SpawnManager => spawnManager;
    //public MainmenuManager MainmenuManager => gui;
    //public GameObject LoadingPanel=> loadingPanel;
    //public List<Button> RevertiveButtons => reviveButtons;
    //public List<Button> RestartButtons => restartButtons;
    //public List<TextMeshProUGUI> LimitTxtUI => limitTxtUI;
    public bool IsAdStartShow { get; private set; } = false;

    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;
    private bool isSdkInitialized = false;
    private AdTarget adTarget = AdTarget.None;
    private float timeAfterInterstetial = 0f;

    private const string APP_KEY = "26b8c37b5";
    private const string REWARD_AD_UNIT_ID = "shsn3zi2t35xoese";
    private const string INTERSTITIAL_AD_UNIT_ID = "6s73h4sri4pvs62u";
    private const string USER_ID_KEY = "userId";
    private const string REWARDED_LIMIT_KEY = "DailyRewardedCount";
    private const string LAST_AD_DATE_KEY = "LastAdDate";
    private const int MAX_DAILY_REWARDED = 22;
    private const float INTERSTETIAL_TIME_INTERVAL = 2 * 60;//m*s (m)


    private void Awake() {
        if (Instance != null && Instance != this) {
            Instance.InitGameUI(spawnManager, gui, loadingPanel, reviveButtons, restartButtons, simpleAdButtons, limitTxtUI);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void InitGameUI(SpawnManager spawnManager, MainmenuManager gui, GameObject loadingPanel, List<Button> reviveButtons, List<Button> restartButtons, List<Button> simpleAdButtons, List<TextMeshProUGUI> limitTxtUI) {
        this.spawnManager = this.spawnManager == null ? spawnManager : this.spawnManager;
        this.gui = this.gui == null ? gui : this.gui;
        this.loadingPanel = this.loadingPanel == null ? loadingPanel : this.loadingPanel;

        this.limitTxtUI = IsListNeedToCopy(this.limitTxtUI, limitTxtUI) ?
            new List<TextMeshProUGUI>(limitTxtUI) : this.limitTxtUI;
        this.reviveButtons = IsListNeedToCopy(this.reviveButtons, reviveButtons) ?
            new List<Button>(reviveButtons) : this.reviveButtons;
        this.restartButtons = IsListNeedToCopy(this.restartButtons, restartButtons) ?
            new List<Button>(restartButtons) : this.restartButtons;
        this.simpleAdButtons = IsListNeedToCopy(this.simpleAdButtons, simpleAdButtons) ?
            new List<Button>(simpleAdButtons) : this.simpleAdButtons;

        InitAdButtons();
    }

    void Start() {
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        //LevelPlay.SetMetaData("is_test_suite", "enable");
        LevelPlay.Init(APP_KEY, GetUserID());
    }

    bool IsInterstetialTime => Time.time - timeAfterInterstetial >= INTERSTETIAL_TIME_INTERVAL;
    void ResetInterstetialTime() => timeAfterInterstetial = Time.time;

    bool IsListNeedToCopy<T>(List<T> currentList, List<T> newList) =>
         (currentList == null || currentList.Count == 0 || currentList[0] == null)
            && newList != null && newList.Count > 0;

    void InitAdButtons() {
        foreach (Button b in reviveButtons) {
            if (b != null)
                b.onClick.AddListener(OnReviveClicked);
        }

        foreach (Button b in restartButtons) {
            if (b != null)
                b.onClick.AddListener(OnRetryClicked);
        }

        foreach (Button b in simpleAdButtons)
            if (b != null)
                b.onClick.AddListener(() => OnShowInterstitialAd());

        SetButtonsInteractable(true);
    }

    private string GetUserID() {
        if (!PlayerPrefs.HasKey(USER_ID_KEY)) {
            string userId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(USER_ID_KEY, userId);
            return userId;
        }
        return PlayerPrefs.GetString(USER_ID_KEY);
    }

    void SdkInitializationCompletedEvent(LevelPlayConfiguration lc) {
        isSdkInitialized = true;
        Debug.Log("The LevelPlay SDK has been successfully initialized!");

        if (rewardedAd == null) {
            rewardedAd = new LevelPlayRewardedAd(REWARD_AD_UNIT_ID);
            rewardedAd.OnAdLoaded += (adInfo) => Debug.Log("Rewarded loaded");
            rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            rewardedAd.OnAdRewarded += RewardedAdOnAdRewardedEvent;
            rewardedAd.OnAdClosed += OnRewardedAdClosed;

            rewardedAd.LoadAd();
        }

        if (interstitialAd == null) {
            interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_UNIT_ID);
            interstitialAd.OnAdLoaded += (adInfo) => Debug.Log("Interstitial завантажено");
            interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            interstitialAd.OnAdClosed += OnInterstitialAdClosed;

            interstitialAd.LoadAd();//load in backround
        }
    }

    void SdkInitializationFailedEvent(LevelPlayInitError le) {
        isSdkInitialized = false;
        Debug.LogError($"LevelPlay initialization error: {le}");
    }

    private void OnReviveClicked() {//rewarded
        adTarget = AdTarget.ExtraLife;
        IsAdStartShow = true;

        // check premium\founders
        if (ProgressSaver.IsPlayerHasAdImmunitet().premium) {
            Debug.Log("The player has a premium account! We'll revive it for free.");
            RewardedRewardBeh();
            IsAdStartShow = false;
            return;
        }

        if (!isSdkInitialized) {
            Debug.LogWarning("The ad isn't ready yet; the SDK is initializing.");
            adTarget = AdTarget.None;
            IsAdStartShow = false;
            return;
        }

        // check day limit
        if (!CanWatchRewarded()) {
            Debug.Log("The free revival attempts have ended for today!");
            adTarget = AdTarget.None;
            IsAdStartShow = false;
            return;
        }

        //check add readiness
        if (rewardedAd != null && rewardedAd.IsAdReady()) {
            SetButtonsInteractable(false);
            rewardedAd.ShowAd();
            rewardedAd.LoadAd();
        }
        else {//try load again if failed load           
            IsAdStartShow = true;
            Debug.Log("Rewarded isn't ready. Let's try downloading it...");
            loadingPanel.SetActive(false);
            //gui.ActivateRespawnWnd();
            rewardedAd?.LoadAd();
        }
    }

    /// <summary>
    /// return true if ad should appear
    /// </summary>
    /// <returns></returns>
    public bool OnShowInterstitialAd() {//just show ad (used in pause menu on exit)
        if (IsInterstetialTime)
            ResetInterstetialTime();
        else
            return false;

        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined. Skip the ad.");
            return false;
        }

        IsAdStartShow = true;
        if (interstitialAd != null && interstitialAd.IsAdReady()) {
            interstitialAd.ShowAd();
            interstitialAd.LoadAd();
            return true;
        }

        return false;
    }

    void RewardedAdOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward) {
        // recalc limits
        int currentCount = PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, 0);
        PlayerPrefs.SetInt(REWARDED_LIMIT_KEY, currentCount + 1);
        PlayerPrefs.Save();
        SetLimitAdAmountTxt(MAX_DAILY_REWARDED - currentCount);
        IsAdStartShow = false;

        RewardedRewardBeh();
    }

    void SetLimitAdAmountTxt(int currentCnt) {
        foreach (TextMeshProUGUI t in limitTxtUI)
            t.text = currentCnt + "/" + MAX_DAILY_REWARDED;
    }

    void OnRewardedAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        IsAdStartShow = false;
    }

    void OnRewardedLoadFailed(LevelPlayAdError error) {
        IsAdStartShow = false;
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        Debug.LogWarning($"Rewarded loading error: {error}");
    }

    public void OnRetryClicked() {//revive -> interstitial
        adTarget = AdTarget.JustRespawn;

        if (IsInterstetialTime)
            ResetInterstetialTime();
        else {
            InterstitialRespawnBeh();
            return;
        }

        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            InterstitialRespawnBeh();
            return;
        }

        loadingPanel.SetActive(true);
        if (interstitialAd != null && interstitialAd.IsAdReady()) {
            SetButtonsInteractable(false);
            interstitialAd.ShowAd();
            interstitialAd.LoadAd();
        }
        else {
            //ad not ready (haha classic lucky u - bum i
            Debug.Log("The interstitial isn't ready. Just restart the game.");
            InterstitialRespawnBeh();
        }
    }

    void OnInterstitialAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        IsAdStartShow = false;
        interstitialAd.LoadAd(); //load immediately load on backgr new ad
        InterstitialRespawnBeh();
    }

    void OnInterstitialLoadFailed(LevelPlayAdError error) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        IsAdStartShow = false;
        Debug.LogWarning($"Loading error Interstitial: {error}");
    }


    /// <summary>
    /// premium - delete interstitial and rewardeed ads, noAds - only del interstitial
    /// </summary>
    /// <returns></returns>
    //private (bool premium, bool noAds) IsPlayerHasAddImmunitet() {
    //    if (ProgressSaver.IsDataExist()) {
    //        SaveData_Player data = ProgressSaver.Load();
    //        if (data != null)
    //            return (data.isPremiumUser, data.isNoAD);
    //    }
    //    return (false, false);
    //}

    private bool CanWatchRewarded() {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        string lastDate = PlayerPrefs.GetString(LAST_AD_DATE_KEY, "");

        if (lastDate != today) {
            PlayerPrefs.SetString(LAST_AD_DATE_KEY, today);
            PlayerPrefs.SetInt(REWARDED_LIMIT_KEY, 0);
            PlayerPrefs.Save();
        }

        return PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, 0) < MAX_DAILY_REWARDED;
    }

    private void SetButtonsInteractable(bool interactable) {
        foreach (var b in reviveButtons)
            if (b != null)
                b.interactable = interactable;
        foreach (var b in restartButtons)
            if (b != null)
                b.interactable = interactable;
        foreach (var b in simpleAdButtons)
            if (b != null)
                b.interactable = interactable;
    }

    private void RewardedRewardBeh() {
        if (adTarget == AdTarget.ExtraLife) {
            Debug.Log("RevivePlayerLogic: A fish comes back to life at the very spot where it died, with the same level!");
            gui.DeactivateRespawnWnd();
            spawnManager.RespawnPlayer(true, true);

            adTarget = AdTarget.None;
        }
    }

    private void InterstitialRespawnBeh() {
        if (adTarget == AdTarget.JustRespawn) {
            gui.DeactivateRespawnWnd();
            spawnManager.RespawnPlayer(true, false);
            //Debug.Log("RestartGameLogic: The fish falls off the platform and the game restarts.");

            adTarget = AdTarget.None;
        }
    }

    private void OnDestroy() {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }
}