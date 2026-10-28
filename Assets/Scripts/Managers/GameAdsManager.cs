//#define UNITY_ANDROID

using System;
using UnityEngine;
using Unity.Services.LevelPlay;
using System.Threading.Tasks;
using VContainer;
using VContainer.Unity;


public enum AdTarget {
    None,
    JustRespawn,
    ExtraLife,
    DoubleShells,
    ExtraShells,
    BoostResearch
}

public interface IAdsManager {
    #if UNITY_ANDROID
    event Action<bool> OnAdButtonsInteractable;
    event Action OnDoubleShellsRewarded;
    event Action OnExtraShellsRewarded;
    event Action OnReviveRewarded;

    bool IsAdStartShow { get; }
    void OnReviveClicked();
    void OnDoubleShellsClick();
    void OnClaimExtraShellsClick();
    void OnBoostClicked(BiomeType type);
    bool OnShowInterstitialAd();
    void OnRetryClicked();
#endif
}

public class GameAdsManager : MonoBehaviour, IAdsManager {
    public static GameAdsManager Instance { get; private set; }

    //self inject
    [Inject] private readonly IPlayerFishPoolService _playerFishPoolService;
    [Inject] private readonly IAlertWindowsManager _alertWindows;
    //[Inject] private readonly IGameHudController _gameHudController;

    public bool IsAdStartShow { get; private set; } = false;
#if UNITY_ANDROID
    public event Action<bool> OnAdButtonsInteractable;
    public event Action OnDoubleShellsRewarded;
    public event Action OnExtraShellsRewarded;
    public event Action OnReviveRewarded;

    private LevelPlayRewardedAd _rewardedAd;
    private bool _isSdkInitialized = false;

    private LevelPlayInterstitialAd _interstitialAd;
    private AdTarget _adTarget = AdTarget.None;
    private BiomeType _boostResearch;
    private float _timeAfterInterstetial = 0f;
    private bool _isRewardEarned = false;

    private const string APP_KEY = "26b8c37b5";
    private const string REWARD_AD_UNIT_ID = "shsn3zi2t35xoese";
    private const string INTERSTITIAL_AD_UNIT_ID = "6s73h4sri4pvs62u";
    private const string USER_ID_KEY = "userId";
    private const float INTERSTETIAL_TIME_INTERVAL = 2 * 60;//m*s (m)
    private const float INTERSETIAL_DELAY_RELOAD = 10f;


    private void Awake() {
        if (Instance != null && Instance != this) {
            //Instance.ReInitGameUI(/*spawnManager,*/ gui);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //void ReInitGameUI(SpawnManager spawnManager, MainmenuManager gui) {
    //    this.spawnManager = this.spawnManager == null ? spawnManager : this.spawnManager;
    //    this.gui = this.gui == null ? gui : this.gui;
    //    OnAdButtonsInteractable?.Invoke(true);
    //}
    //void ReInitGameUI(/*SpawnManager newSpawnManager,*/ MainmenuManager newGui) {
    //    //spawnManager = newSpawnManager;
    //    gui = newGui;

    //    OnAdButtonsInteractable?.Invoke(true);
    //}

    async void Start() {
        SetInjects();

        await Task.Delay(2000);

        try {
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            //LevelPlay.SetMetaData("is_test_suite", "enable");
            LevelPlay.Init(APP_KEY, GetUserID());
        }
        catch (Exception ex) {
            Debug.LogWarning($"[AdInit] LevelPlay init failed safely: {ex.Message}");
        }
    }

    void SetInjects() {
        LifetimeScope scope = FindFirstObjectByType<LifetimeScope>();

        if (scope == null)
            return;

        scope.Container.Inject(this);
    }

    bool IsInterstetialTime => Time.time - _timeAfterInterstetial >= INTERSTETIAL_TIME_INTERVAL;
    void ResetInterstetialTime() => _timeAfterInterstetial = Time.time;

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
        _isSdkInitialized = true;
        Debug.Log("The LevelPlay SDK has been successfully initialized!");
        //LevelPlay.LaunchTestSuite();

        if (_rewardedAd == null) {
            _rewardedAd = new LevelPlayRewardedAd(REWARD_AD_UNIT_ID);
            _rewardedAd.OnAdLoaded += (adInfo) => Debug.Log("Rewarded loaded");
            _rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            _rewardedAd.OnAdRewarded += RewardedAdOnRewardEvent;
            _rewardedAd.OnAdClosed += OnRewardedAdClosed;

            _rewardedAd.LoadAd();
        }

        if (_interstitialAd == null) {
            _interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_UNIT_ID);
            _interstitialAd.OnAdLoaded += (adInfo) => Debug.Log("Interstitial loaded");
            _interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            _interstitialAd.OnAdClosed += OnInterstitialAdClosed;

            _interstitialAd.LoadAd();//load in backround
        }
    }

    void SdkInitializationFailedEvent(LevelPlayInitError le) {
        _isSdkInitialized = false;
        Debug.LogError($"LevelPlay initialization error: {le}");
    }
#endif

    public void OnReviveClicked() {//rewarded
#if UNITY_ANDROID
        _adTarget = AdTarget.ExtraLife;
        IsAdStartShow = true;
        _alertWindows.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        // check premium\founders
        if (ProgressSaver.IsPlayerHasAdImmunitet().premium) {
            Debug.Log("The player has a premium account! We'll revive it for free.");
            RewardedRewardBeh();
            ResetState();
            return;
        }

        RewardedCheckAndShow();
#endif
    }

    public void OnDoubleShellsClick() {//rewarded
#if UNITY_ANDROID
        _adTarget = AdTarget.DoubleShells;
        RewardedCheckAndShow();
#endif
    }

    public void OnClaimExtraShellsClick() {
#if UNITY_ANDROID
        _adTarget = AdTarget.ExtraShells;
        RewardedCheckAndShow();
#endif
    }

    public void OnBoostClicked(BiomeType type) {
#if UNITY_ANDROID
        _boostResearch = type;
        _adTarget = AdTarget.BoostResearch;
        RewardedCheckAndShow();
#endif
    }

    void RewardedCheckAndShow() {
#if UNITY_ANDROID
        IsAdStartShow = true;
        _alertWindows.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        if (GameManager.IsInternetNotAvailable) {
            _alertWindows.ActivateNoInternetConnectionWnd(true);
            ResetState();
            return;
        }

        if (!_isSdkInitialized) {
            Debug.LogWarning("The ad isn't ready yet; the SDK is initializing.");
            ResetState();
            return;
        }

        //check add readiness
        if (_rewardedAd != null && _rewardedAd.IsAdReady())
            _rewardedAd.ShowAd();
        else {//try load again if failed load   
            Debug.Log("Rewarded isn't ready. Let's try downloading it...");
            _rewardedAd?.LoadAd();
            ResetState();
        }
#endif
    }


    private void ResetState() {
#if UNITY_ANDROID
        IsAdStartShow = false;
        _adTarget = AdTarget.None;
        _alertWindows.ActivateLoadingPanelWnd(false);
        OnAdButtonsInteractable?.Invoke(true);
#endif
    }

    /// <summary>
    /// return true if ad should appear
    /// </summary>
    /// <returns></returns>
    public bool OnShowInterstitialAd() {//just show ad (used in pause menu on exit)
#if UNITY_ANDROID
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
        if (_interstitialAd != null && _interstitialAd.IsAdReady()) {
            _alertWindows.ActivateLoadingPanelWnd(true);
            _interstitialAd.ShowAd();
            return true;
        }
#endif

        return false;
    }

#if UNITY_ANDROID
    void RewardedAdOnRewardEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward) {
        _isRewardEarned = true;

        if (!IsAdStartShow)
            ProcessEarnedReward();
    }

    void ProcessEarnedReward() {

        if (_isRewardEarned) {
            RewardedRewardBeh();
            _isRewardEarned = false;
        }
    }
#endif

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

#if (UNITY_ANDROID || UNITY_EDITOR) && !UNITY_STANDALONE_WIN
    void OnRewardedAdClosed(LevelPlayAdInfo adInfo) {

        ProcessEarnedReward();
        rewardedAd.LoadAd();


#if UNITY_EDITOR//  in case if OnAdClosed worked first, it reset adTarget to None via ResetState().
        IsAdStartShow = false;
        _alertWindows.ActivateLoadingPanelWnd(false);
        OnAdButtonsInteractable?.Invoke(true);
#else
        ResetState();//!!! necessarily
#endif
    }

    void OnRewardedLoadFailed(LevelPlayAdError error) {
        Debug.LogWarning($"Rewarded loading error: {error}");
    }
#endif

    public void OnRetryClicked() {//restart -> interstitial
#if UNITY_ANDROID
        _adTarget = AdTarget.JustRespawn;

        if (IsInterstetialTime)
            ResetInterstetialTime();
        else {
            InterstitialRespawnBeh();
            return;
        }

        _alertWindows.ActivateLoadingPanelWnd(true);
        OnAdButtonsInteractable?.Invoke(false);

        (bool prem, bool noadd) = ProgressSaver.IsPlayerHasAdImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            InterstitialRespawnBeh();
            ResetState();
            return;
        }

        if (_interstitialAd != null && _interstitialAd.IsAdReady())
            _interstitialAd.ShowAd();
        else {
            //ad not ready (haha classic lucky u - bum i
            Debug.Log("The interstitial isn't ready. Just restart the game.");
            InterstitialRespawnBeh();
            ResetState();
        }
#endif
    }

#if UNITY_ANDROID
    void OnInterstitialAdClosed(LevelPlayAdInfo adInfo) {
        _interstitialAd.LoadAd(); //load immediately load on backgr new ad
        InterstitialRespawnBeh();

        ResetState();
    }

    void OnInterstitialLoadFailed(LevelPlayAdError error) {
        Debug.LogWarning($"Loading error Interstitial: {error}");
        //Try to load again (preferably add a delay via Invoke or Coroutine for 5-10 seconds to avoid spamming the server)
        Invoke(nameof(RetryLoadInterstitial), INTERSETIAL_DELAY_RELOAD);
    }

    void RetryLoadInterstitial() {
        if (_interstitialAd != null && !_interstitialAd.IsAdReady())
            _interstitialAd.LoadAd();
    }

    private void RewardedRewardBeh() {
        if (_adTarget == AdTarget.ExtraLife) {
            Debug.Log("RevivePlayerLogic: A fish comes back to life at the very spot where it died, with the same level!");
            //_gameHudController.DeactivateRespawnWnd();//solve by playerServis respawn event
            _playerFishPoolService.RespawnPlayer(true, true);
            OnReviveRewarded?.Invoke();
        }
        else if (_adTarget == AdTarget.DoubleShells)
            OnDoubleShellsRewarded?.Invoke();

        else if (_adTarget == AdTarget.ExtraShells) {
            if (SanctuariesManager.Instance != null)
                SanctuariesManager.Instance.OnAddShells(GameStaticInfo.EXTRA_SHELLS_CNT);
            OnExtraShellsRewarded?.Invoke();
        }
        else if (_adTarget == AdTarget.BoostResearch) {
            if (SanctuariesManager.Instance != null)
                SanctuariesManager.Instance.BoostResearch(_boostResearch);
        }

        _adTarget = AdTarget.None;
    }

    private void InterstitialRespawnBeh() {
        if (_adTarget == AdTarget.JustRespawn) {
            //_gameHudController.DeactivateRespawnWnd(); //solve by playerServis respawn event
            _playerFishPoolService.RespawnPlayer(true, false);
            //Debug.Log("RestartGameLogic: The fish falls off the platform and the game restarts.");

            _adTarget = AdTarget.None;
        }
    }

    private void OnDestroy() {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;

        if (Instance == this)
            Instance = null;
    }
#endif
}