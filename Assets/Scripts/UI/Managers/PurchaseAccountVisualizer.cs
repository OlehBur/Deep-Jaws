#define UNITY_ANDROID 
#undef UNITY_EDITOR 

using UnityEngine;
using VContainer;



public class PurchaseAccountVisualizer : MonoBehaviour {
    [SerializeField] GameObject loyaltyRewardedWnd;
    [SerializeField] Animator titleAdFree;
    [SerializeField] Animator titlePremium;
    [SerializeField] Animator titleFounder;
    [SerializeField] Animator titleDemo;

    [Inject] private readonly IGamePurchaseManager _gamePurchaseManager;
    [Inject] private readonly IAlertWindowsManager _alertWindowsManager;



    private void Start() {
#if UNITY_STANDALONE_WIN                     
        titleAdFree.gameObject.SetActive(false);
        titlePremium.gameObject.SetActive(false);
        titleFounder.gameObject.SetActive(false);
        titleDemo.gameObject.SetActive(GameManager.IS_PC_DEMO);

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;
        titleDemo.enabled = GameManager.IS_PC_DEMO;

#else
        /*if UNITY_ANDROID && !UNITY_EDITOR*/
        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdateAccountTittles -= InitMainMenuAccountTittles;
            _gamePurchaseManager.OnNonInternetConnection -= PurchaseManagerNoInternetHandler;

            _gamePurchaseManager.OnUpdateAccountTittles += InitMainMenuAccountTittles;
            _gamePurchaseManager.OnNonInternetConnection += PurchaseManagerNoInternetHandler;
            LocalLoadTitlesAndSkins();
        }

        _gamePurchaseManager?.RefreshUI();
#endif

    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnDestroy() {
        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdateAccountTittles -= InitMainMenuAccountTittles;
            _gamePurchaseManager.OnNonInternetConnection -= PurchaseManagerNoInternetHandler;
        }
    }

    void PurchaseManagerNoInternetHandler() => _alertWindowsManager.ActivateNoInternetConnectionWnd(true);

#if !UNITY_STANDALONE_WIN
    void LocalLoadTitlesAndSkins() {
        AccountTitles at = _gamePurchaseManager.GetCurrentAccountTitles();

        InitMainMenuAccountTittles(at);
    }
#endif

    void OpenLoyaltyRewardedWnd() {
        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        if (currentData != null && !currentData.confirmedLoyalty) {
            loyaltyRewardedWnd.SetActive(true);
            currentData.confirmedLoyalty = true;

            ProgressSaver.SaveData(currentData);
        }
    }

    private void InitMainMenuAccountTittles(AccountTitles at) {
        if (titleAdFree == null || titlePremium == null || titleFounder == null)
            return;

        titleAdFree.gameObject.SetActive(false);
        titlePremium.gameObject.SetActive(false);
        titleFounder.gameObject.SetActive(false);
        titleDemo.gameObject.SetActive(false);

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;
        titleDemo.enabled = false;

        if (at.HasFounder) {
            titleFounder.gameObject.SetActive(true);
            titleFounder.enabled = true;
            OpenLoyaltyRewardedWnd();
        }
        else if (at.HasPremium) {
            titlePremium.gameObject.SetActive(true);
            titlePremium.enabled = true;
        }
        else if (at.HasNoAd) {
            titleAdFree.gameObject.SetActive(true);
            titleAdFree.enabled = true;
        }
    }
#endif
}
