using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;



public class GamePurchaseManager : MonoBehaviour {
    public static GamePurchaseManager Instance { get; private set; }
    /// <summary>
    /// no ads, premium, founders
    /// </summary>
    public event Action<AccountTitles> OnUpdateAccountTittles;
    public event Action<PurchasedSkins> OnUpdatePurchasedSkins;

    [SerializeField] GPG_Manager GPGmanager;
    [SerializeField] AccountTitles ac;
    //[SerializeField] PurchaseUIvisualizer purchaseUIvisualizer;

    private StoreController m_StoreController;
    //private AccountTitles accountTitles;
    //private PurchasedSkins purchasedSkins;
    private const string PREMIUM_PRODUCT_ID = "com.deepjaws.premium_no_ads";
    private const string NOADD_PRODUCT_ID = "com.deepjaws.remove_interstitials";
    private const string ASHFLOW_PRODUCT_ID = "com.deepjaws.ashflow";
    private const string CHAMELEON_PRODUCT_ID = "com.deepjaws.chameleon";
    private const string MERCURY_PRODUCT_ID = "com.deepjaws.mercury";


    private void Awake() {
        if (Instance != null && Instance != this) {
            Instance.ReInit(GPGmanager, /*purchaseUIvisualizer, */m_StoreController);
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        //local fast check on start without internet
        CheckPurchasesLocally();
    }

    void ReInit(GPG_Manager GPGmanager, /*PurchaseUIvisualizer purchaseUIvisualizer,*/ StoreController m_StoreController) {
        this.GPGmanager = this.GPGmanager == null ? GPGmanager : this.GPGmanager;
        //this.purchaseUIvisualizer = this.purchaseUIvisualizer == null ? purchaseUIvisualizer : this.purchaseUIvisualizer;
        this.m_StoreController = this.m_StoreController == null ? m_StoreController : this.m_StoreController;
    }

    async void Start() {
        Debug.Log("IAP v5: Start of initialization...");

        m_StoreController = UnityIAPServices.StoreController();

        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnProductsFetched += OnProductsFetched;
        m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
        m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;
        m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

        try {
            await m_StoreController.Connect();
            Debug.Log("IAP v5: Connected to the store. Retrieving products...");

            var initialProductsToFetch = new List<ProductDefinition>
            {
                new(PREMIUM_PRODUCT_ID, ProductType.NonConsumable),
                new(NOADD_PRODUCT_ID, ProductType.NonConsumable),
                new(Application.identifier, ProductType.NonConsumable)
            };

            m_StoreController.FetchProducts(initialProductsToFetch);
        }
        catch (Exception e) {
            Debug.LogError($"IAP v5: Offline mode or network error: {e.Message}");
        }
    }

    private void CheckPurchasesLocally() {
        if (ProgressSaver.IsDataExist()) {
            SaveData_Player currentData = ProgressSaver.Load();
            if (currentData != null) {
                AccountTitles accountTitles = new();
                PurchasedSkins purchasedSkins = new();

                // player has immunited to ad purchases 
                if (currentData.isPremiumUser || currentData.isNoAD) {
                    Debug.Log("Local check: Paid status found in the save file! Disabling interstitial ads.");
                    ApplyNoAdsLogic(true);                    
                }
                accountTitles.HasPremium = currentData.isPremiumUser;
                accountTitles.HasNoAd = currentData.isNoAD;
                accountTitles.HasFounder = currentData.isFounder;
                purchasedSkins.Mercury=currentData.hasMercurySkin;
                purchasedSkins.Chameleon = currentData.hasChameleon;
                purchasedSkins.Ashflow = currentData.hasAshflowSkin;
                purchasedSkins.Selected = currentData.selectedSkin;

                OnUpdateAccountTittles?.Invoke(accountTitles);
                OnUpdatePurchasedSkins?.Invoke(purchasedSkins);
                return;
            }
        }

        Debug.Log("Local check: No active ad purchases were found in the cache.");
        ApplyNoAdsLogic(false);
    }

    // update GP purchases history
    private void OnPurchasesFetched(Orders orders) {
        AccountTitles accountTitles = new();
        PurchasedSkins purchasedSkins = new();
        //bool hasPremium = false;
        //bool hasFounder = false;
        //bool hasNoAd = false;

        if (orders.ConfirmedOrders != null) {
            //full premium
            if (IsHasProduct(orders, PREMIUM_PRODUCT_ID))
                accountTitles.HasPremium = true;

            //only interstitial remove
            if (IsHasProduct(orders, NOADD_PRODUCT_ID))
                accountTitles.HasNoAd = true;

            //old founder check
            if (IsHasProduct(orders, Application.identifier)) {
                accountTitles.HasPremium = true; //old buyers get all prem
                accountTitles.HasFounder = true;

                GPGmanager.OnAddToFoundersLeaderboard();
            }

            if (IsHasProduct(orders, ASHFLOW_PRODUCT_ID))
                purchasedSkins.Ashflow = true;

            if (IsHasProduct(orders, MERCURY_PRODUCT_ID))
                purchasedSkins.Mercury = true;

            if(IsHasProduct(orders,CHAMELEON_PRODUCT_ID))
                purchasedSkins.Chameleon = true; 
        }

        UpdatePremiumInSaveFile(accountTitles);
        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins);
        //purchaseUIvisualizer.InitAccountTittles(hasNoAd, hasPremium, hasFounder);
    }

    bool IsHasProduct(Orders orders, string PRODUCT_ID) {
        return orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                     order.Info.PurchasedProductInfo != null &&
                                                     order.Info.PurchasedProductInfo.Any(p => p.productId == PRODUCT_ID));
    }

    // save purchase to ProgressSaver
    private void UpdatePremiumInSaveFile(AccountTitles accountTitles) {
        SaveData_Player currentData = ProgressSaver.IsDataExist() ?
            ProgressSaver.Load() : new SaveData_Player();

        if (currentData != null) {
            currentData.isPremiumUser = accountTitles.HasPremium;
            currentData.isNoAD = accountTitles.HasNoAd;
            currentData.isFounder = accountTitles.HasFounder;

            ProgressSaver.Save(currentData);
            Debug.Log($"IAP v5: Purchase status has been successfully synced with Google Play and saved! Premium: {accountTitles.HasPremium}");

            ApplyNoAdsLogic(accountTitles.HasPremium || accountTitles.HasNoAd);//enab\disab ad in case
        }
    }

    private void ApplyNoAdsLogic(bool disableAds) {
        if (disableAds) 
            Debug.Log("ADVERTISEMENTS ARE DISABLED FOR THIS SESSION");
        else 
            Debug.Log("ADVERTISEMENT ACTIVE");
    }

    private void OnProductsFetched(List<Product> products) {
        Debug.Log("IAP v5: Items received. Checking purchase history...");
        m_StoreController.FetchPurchases();
    }

    // check purchased prods during start

    // PROCESSING AN INCOMPLETE PURCHASE
    private void OnPurchasePending(PendingOrder pendingOrder) {
        if (pendingOrder.Info != null && pendingOrder.Info.PurchasedProductInfo != null && pendingOrder.Info.PurchasedProductInfo.Count > 0) {
            string productId = pendingOrder.Info.PurchasedProductInfo[0].productId; //
            Debug.Log($"IAP v5: Transaction processing for a product: {productId}");

            SaveData_Player currentData = ProgressSaver.IsDataExist() ? ProgressSaver.Load() : new SaveData_Player();

            if (currentData != null) {
                bool shouldConfirm = false;

                if (string.Equals(productId, PREMIUM_PRODUCT_ID, StringComparison.Ordinal)) {
                    currentData.isPremiumUser = true;
                    shouldConfirm = true;
                }
                else if (string.Equals(productId, NOADD_PRODUCT_ID, StringComparison.Ordinal)) {
                    currentData.isNoAD = true;
                    shouldConfirm = true;
                }

                if (shouldConfirm) {
                    ProgressSaver.Save(currentData);
                    ApplyNoAdsLogic(currentData.isPremiumUser || currentData.isNoAD);

                    // confirm purch in Google Play
                    m_StoreController.ConfirmPurchase(pendingOrder);
                    Debug.Log($"IAP v5: The purchase of {productId} has been successfully confirmed and saved.");
                }
            }
        }
    }

    public void BuyPremium() {
        if (m_StoreController != null) {
            Debug.Log($"IAP v5: Start the purchase process for {PREMIUM_PRODUCT_ID}");
            m_StoreController.PurchaseProduct(PREMIUM_PRODUCT_ID);
        }
        else {
            Debug.LogError("IAP v5: The store controller isn't ready yet!");
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failedReason) {
        Debug.LogError($"IAP v5: Unable to load purchases. Reason: {failedReason}");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failedReason) {
        Debug.LogError($"IAP v5: Unable to load the order. Description: {failedReason.message}");
    }
}