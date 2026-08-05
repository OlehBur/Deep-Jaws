using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;
using TMPro;


public enum AdTarget {
    None,
    JustRespawn,
    ExtraLife,
    DoubleShells,
    ExtraShells,
    BoostResearch
}

public class GameAdsManager : MonoBehaviour {
    public static GameAdsManager Instance { get; private set; }
    public event Action<bool> OnAdButtonsInteractable;

    [SerializeField] SpawnManager spawnManager;
    [SerializeField] MainmenuManager gui;

    public bool IsAdStartShow { get; private set; } = false;
    public event Action OnDoubleShellsRewarded;
    public event Action OnReviveRewarded;

    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;
    private bool isSdkInitialized = false;
    private AdTarget adTarget = AdTarget.None;
    private BiomeType boostResearch;
    private float timeAfterInterstetial = 0f;
    private bool isRewardEarned = false;

    private const string APP_KEY = "26b8c37b5";
    private const string REWARD_AD_UNIT_ID = "shsn3zi2t35xoese";
    private const string INTERSTITIAL_AD_UNIT_ID = "6s73h4sri4pvs62u";
    private const string USER_ID_KEY = "userId";
    private const float INTERSTETIAL_TIME_INTERVAL = 2 * 60;//m*s (m)
    private const float INTERSETIAL_DELAY_RELOAD = 10f;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Instance.ReInitGameUI(spawnManager, gui);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void ReInitGameUI(SpawnManager spawnManager, MainmenuManager gui) {
        this.spawnManager = this.spawnManager == null ? spawnManager : this.spawnManager;
        this.gui = this.gui == null ? gui : this.gui;
        OnAdButtonsInteractable?.Invoke(true);
    }

    void Start() {
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        //LevelPlay.SetMetaData("is_test_suite", "enable");
        LevelPlay.Init(APP_KEY, GetUserID());
    }

    bool IsInterstetialTime => Time.time - timeAfterInterstetial >= INTERSTETIAL_TIME_INTERVAL;
    void ResetInterstetialTime() => timeAfterInterstetial = Time.time;

    //bool IsListNeedToCopy<T>(List<T> currentList, List<T> newList) =>
    //     (currentList == null || currentList.Count == 0 || currentList[0] == null)
    //        && newList != null && newList.Count > 0;

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
        //LevelPlay.LaunchTestSuite();

        if (rewardedAd == null) {
            rewardedAd = new LevelPlayRewardedAd(REWARD_AD_UNIT_ID);
            rewardedAd.OnAdLoaded += (adInfo) => Debug.Log("Rewarded loaded");
            rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            rewardedAd.OnAdRewarded += RewardedAdOnRewardEvent;
            rewardedAd.OnAdClosed += OnRewardedAdClosed;

            rewardedAd.LoadAd();
        }

        if (interstitialAd == null) {
            interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_UNIT_ID);
            interstitialAd.OnAdLoaded += (adInfo) => Debug.Log("Interstitial loaded");
            interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            interstitialAd.OnAdClosed += OnInterstitialAdClosed;

            interstitialAd.LoadAd();//load in backround
        }
    }

    void SdkInitializationFailedEvent(LevelPlayInitError le) {
        isSdkInitialized = false;
        Debug.LogError($"LevelPlay initialization error: {le}");
    }

    public void OnReviveClicked() {//rewarded
        adTarget = AdTarget.ExtraLife;
        IsAdStartShow = true;
        gui.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        // check premium\founders
        if (ProgressSaver.IsPlayerHasAdImmunitet().premium) {
            Debug.Log("The player has a premium account! We'll revive it for free.");
            RewardedRewardBeh();
            ResetState();
            return;
        }

        RewardedCheckAndShow();
    }

    public void OnDoubleShellsClicked() {//rewarded
        adTarget = AdTarget.DoubleShells;
        RewardedCheckAndShow();
    }

    public void OnBoostClicked(BiomeType type) {
        boostResearch = type;
        adTarget = AdTarget.BoostResearch;
        RewardedCheckAndShow();
    }

    void RewardedCheckAndShow() {
        IsAdStartShow = true;
        gui.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        if (GameManager.IsInternetNotAvailable) {
            gui.ActivateNoInternetConnectionWnd(true);
            ResetState();
            return;
        }

        if (!isSdkInitialized) {
            Debug.LogWarning("The ad isn't ready yet; the SDK is initializing.");
            ResetState();
            return;
        }

        //check add readiness
        if (rewardedAd != null && rewardedAd.IsAdReady())
            rewardedAd.ShowAd();
        else {//try load again if failed load   
            Debug.Log("Rewarded isn't ready. Let's try downloading it...");
            rewardedAd?.LoadAd();
            ResetState();
        }
    }


    private void ResetState() {
        IsAdStartShow = false;
        adTarget = AdTarget.None;
        gui.ActivateLoadingPanelWnd(false);
        OnAdButtonsInteractable?.Invoke(true);
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
            gui.ActivateLoadingPanelWnd(true);
            interstitialAd.ShowAd();
            return true;
        }

        return false;
    }

    void RewardedAdOnRewardEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward) {
        isRewardEarned = true;

        if (!IsAdStartShow)
            ProcessEarnedReward();
    }

    void ProcessEarnedReward() {
        if (isRewardEarned) {
            RewardedRewardBeh();
            isRewardEarned = false;
        }
    }

    //true if limit hsnt exhausted
    //bool RewardedLimitRecalc() {
    //    int loadedAttemptsCnt = PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, MAX_DAILY_REWARDED);
    //    if (loadedAttemptsCnt != 0) {
    //        loadedAttemptsCnt--;
    //        PlayerPrefs.SetInt(REWARDED_LIMIT_KEY, loadedAttemptsCnt);
    //        PlayerPrefs.Save();
    //        return true;
    //    }
    //    else //loadedAttemptsCnt == 0
    //        return false;
    //    //PlayerPrefs.Save();
    //    //SetLimitAdAmountTxt(/*MAX_DAILY_REWARDED -*/ loadedAttemptsCnt);
    //}

    //void SetLimitAdAmountTxt(int currentCnt) {
    //    //foreach (TextMeshProUGUI t in limitTxtUI)
    //    limitTxtUI.text = currentCnt + "/" + MAX_DAILY_REWARDED;
    //}

    //public void UpdateDailyRevivesTxt() {
    //    int attemptsLeft = /*MAX_DAILY_REWARDED - */PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, MAX_DAILY_REWARDED);
    //    limitRevivesTxtUI.text = attemptsLeft + "/" + MAX_DAILY_REWARDED;
    //    //SetLimitAdAmountTxt(attemptsLeft);
    //}

    void OnRewardedAdClosed(LevelPlayAdInfo adInfo) {
        ProcessEarnedReward();

        rewardedAd.LoadAd();
#if UNITY_EDITOR//  in case if OnAdClosed worked first, it reset adTarget to None via ResetState().
        IsAdStartShow = false;
        gui.ActivateLoadingPanelWnd(false);
        OnAdButtonsInteractable?.Invoke(true);
#else
        ResetState();//!!! necessarily
#endif
    }

    void OnRewardedLoadFailed(LevelPlayAdError error) {
        Debug.LogWarning($"Rewarded loading error: {error}");
    }

    public void OnRetryClicked() {//restart -> interstitial
        adTarget = AdTarget.JustRespawn;

        if (IsInterstetialTime)
            ResetInterstetialTime();
        else {
            InterstitialRespawnBeh();
            return;
        }

        gui.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            InterstitialRespawnBeh();
            ResetState();
            return;
        }

        if (interstitialAd != null && interstitialAd.IsAdReady())
            interstitialAd.ShowAd();
        else {
            //ad not ready (haha classic lucky u - bum i
            Debug.Log("The interstitial isn't ready. Just restart the game.");
            InterstitialRespawnBeh();
            ResetState();
        }
    }

    void OnInterstitialAdClosed(LevelPlayAdInfo adInfo) {
        interstitialAd.LoadAd(); //load immediately load on backgr new ad
        InterstitialRespawnBeh();

        ResetState();
    }

    void OnInterstitialLoadFailed(LevelPlayAdError error) {
        Debug.LogWarning($"Loading error Interstitial: {error}");
        //Try to load again (preferably add a delay via Invoke or Coroutine for 5-10 seconds to avoid spamming the server)
        Invoke(nameof(RetryLoadInterstitial), INTERSETIAL_DELAY_RELOAD);
    }

    void RetryLoadInterstitial() {
        if (interstitialAd != null && !interstitialAd.IsAdReady())
            interstitialAd.LoadAd();
    }

    private void RewardedRewardBeh() {
        if (adTarget == AdTarget.ExtraLife) {
            Debug.Log("RevivePlayerLogic: A fish comes back to life at the very spot where it died, with the same level!");
            gui.DeactivateRespawnWnd();
            spawnManager.RespawnPlayer(true, true);
            OnReviveRewarded?.Invoke();
        }
        else if (adTarget == AdTarget.DoubleShells)
            OnDoubleShellsRewarded?.Invoke();
        else if (adTarget == AdTarget.BoostResearch)
            if (SanctuariesManager.Instance != null)
                SanctuariesManager.Instance.BoostResearch(boostResearch);

        adTarget = AdTarget.None;
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