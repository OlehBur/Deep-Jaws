using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;
using TMPro;


public enum AdTarget {
    None,
    JustRespawn,
    ExtraLife
}

public class FishAdManager : MonoBehaviour {
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] MainmenuManager gui;
    [SerializeField] GameObject loadingPanel;

    [Header("Revive Buttons (Rewarded)")]
    [SerializeField] private List<Button> reviveButtons = new List<Button>();

    [Header("Retry Buttons (Interstitial)")]
    [SerializeField] private List<Button> restartButtons = new List<Button>();
    [SerializeField] List<TextMeshProUGUI> limitTxtUI = new List<TextMeshProUGUI>();


    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;
    private bool isSdkInitialized = false;
    private AdTarget adTarget = AdTarget.None;

    private const string APP_KEY = "26b8c37b5";
    private const string REWARD_AD_UNIT_ID = "shsn3zi2t35xoese";
    private const string INTERSTITIAL_AD_UNIT_ID = "6s73h4sri4pvs62u";

    private const string USER_ID_KEY = "userId";
    private const string REWARDED_LIMIT_KEY = "DailyRewardedCount";
    private const string LAST_AD_DATE_KEY = "LastAdDate";
    private const int MAX_DAILY_REWARDED = 22;


    void Start() {
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        LevelPlay.SetMetaData("is_test_suite", "enable");
        LevelPlay.Init(APP_KEY, GetUserID());

        InitButtons();
    }


    void InitButtons() {
        foreach (Button b in reviveButtons) {
            if (b != null)
                b.onClick.AddListener(OnReviveClicked);
        }

        foreach (Button b in restartButtons) {
            if (b != null)
                b.onClick.AddListener(OnRetryClicked);
        }

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

        // check premium\founders
        if (IsPlayerHasAddImmunitet().premium) {
            Debug.Log("The player has a premium account! We'll revive it for free.");
            RewardedRewardBeh();
            return;
        }

        if (!isSdkInitialized) {
            Debug.LogWarning("The ad isn't ready yet; the SDK is initializing.");
            adTarget = AdTarget.None;
            return;
        }

        // check day limit
        if (!CanWatchRewarded()) {
            Debug.Log("The free revival attempts have ended for today!");
            adTarget = AdTarget.None;
            return;
        }

        //check add readiness
        if (rewardedAd != null && rewardedAd.IsAdReady()) {
            SetButtonsInteractable(false);
            rewardedAd.ShowAd();
            rewardedAd.LoadAd();
        }
        else {//try load again if failed load           
            Debug.Log("Rewarded isn't ready. Let's try downloading it...");
            loadingPanel.SetActive(false);
            //gui.ActivateRespawnWnd();
            rewardedAd?.LoadAd();
        }
    }

    public void OnShowInterstitialAd() {//just show ad
        (bool prem, bool noadd) = IsPlayerHasAddImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            return;
        }

        if (interstitialAd != null && interstitialAd.IsAdReady()) {
            interstitialAd.ShowAd();
            interstitialAd.LoadAd();
        }
    }

    void RewardedAdOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward) {
        // recalc limits
        int currentCount = PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, 0);
        PlayerPrefs.SetInt(REWARDED_LIMIT_KEY, currentCount + 1);
        PlayerPrefs.Save();
        SetLimitAdAmountTxt(MAX_DAILY_REWARDED - currentCount);

        RewardedRewardBeh();
    }

    void SetLimitAdAmountTxt(int currentCnt) {
        foreach (TextMeshProUGUI t in limitTxtUI)
            t.text = currentCnt + "/" + MAX_DAILY_REWARDED;
    }

    void OnRewardedAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
    }

    void OnRewardedLoadFailed(LevelPlayAdError error) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        Debug.LogWarning($"Rewarded loading error: {error}");
    }

    private void OnRetryClicked() {//revive -> interstitial
        adTarget = AdTarget.JustRespawn;

        (bool prem, bool noadd) = IsPlayerHasAddImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            InterstitialRewardBeh();
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
            InterstitialRewardBeh();
        }
    }

    void OnInterstitialAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        interstitialAd.LoadAd(); //load immediately load on backgr new ad
        InterstitialRewardBeh();
    }

    void OnInterstitialLoadFailed(LevelPlayAdError error) {
        SetButtonsInteractable(true);
        loadingPanel.SetActive(false);
        Debug.LogWarning($"Loading error Interstitial: {error}");
    }


    /// <summary>
    /// premium - delete interstitial and rewardeed ads, noAds - only del interstitial
    /// </summary>
    /// <returns></returns>
    private (bool premium, bool noAds) IsPlayerHasAddImmunitet() {
        if (ProgressSaver.IsDataExist()) {
            SaveData_Player data = ProgressSaver.Load();
            if (data != null)
                return (data.isPremiumUser, data.isNoAD);
        }
        return (false, false);
    }

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
    }

    private void RewardedRewardBeh() {
        if (adTarget == AdTarget.ExtraLife) {
            Debug.Log("RevivePlayerLogic: A fish comes back to life at the very spot where it died, with the same level!");
            gui.DeactivateRespawnWnd();
            spawnManager.RespawnPlayer(true, true);
            
            adTarget = AdTarget.None;
        }
    }

    private void InterstitialRewardBeh() {
        if (adTarget == AdTarget.JustRespawn) {
            gui.DeactivateRespawnWnd();
            spawnManager.RespawnPlayer(true, false);
            Debug.Log("RestartGameLogic: The fish falls off the platform and the game restarts.");
            
            adTarget = AdTarget.None;
        }
    }

    private void OnDestroy() {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }
}