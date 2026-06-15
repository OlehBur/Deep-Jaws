using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.Purchasing;

public class GamePurchaseManager : MonoBehaviour {
    [SerializeField] GPG_Manager GPGmanager;

    private StoreController m_StoreController;

    private const string PREMIUM_PRODUCT_ID = "com.deepjaws.premium_no_ads";
    private const string NOADD_PRODUCT_ID = "com.deepjaws.remove_interstitials";


    private void Awake() {
        //local fast check on start without internet
        CheckPremiumLocally();
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

    // МИТТЄВА ПЕРЕВІРКА ПРИ ЗАХОДІ
    //private void CheckPremiumLocally() {
    //    if (ProgressSaver.IsDataExist()) {
    //        SaveData_Player currentData = ProgressSaver.Load();
    //        if (currentData != null && currentData.isPremiumUser) {
    //            Debug.Log("Локальна перевірка: Преміум знайдено в зашифрованому сейві! Вимикаємо рекламу.");
    //            ApplyNoAdsLogic(true);
    //            return;
    //        }
    //    }

    //    Debug.Log("Локальна перевірка: Преміум в сейві не активний.");
    //    ApplyNoAdsLogic(false);
    //}
    private void CheckPremiumLocally() {
        if (ProgressSaver.IsDataExist()) {
            SaveData_Player currentData = ProgressSaver.Load();
            if (currentData != null) {
                // player has immunited to ad purchases 
                if (currentData.isPremiumUser || currentData.isNoAD) {
                    Debug.Log("Local check: Paid status found in the save file! Disabling interstitial ads.");
                    ApplyNoAdsLogic(true);
                    return;
                }
            }
        }

        Debug.Log("Local check: No active ad purchases were found in the cache.");
        ApplyNoAdsLogic(false);
    }

    // update GP purchases history
    private void OnPurchasesFetched(Orders orders) {
        bool hasPremium = false;
        bool hasFounder = false;
        bool hasNoAd = false;

        if (orders.ConfirmedOrders != null) {//full premium
            if (orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                    order.Info.PurchasedProductInfo != null &&
                                                    order.Info.PurchasedProductInfo.Any(p => p.productId == PREMIUM_PRODUCT_ID))) {
                hasPremium = true;
            }//only interstitial remove
            if (orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                    order.Info.PurchasedProductInfo != null &&
                                                    order.Info.PurchasedProductInfo.Any(p => p.productId == NOADD_PRODUCT_ID))) {
                hasNoAd = true;
            }

            //old founder check
            if (orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                    order.Info.PurchasedProductInfo != null &&
                                                    order.Info.PurchasedProductInfo.Any(p => p.productId == Application.identifier))) {
                hasPremium = true; //old buyers get all prem
                hasFounder = true;

                GPGmanager.OnAddToFoundersLeaderboard();
            }
        }
        UpdatePremiumInSaveFile(hasPremium, hasNoAd ,hasFounder);
    }

    // save purchase to ProgressSaver
    private void UpdatePremiumInSaveFile(bool isPremium, bool isNoAd, bool isFounder) {
        SaveData_Player currentData = ProgressSaver.IsDataExist() ?
            ProgressSaver.Load() : new SaveData_Player();

        if (currentData != null) {
            currentData.isPremiumUser = isPremium;
            currentData.isNoAD = isNoAd;
            currentData.isFounder = isFounder;

            ProgressSaver.Save(currentData);
            Debug.Log($"IAP v5: Purchase status has been successfully synced with Google Play and saved! Premium: {isPremium}");

            ApplyNoAdsLogic(isPremium || isNoAd);//enab\disab ad in case
        }
    }

    private void ApplyNoAdsLogic(bool disableAds) {
        if (disableAds) {
            Debug.Log("ADVERTISEMENTS ARE DISABLED FOR THIS SESSION");
        }
        else {
            Debug.Log("ADVERTISEMENT ACTIVE");
        }
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
        //if (pendingOrder.Info != null && pendingOrder.Info.PurchasedProductInfo != null && pendingOrder.Info.PurchasedProductInfo.Count > 0) {
        //    string productId = pendingOrder.Info.PurchasedProductInfo[0].productId;
        //    Debug.Log($"IAP v5: Обробка незавершеної покупки для продукту: {productId}");

        //    if (string.Equals(productId, PREMIUM_PRODUCT_ID, StringComparison.Ordinal)) {
        //        ActivateNoAdsStatus();
        //        m_StoreController.ConfirmPurchase(pendingOrder);
        //    }
        //}
    }

    //private void ActivateNoAdsStatus() {
    //    Debug.Log("IAP v5: Статус БЕЗ РЕКЛАМИ активовано!");
    //    PlayerPrefs.SetInt("IsPremiumUser", 1);
    //    PlayerPrefs.Save();
    //}

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