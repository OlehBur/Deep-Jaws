using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Purchasing;



public class GamePurchaseManager : MonoBehaviour {
    public static GamePurchaseManager Instance { get; private set; }
    /// <summary>
    /// no ads, premium, founders
    /// </summary>
    public event Action<AccountTitles> OnUpdateAccountTittles;
    public event Action<PurchasedSkins> OnUpdatePurchasedSkins;

    [SerializeField] GPG_Manager GPGmanager;
    [SerializeField] PurchaseUIvisualizer purchaseUIvisualizer;

    private StoreController m_StoreController;
    //private AccountTitles accountTitles;
    //private PurchasedSkins purchasedSkins;
    private const string PREMIUM_PRODUCT_ID = "com.deepjaws.premium_no_ads";
    private const string NOADD_PRODUCT_ID = "com.deepjaws.remove_interstitials";
    private const string SUPPORT_DEV_ID = "com.deepjaws.support_developer";
    private const string ASHFLOW_PRODUCT_ID = "com.deepjaws.ashflow";
    private const string CHAMELEON_PRODUCT_ID = "com.deepjaws.chameleon";
    private const string MERCURY_PRODUCT_ID = "com.deepjaws.mercury";
    //private const string PURCHASED_PRODUCT = "Purchased";
    private const string OWNED_PRODUCT = "Owned";
    private const string ERROR_PRICE = "---";


    private void Awake() {
        if (Instance != null && Instance != this) {
            Instance.ReInit(GPGmanager, purchaseUIvisualizer, m_StoreController);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        //local fast check on start without internet
        CheckPurchasesLocally();
    }

    void ReInit(GPG_Manager GPGmanager, PurchaseUIvisualizer purchaseUIvisualizer, StoreController m_StoreController) {
        this.GPGmanager = GPGmanager;//this.GPGmanager == null ? GPGmanager : this.GPGmanager;
        this.purchaseUIvisualizer = purchaseUIvisualizer;//this.purchaseUIvisualizer == null ? purchaseUIvisualizer : this.purchaseUIvisualizer;
        this.m_StoreController = this.m_StoreController == null ? m_StoreController : this.m_StoreController;

        OnUpdateAccountTittles = null;
        OnUpdatePurchasedSkins = null;
    }

    public void RefreshUI() {
        AccountTitles accountTitles = GetCurrentAccountTitles();
        PurchasedSkins purchasedSkins = GetCurrentPurchasedSkins();

        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins);
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
                new(SUPPORT_DEV_ID, ProductType.Consumable),
                new(ASHFLOW_PRODUCT_ID, ProductType.NonConsumable),
                new(CHAMELEON_PRODUCT_ID, ProductType.NonConsumable),
                new(MERCURY_PRODUCT_ID, ProductType.NonConsumable),
                new(Application.identifier, ProductType.NonConsumable)
            };

            m_StoreController.FetchProducts(initialProductsToFetch);
            if (purchaseUIvisualizer != null)
                purchaseUIvisualizer.SetActiveNoInternetWnd(false);
        }
        catch (Exception e) {
            Debug.LogError($"IAP v5: Offline mode or network error: {e.Message}");
            if (purchaseUIvisualizer != null)
                purchaseUIvisualizer.SetActiveNoInternetWnd(true);
        }
    }

    private void CheckPurchasesLocally() {
        if (ProgressSaver.IsDataExist()) {
            SaveData currentData = ProgressSaver.Load();
            if (currentData != null) {
                AccountTitles accountTitles = new();
                PurchasedSkins purchasedSkins = new();

                // player has immunited to ad purchases 
                if (currentData.isPremiumUser || currentData.isNoAD) //Debug.Log("Local check: Paid status found in the save file! Disabling interstitial ads.");
                    ApplyNoAdsLogic(true);

                string purchasedTxt = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_purchased");

                accountTitles.HasPremium = currentData.isPremiumUser;
                accountTitles.HasNoAd = currentData.isNoAD;
                accountTitles.HasFounder = currentData.isFounder;

                purchasedSkins.Mercury = currentData.hasMercurySkin;
                purchasedSkins.Chameleon = currentData.hasChameleon;
                purchasedSkins.Ashflow = currentData.hasAshflowSkin;
                purchasedSkins.Selected = currentData.selectedSkin;

                accountTitles.NoAdPrice = accountTitles.HasPremium ? /*PURCHASED_PRODUCT*/purchasedTxt : ERROR_PRICE;
                accountTitles.PremiumPrice = accountTitles.HasPremium ? /*PURCHASED_PRODUCT*/purchasedTxt : ERROR_PRICE;
                accountTitles.SupportDevPrice = ERROR_PRICE;
                purchasedSkins.MercuryPrice = purchasedSkins.Mercury ? OWNED_PRODUCT : ERROR_PRICE;
                purchasedSkins.ChameleonPrice = purchasedSkins.Chameleon ? OWNED_PRODUCT : ERROR_PRICE;
                purchasedSkins.AshflowPrice = purchasedSkins.Ashflow ? OWNED_PRODUCT : ERROR_PRICE;

                OnUpdateAccountTittles?.Invoke(accountTitles);
                OnUpdatePurchasedSkins?.Invoke(purchasedSkins);
                return;
            }
        }

        ApplyNoAdsLogic(false);//Debug.Log("Local check: No active ad purchases were found in the cache.");
    }

    // update GP purchases history
    private void OnPurchasesFetched(Orders orders) {
        AccountTitles accountTitles = new();
        PurchasedSkins purchasedSkins = new();
        string priceBuffer;

        if (orders.ConfirmedOrders != null) {
            //full premium
            if (IsHasProduct(orders, PREMIUM_PRODUCT_ID, out priceBuffer)) {
                accountTitles.HasPremium = true;
                accountTitles.PremiumPrice = priceBuffer;
            }
            else
                accountTitles.PremiumPrice = GetProductLocalPrice(PREMIUM_PRODUCT_ID);

            //only interstitial remove
            if (IsHasProduct(orders, NOADD_PRODUCT_ID, out priceBuffer)) {
                accountTitles.HasNoAd = true;
                accountTitles.NoAdPrice = priceBuffer;
            }
            else
                accountTitles.NoAdPrice = GetProductLocalPrice(NOADD_PRODUCT_ID);

            accountTitles.SupportDevPrice = GetProductLocalPrice(SUPPORT_DEV_ID);

            //old founder check
            if (IsHasProduct(orders, Application.identifier, out priceBuffer)) {
                accountTitles.HasPremium = true; //old buyers get all prem
                accountTitles.HasFounder = true;

                if (GPGmanager != null)
                    GPGmanager.OnAddToFoundersLeaderboard();
            }

            if (IsHasProduct(orders, ASHFLOW_PRODUCT_ID, out priceBuffer)) {
                purchasedSkins.Ashflow = true;
                purchasedSkins.AshflowPrice = priceBuffer;
            }
            else
                purchasedSkins.AshflowPrice = GetProductLocalPrice(ASHFLOW_PRODUCT_ID);

            if (IsHasProduct(orders, MERCURY_PRODUCT_ID, out priceBuffer)) {
                purchasedSkins.Mercury = true;
                purchasedSkins.MercuryPrice = priceBuffer;
            }
            else
                purchasedSkins.MercuryPrice = GetProductLocalPrice(MERCURY_PRODUCT_ID);

            if (IsHasProduct(orders, CHAMELEON_PRODUCT_ID, out priceBuffer)) {
                purchasedSkins.Chameleon = true;
                purchasedSkins.ChameleonPrice = priceBuffer;
            }
            else
                purchasedSkins.ChameleonPrice = GetProductLocalPrice(CHAMELEON_PRODUCT_ID);
        }

        if (ProgressSaver.IsDataExist())//load local sekected skuin
            purchasedSkins.Selected = ProgressSaver.Load().selectedSkin;

        UpdatePremiumInSaveFile(accountTitles, purchasedSkins);
        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins);
    }

    string GetProductLocalPrice(string id) {
        if (m_StoreController == null)
            return ERROR_PRICE;

        Product prod = m_StoreController.GetProductById(id);
        return prod != null ?
            prod.metadata.localizedPriceString : ERROR_PRICE;
    }

    bool IsHasProduct(Orders orders, string PRODUCT_ID, out string realPrice) {
        realPrice = GetProductLocalPrice(PREMIUM_PRODUCT_ID);

        return orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                     order.Info.PurchasedProductInfo != null &&
                                                     order.Info.PurchasedProductInfo.Any(p => p.productId == PRODUCT_ID));
    }

    // save purchase to ProgressSaver
    private void UpdatePremiumInSaveFile(AccountTitles accountTitles, PurchasedSkins purchasedSkins) {
        SaveData currentData = ProgressSaver.IsDataExist() ?
            ProgressSaver.Load() : new SaveData();

        //if (currentData != null) {
        currentData.isPremiumUser = accountTitles.HasPremium;
        currentData.isNoAD = accountTitles.HasNoAd;
        currentData.isFounder = accountTitles.HasFounder;

        currentData.hasMercurySkin = purchasedSkins.Mercury;
        currentData.hasChameleon = purchasedSkins.Chameleon;
        currentData.hasAshflowSkin = purchasedSkins.Ashflow;
        //currentData.selectedSkin = purchasedSkins.Selected;

        ProgressSaver.Save(currentData);//Debug.Log($"IAP v5: Purchase status has been successfully synced with Google Play and saved! Premium: {accountTitles.HasPremium}");
        ApplyNoAdsLogic(accountTitles.HasPremium || accountTitles.HasNoAd);//enab\disab ad in case
        //}
    }

    private void ApplyNoAdsLogic(bool disableAds) =>
        Debug.Log(disableAds ? "ADVERTISEMENTS ARE DISABLED FOR THIS SESSION" : "ADVERTISEMENT ACTIVE");

    public AccountTitles GetCurrentAccountTitles() {
        AccountTitles at = new AccountTitles();

        if (ProgressSaver.IsDataExist()) {
            SaveData currentData = ProgressSaver.Load();
            if (currentData != null) {
                at.HasNoAd = currentData.isNoAD;
                at.HasPremium = currentData.isPremiumUser;
                at.HasFounder = currentData.isFounder;
            }
            else {
                at.HasNoAd = false;
                at.HasPremium = false;
                at.HasFounder = false;
            }
        }

        string purchasedTxt = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_purchased");

        //local prices
        at.NoAdPrice = at.HasNoAd ? /*PURCHASED_PRODUCT */purchasedTxt : (m_StoreController != null ? GetProductLocalPrice(NOADD_PRODUCT_ID) : ERROR_PRICE);
        at.PremiumPrice = at.HasPremium ? /*PURCHASED_PRODUCT */purchasedTxt : (m_StoreController != null ? GetProductLocalPrice(PREMIUM_PRODUCT_ID) : ERROR_PRICE);
        at.SupportDevPrice = m_StoreController != null ? GetProductLocalPrice(SUPPORT_DEV_ID) : ERROR_PRICE;

        return at;
    }

    public PurchasedSkins GetCurrentPurchasedSkins() {
        PurchasedSkins ps = new PurchasedSkins();

        if (ProgressSaver.IsDataExist()) {
            SaveData data = ProgressSaver.Load();
            ps.Mercury = data.hasMercurySkin;
            ps.Chameleon = data.hasChameleon;
            ps.Ashflow = data.hasAshflowSkin;
            ps.Selected = data.selectedSkin;
        }

        ps.MercuryPrice = ps.Mercury ?
            OWNED_PRODUCT : (m_StoreController != null ?
                GetProductLocalPrice(MERCURY_PRODUCT_ID) : ERROR_PRICE);
        ps.ChameleonPrice = ps.Chameleon ?
            OWNED_PRODUCT : (m_StoreController != null ?
                GetProductLocalPrice(CHAMELEON_PRODUCT_ID) : ERROR_PRICE);
        ps.AshflowPrice = ps.Ashflow ?
            OWNED_PRODUCT : (m_StoreController != null ?
                GetProductLocalPrice(ASHFLOW_PRODUCT_ID) : ERROR_PRICE);

        return ps;
    }

    private void OnProductsFetched(List<Product> products) {
        AccountTitles accountTitles = GetCurrentAccountTitles();
        PurchasedSkins purchasedSkins = GetCurrentPurchasedSkins();

        //load prices
        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins);

        m_StoreController.FetchPurchases();//Debug.Log("IAP v5: Items received. Checking purchase history...");
    }

    // PROCESSING AN INCOMPLETE PURCHASE
    private void OnPurchasePending(PendingOrder pendingOrder) {
        if (pendingOrder.Info != null && pendingOrder.Info.PurchasedProductInfo != null
            && pendingOrder.Info.PurchasedProductInfo.Count > 0) {

            string productId = pendingOrder.Info.PurchasedProductInfo[0].productId;
            Debug.Log($"IAP v5: Transaction processing for a product: {productId}");

            SaveData currentData = ProgressSaver.IsDataExist() ?
                ProgressSaver.Load() : new SaveData();

            //if (currentData != null) {
            bool shouldConfirm = false;

            if (string.Equals(productId, PREMIUM_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.isPremiumUser = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, NOADD_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.isNoAD = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, MERCURY_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.hasMercurySkin = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, CHAMELEON_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.hasChameleon = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, ASHFLOW_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.hasAshflowSkin = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, SUPPORT_DEV_ID, StringComparison.Ordinal))
                shouldConfirm = true;    //for donut just confirm without flags in saave

            if (shouldConfirm) {
                ProgressSaver.Save(currentData);
                ApplyNoAdsLogic(currentData.isPremiumUser || currentData.isNoAD);

                // confirm purch in Google Play
                m_StoreController.ConfirmPurchase(pendingOrder);
                Debug.Log($"IAP v5: The purchase of {productId} has been successfully confirmed and saved.");

                /// immediateli force store history & => upd ui through events
                m_StoreController.FetchPurchases();
            }
            //}
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failedReason) {
        Debug.LogError($"IAP v5: Unable to load purchases. Reason: {failedReason}");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failedReason) {
        Debug.LogError($"IAP v5: Unable to load the order. Description: {failedReason.message}");
    }

    void BuyProduct(string PROD_ID) {
        if (m_StoreController != null) {
            // check internet conn before purchase
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                if (purchaseUIvisualizer != null)
                    purchaseUIvisualizer.SetActiveNoInternetWnd(true);
                return;
            }

            Debug.Log($"IAP v5: Start the purchase process for {PROD_ID}");
            m_StoreController.PurchaseProduct(PROD_ID);
        }
        else {
            Debug.LogError("IAP v5: Store controller not ready!");
            if (purchaseUIvisualizer != null)
                purchaseUIvisualizer.SetActiveNoInternetWnd(true);
        }
        //if (m_StoreController != null) {
        //    Debug.Log($"IAP v5: Start the purchase process for {PROD_ID}");
        //    m_StoreController.PurchaseProduct(PROD_ID);
        //}
        //else {
        //    Debug.LogError("IAP v5: The store controller isn't ready yet!");
        //}
    }

    public void OnBuy_Premium() => BuyProduct(PREMIUM_PRODUCT_ID);
    public void OnBuy_NoAd() => BuyProduct(NOADD_PRODUCT_ID);
    public void OnBuy_SupportDev() => BuyProduct(SUPPORT_DEV_ID);
    public void OnBuy_MercurySkin() => BuyProduct(MERCURY_PRODUCT_ID);
    public void OnBuy_ChameleonSkin() => BuyProduct(CHAMELEON_PRODUCT_ID);
    public void OnBuy_AshflowSkin() => BuyProduct(ASHFLOW_PRODUCT_ID);
}