using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;
using TMPro;

public class FishAdManager : MonoBehaviour {
    private const string APP_KEY = "26b8c37b5";
    private const string REWARD_AD_UNIT_ID = "shsn3zi2t35xoese";
    private const string INTERSTITIAL_AD_UNIT_ID = "6s73h4sri4pvs62u";

    private const string USER_ID_KEY = "userId";
    private const string REWARDED_LIMIT_KEY = "DailyRewardedCount";
    private const string LAST_AD_DATE_KEY = "LastAdDate";
    private const int MAX_DAILY_REWARDED = 22;

    [Header("Revive Buttons (Rewarded)")]
    [SerializeField] private List<Button> reviveButtons = new List<Button>();

    [Header("Retry Buttons (Interstitial)")]
    [SerializeField] private List<Button> restartButtons = new List<Button>();
    [SerializeField] List<TextMeshProUGUI> limitTxtUI = new List<TextMeshProUGUI>();

    // Об'єкти реклами LevelPlay
    private LevelPlayRewardedAd rewardedAd;
    private LevelPlayInterstitialAd interstitialAd;

    private bool isSdkInitialized = false;

    void Start() {
        // 1. Підписуємося на ініціалізацію SDK
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        // Вмикаємо тестовий режим, якщо треба перевірити інтеграцію
        LevelPlay.SetMetaData("is_test_suite", "enable");

        // Запускаємо LevelPlay
        LevelPlay.Init(APP_KEY, GetUserID());

        // 2. Налаштовуємо кнопки (вішаємо лісенери)
        InitButtons();
    }


    void InitButtons() {
        foreach (Button b in reviveButtons) {
            if (b != null)
                b.onClick.AddListener(OnReviveClicked);
        }

        foreach (Button b in restartButtons) {
            if (b != null)
                b.onClick.AddListener(OnRestartClicked);
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

    // Спрацює, коли LevelPlay успішно запуститься
    void SdkInitializationCompletedEvent(LevelPlayConfiguration lc) {
        isSdkInitialized = true;
        Debug.Log("The LevelPlay SDK has been successfully initialized!");

        // Створюємо об'єкт Rewarded реклами
        if (rewardedAd == null) {
            rewardedAd = new LevelPlayRewardedAd(REWARD_AD_UNIT_ID);
            rewardedAd.OnAdLoaded += (adInfo) => Debug.Log("Rewarded loaded");
            rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            rewardedAd.OnAdRewarded += RewardedAdOnAdRewardedEvent;
            rewardedAd.OnAdClosed += OnRewardedAdClosed;
        }

        // Створюємо об'єкт Interstitial (звичайної) реклами
        if (interstitialAd == null) {
            interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_UNIT_ID);
            interstitialAd.OnAdLoaded += (adInfo) => Debug.Log("Interstitial завантажено");
            interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            interstitialAd.OnAdClosed += OnInterstitialAdClosed;

            // Фоново завантажуємо звичайну рекламу, щоб вона була готова на момент смерті
            interstitialAd.LoadAd();
        }
    }

    void SdkInitializationFailedEvent(LevelPlayInitError le) {
        isSdkInitialized = false;
        Debug.LogError($"LevelPlay initialization error: {le}");
    }

    // ------------------ КЛІК НА КНОПКУ "ОЖИВИТИСЬ" (REWARDED) ------------------
    private void OnReviveClicked() {
        // 1. Перевіряємо зашифрований преміум/меценатів
        if (IsPlayerHasAddImmunitet().premium) {
            Debug.Log("The player has a premium account! We'll revive it for free.");
            RevivePlayerLogic();
            return;
        }

        if (!isSdkInitialized) {
            Debug.LogWarning("The ad isn't ready yet; the SDK is initializing.");
            return;
        }

        // 2. Перевіряємо ліміт на день
        if (!CanWatchRewarded()) {
            Debug.Log("The free revival attempts have ended for today!");
            return;
        }

        // 3. Перевіряємо готовність реклами і показуємо
        if (rewardedAd != null && rewardedAd.IsAdReady()) {
            SetButtonsInteractable(false); // Блокуємо спам-кліки
            rewardedAd.ShowAd();
        }
        else {
            // Якщо реклама не встигла підвантажитися, робимо запит заново
            Debug.Log("Rewarded isn't ready. Let's try downloading it...");
            rewardedAd?.LoadAd();
        }
    }

    // Гравець додивився відео до кінця — даємо нагороду
    void RewardedAdOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward adReward) {
        // Фіксуємо перегляд у лімітах
        int currentCount = PlayerPrefs.GetInt(REWARDED_LIMIT_KEY, 0);
        PlayerPrefs.SetInt(REWARDED_LIMIT_KEY, currentCount + 1);
        PlayerPrefs.Save();
        SetLimitAdAmountTxt(MAX_DAILY_REWARDED - currentCount);

        RevivePlayerLogic();
    }

    void SetLimitAdAmountTxt(int currentCnt) {
        foreach (TextMeshProUGUI t in limitTxtUI)
            t.text = currentCnt + "/" + MAX_DAILY_REWARDED;
    }

    void OnRewardedAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
    }

    void OnRewardedLoadFailed(LevelPlayAdError error) {
        SetButtonsInteractable(true);
        Debug.LogWarning($"Rewarded loading error: {error}");
    }


    // ------------------ КЛІК НА КНОПКУ "ПОЧАТИ ЗАНОВО" (INTERSTITIAL) ------------------
    private void OnRestartClicked() {
        // Якщо преміум — ніякої реклами, одразу пускаємо далі
        (bool prem, bool noadd) = IsPlayerHasAddImmunitet();
        if (prem || noadd) {
            Debug.Log("The Premium player has declined to be revived. Skip the ad.");
            RestartGameLogic();
            return;
        }

        // Перевіряємо чи готова звичайна реклама LevelPlay
        if (interstitialAd != null && interstitialAd.IsAdReady()) {
            SetButtonsInteractable(false);
            interstitialAd.ShowAd();
        }
        else {
            // Якщо реклама не готова або немає інтернету — просто йдемо далі, щоб не ламати геймплей
            Debug.Log("The interstitial isn't ready. Just restart the game.");
            RestartGameLogic();
        }
    }

    void OnInterstitialAdClosed(LevelPlayAdInfo adInfo) {
        SetButtonsInteractable(true);
        interstitialAd.LoadAd(); // Одразу фоново завантажуємо наступну рекламу
        RestartGameLogic();
    }

    void OnInterstitialLoadFailed(LevelPlayAdError error) {
        SetButtonsInteractable(true);
        Debug.LogWarning($"Loading error Interstitial: {error}");
    }


    // ------------------ ДОПОМІЖНІ МЕТОДИ ------------------
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

    private void RevivePlayerLogic() {
        Debug.Log("RevivePlayerLogic: A fish comes back to life at the very spot where it died, with the same level!");
        // Твій код повернення риби у гру
    }

    private void RestartGameLogic() {
        Debug.Log("RestartGameLogic: The fish falls off the platform and the game restarts.");
        // Твій код скидання рівня або перезапуску сцени
    }


    private void OnDestroy() {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }
}