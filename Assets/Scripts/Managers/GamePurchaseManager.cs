using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Purchasing;
using VContainer;



public interface IGamePurchaseManager {
    event Action<AccountTitles> OnUpdateAccountTittles;
    event Action<PurchasedSkins, AccountData> OnUpdatePurchasedSkins;
    event Action OnNonInternetConnection;
    void ForceOnUpdateAccountTittles(AccountTitles at);
    void RefreshUI();
    void OnRestorePurchases();
    AccountTitles GetCurrentAccountTitles();
    PurchasedSkins GetCurrentPurchasedSkins();
    void OnBuy_Premium();
    void OnBuy_NoAd();
    void OnBuy_SupportDev();
    void OnBuy_MercurySkin();
    void OnBuy_ChameleonSkin();
    void OnBuy_AshflowSkin();
    void OnBuy_FreeSkin(Skin skin);
}

public class GamePurchaseManager : MonoBehaviour, IGamePurchaseManager {
    public event Action<AccountTitles> OnUpdateAccountTittles;
    public event Action<PurchasedSkins, AccountData> OnUpdatePurchasedSkins;
    public event Action OnNonInternetConnection;

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;

    private StoreController _StoreController;

    private const string PREMIUM_PRODUCT_ID = "com.deepjaws.premium_no_ads";
    private const string NOADD_PRODUCT_ID = "com.deepjaws.remove_interstitials";
    private const string SUPPORT_DEV_ID = "com.deepjaws.support_developer";
    private const string ASHFLOW_PRODUCT_ID = "com.deepjaws.ashflow";
    private const string ASHFLOW_DISCOUNT_PRODUCT_ID = "com.deepjaws.ashflow_discount";
    private const string CHAMELEON_PRODUCT_ID = "com.deepjaws.chameleon";
    private const string MERCURY_PRODUCT_ID = "com.deepjaws.mercury";
    private const string OWNED_PRODUCT = "Owned";
    private const string ERROR_PRICE = "---";

    private static readonly DateTime ASHFLOW_DISCOUNT_END_DATE = new DateTime(2026, 8, 14, 1, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DATE_NOW = DateTime.UtcNow;
    private static readonly bool IS_ASHFLOW_ON_SALE = DATE_NOW <= ASHFLOW_DISCOUNT_END_DATE;


    private void Awake() {
        CheckPurchasesLocally();
    }

    public void ForceOnUpdateAccountTittles(AccountTitles at) =>
        OnUpdateAccountTittles?.Invoke(at);

    public void RefreshUI() {
        AccountTitles accountTitles = GetCurrentAccountTitles();
        PurchasedSkins purchasedSkins = GetCurrentPurchasedSkins();

        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins, null);
    }

    async void Start() {
        //Debug.Log("IAP v5: Start of initialization...");

        _StoreController = UnityIAPServices.StoreController();

        _StoreController.OnPurchasePending += OnPurchasePending;
        _StoreController.OnProductsFetched += OnProductsFetched;
        _StoreController.OnPurchasesFetched += OnPurchasesFetched;
        _StoreController.OnProductsFetchFailed += OnProductsFetchFailed;
        _StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

        try {
            await _StoreController.Connect();
            //Debug.Log("IAP v5: Connected to the store. Retrieving products...");

            List<ProductDefinition> initialProductsToFetch = new List<ProductDefinition>
            {
                new(PREMIUM_PRODUCT_ID, ProductType.NonConsumable),
                new(NOADD_PRODUCT_ID, ProductType.NonConsumable),
                new(SUPPORT_DEV_ID, ProductType.Consumable),
                new(ASHFLOW_PRODUCT_ID, ProductType.NonConsumable),
                new(ASHFLOW_DISCOUNT_PRODUCT_ID, ProductType.NonConsumable),
                new(CHAMELEON_PRODUCT_ID, ProductType.NonConsumable),
                new(MERCURY_PRODUCT_ID, ProductType.NonConsumable)
            };

            _StoreController.FetchProducts(initialProductsToFetch);
            _StoreController.FetchPurchases();//get history for restore
        }
        catch (Exception e) {
            Debug.LogError($"IAP v5: Offline mode or network error: {e.Message}");
            OnNonInternetConnection?.Invoke();
        }
    }

    public void OnRestorePurchases() {
        if (_StoreController != null) {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                OnNonInternetConnection?.Invoke();

            Debug.Log("IAP v5: Manual restore purchases requested...");
            _StoreController.FetchPurchases();
        }
        else 
            Debug.LogWarning("IAP v5: Cannot restore, store controller is null.");
    }

    private void CheckPurchasesLocally() {
        if (ProgressSaver.IsAccountDataExist) {
            AccountData currentData = ProgressSaver.LoadAccountData();
            if (currentData != null) {
                AccountTitles accountTitles = new();
                PurchasedSkins purchasedSkins = new();

                //// player has immunited to ad purchases 
                //if (currentData.isPremiumUser || currentData.isNoAD) //Debug.Log("Local check: Paid status found in the save file! Disabling interstitial ads.");
                //    ApplyNoAdsLogic(true);

                string purchasedTxt = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_purchased");

                accountTitles.HasPremium = currentData.isPremiumUser;
                accountTitles.HasNoAd = currentData.isNoAD;
                accountTitles.HasFounder = currentData.isFounder;

                purchasedSkins.Mercury = currentData.hasMercurySkin;
                purchasedSkins.Chameleon = currentData.hasChameleon;
                purchasedSkins.Ashflow = currentData.hasAshflowSkin;

                accountTitles.NoAdPrice = accountTitles.HasPremium ? purchasedTxt : ERROR_PRICE;
                accountTitles.PremiumPrice = accountTitles.HasPremium ? purchasedTxt : ERROR_PRICE;
                accountTitles.SupportDevPrice = ERROR_PRICE;
                purchasedSkins.MercuryPrice = purchasedSkins.Mercury ? OWNED_PRODUCT : ERROR_PRICE;
                purchasedSkins.ChameleonPrice = purchasedSkins.Chameleon ? OWNED_PRODUCT : ERROR_PRICE;
                purchasedSkins.AshflowPrice = purchasedSkins.Ashflow ? OWNED_PRODUCT : ERROR_PRICE;

                OnUpdateAccountTittles?.Invoke(accountTitles);
                OnUpdatePurchasedSkins?.Invoke(purchasedSkins, currentData);
                return;
            }
        }

        //ApplyNoAdsLogic(false);//Debug.Log("Local check: No active ad purchases were found in the cache.");
    }

    // update GP purchases history
    private void OnPurchasesFetched(Orders orders) {
        AccountTitles accountTitles = GetCurrentAccountTitles();
        PurchasedSkins purchasedSkins = GetCurrentPurchasedSkins();

        if (orders.ConfirmedOrders != null) {
            if (IsHasProduct(orders, PREMIUM_PRODUCT_ID))   //full premium
                accountTitles.HasPremium = true;
            if (IsHasProduct(orders, NOADD_PRODUCT_ID))            //only interstitial remove
                accountTitles.HasNoAd = true;
            //old founder check in gpg_manager
            if (IsHasProduct(orders, ASHFLOW_PRODUCT_ID))
                purchasedSkins.Ashflow = true;
            if (IsHasProduct(orders, MERCURY_PRODUCT_ID))
                purchasedSkins.Mercury = true;
            if (IsHasProduct(orders, CHAMELEON_PRODUCT_ID))
                purchasedSkins.Chameleon = true;
        }

        UpdatePremiumInSaveFile(accountTitles, purchasedSkins);
        RefreshUI();
    }

    string GetProductLocalPrice(string id) {
        if (_StoreController == null)
            return ERROR_PRICE;

        Product prod = _StoreController.GetProductById(id);
        return prod != null ?
            prod.metadata.localizedPriceString : ERROR_PRICE;
    }


    bool IsHasProduct(Orders orders, string PRODUCT_ID) {
        return orders.ConfirmedOrders.Any(order => order.Info != null &&
                                                     order.Info.PurchasedProductInfo != null &&
                                                     order.Info.PurchasedProductInfo.Any(p => p.productId == PRODUCT_ID));
    }

    // save purchase to ProgressSaver
    private void UpdatePremiumInSaveFile(AccountTitles accountTitles, PurchasedSkins purchasedSkins) {
        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        currentData.isPremiumUser = accountTitles.HasPremium;
        currentData.isNoAD = accountTitles.HasNoAd;
        currentData.isFounder = accountTitles.HasFounder;

        currentData.hasMercurySkin = purchasedSkins.Mercury;
        currentData.hasChameleon = purchasedSkins.Chameleon;
        currentData.hasAshflowSkin = purchasedSkins.Ashflow;

        ProgressSaver.SaveData(currentData);//Debug.Log($"IAP v5: Purchase status has been successfully synced with Google Play and saved! Premium: {accountTitles.HasPremium}");
        //ApplyNoAdsLogic(accountTitles.HasPremium || accountTitles.HasNoAd);//enab\disab ad in case
    }

    //private void ApplyNoAdsLogic(bool disableAds) =>
    //    Debug.Log(disableAds ? "ADVERTISEMENTS ARE DISABLED FOR THIS SESSION" : "ADVERTISEMENT ACTIVE");

    public AccountTitles GetCurrentAccountTitles() {
        AccountTitles at = new AccountTitles();

        if (ProgressSaver.IsAccountDataExist) {
            AccountData currentData = ProgressSaver.LoadAccountData();
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
        at.NoAdPrice = at.HasNoAd ? purchasedTxt : (_StoreController != null ? GetProductLocalPrice(NOADD_PRODUCT_ID) : ERROR_PRICE);
        at.PremiumPrice = at.HasPremium ? purchasedTxt : (_StoreController != null ? GetProductLocalPrice(PREMIUM_PRODUCT_ID) : ERROR_PRICE);
        at.SupportDevPrice = _StoreController != null ? GetProductLocalPrice(SUPPORT_DEV_ID) : ERROR_PRICE;

        return at;
    }

    public PurchasedSkins GetCurrentPurchasedSkins() {
        PurchasedSkins ps = new PurchasedSkins();

        if (ProgressSaver.IsAccountDataExist) {
            AccountData data = ProgressSaver.LoadAccountData();
            ps.Mercury = data.hasMercurySkin;
            ps.Chameleon = data.hasChameleon;
            ps.Ashflow = data.hasAshflowSkin;
        }

        ps.MercuryPrice = ps.Mercury ?
            OWNED_PRODUCT : (_StoreController != null ?
                GetProductLocalPrice(MERCURY_PRODUCT_ID) : ERROR_PRICE);
        ps.ChameleonPrice = ps.Chameleon ?
            OWNED_PRODUCT : (_StoreController != null ?
                GetProductLocalPrice(CHAMELEON_PRODUCT_ID) : ERROR_PRICE);

        if (ps.Ashflow)
            ps.AshflowPrice = OWNED_PRODUCT;
        else if (IS_ASHFLOW_ON_SALE && _StoreController != null) {
            string oldPrice = GetProductLocalPrice(ASHFLOW_PRODUCT_ID);          
            string newPrice = GetProductLocalPrice(ASHFLOW_DISCOUNT_PRODUCT_ID); 

            ps.AshflowPrice = $"<s><color=#A0A0A0>{oldPrice}</color></s> {newPrice}";
        }
        else
            ps.AshflowPrice = GetProductLocalPrice(ASHFLOW_PRODUCT_ID);

        return ps;

        //return ps;
    }

    private void OnProductsFetched(List<Product> products) {
        AccountTitles accountTitles = GetCurrentAccountTitles();
        PurchasedSkins purchasedSkins = GetCurrentPurchasedSkins();

        //load prices
        OnUpdateAccountTittles?.Invoke(accountTitles);
        OnUpdatePurchasedSkins?.Invoke(purchasedSkins, null);
    }

    // PROCESSING AN INCOMPLETE PURCHASE
    private void OnPurchasePending(PendingOrder pendingOrder) {
        if (pendingOrder.Info != null && pendingOrder.Info.PurchasedProductInfo != null
            && pendingOrder.Info.PurchasedProductInfo.Count > 0) {

            string productId = pendingOrder.Info.PurchasedProductInfo[0].productId;
            Debug.Log($"IAP v5: Transaction processing for a product: {productId}");

            AccountData currentData = ProgressSaver.IsAccountDataExist ?
                ProgressSaver.LoadAccountData() : new AccountData();

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
            else if (string.Equals(productId, ASHFLOW_PRODUCT_ID, StringComparison.Ordinal) ||
                    string.Equals(productId, ASHFLOW_DISCOUNT_PRODUCT_ID, StringComparison.Ordinal)) {
                currentData.hasAshflowSkin = true;
                shouldConfirm = true;
            }
            else if (string.Equals(productId, SUPPORT_DEV_ID, StringComparison.Ordinal))
                shouldConfirm = true;    //for donut just confirm without flags in saave

            if (shouldConfirm) {
                ProgressSaver.SaveData(currentData);
                //ApplyNoAdsLogic(currentData.isPremiumUser || currentData.isNoAD);

                // confirm purch in Google Play
                _StoreController.ConfirmPurchase(pendingOrder);
                Debug.Log($"IAP v5: The purchase of {productId} has been successfully confirmed and saved.");

                /// immediateli force store history & => upd ui through events
                _StoreController.FetchPurchases();
            }
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failedReason) {
        Debug.LogError($"IAP v5: Unable to load purchases. Reason: {failedReason}");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failedReason) {
        Debug.LogError($"IAP v5: Unable to load the order. Description: {failedReason.message}");
    }

    void BuyProduct(string PROD_ID) {
        if (_StoreController != null) {
            // check internet conn before purchase
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                OnNonInternetConnection?.Invoke();
                return;
            }

            Debug.Log($"IAP v5: Start the purchase process for {PROD_ID}");
            _StoreController.PurchaseProduct(PROD_ID);
        }
        else {
            Debug.LogError("IAP v5: Store controller not ready!");
            OnNonInternetConnection?.Invoke();
        }
    }

    public void OnBuy_Premium() => BuyProduct(PREMIUM_PRODUCT_ID);
    public void OnBuy_NoAd() => BuyProduct(NOADD_PRODUCT_ID);
    public void OnBuy_SupportDev() => BuyProduct(SUPPORT_DEV_ID);
    public void OnBuy_MercurySkin() => BuyProduct(MERCURY_PRODUCT_ID);
    public void OnBuy_ChameleonSkin() => BuyProduct(CHAMELEON_PRODUCT_ID);
    public void OnBuy_AshflowSkin() => BuyProduct(IS_ASHFLOW_ON_SALE ? ASHFLOW_DISCOUNT_PRODUCT_ID : ASHFLOW_PRODUCT_ID);

    public void OnBuy_FreeSkin(Skin skin) {
        AccountData data = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();
        int price = GameStaticInfo.GetFreeSkinPrice(skin);

        switch (skin) {
            case Skin.Porcelain:
            data.hasPorcelain = true;
            break;
            case Skin.Zebra:
            data.hasZebra = true;
            break;
            case Skin.Checkerboard:
            data.hasCheckerboard = true;
            break;
            case Skin.Blinker:
            data.hasBlinker = true;
            break;
            case Skin.Cupid:
            data.hasCupid = true;
            break;
            case Skin.Fairylight:
            data.hasFairylight = true;
            break;
            case Skin.Pixy:
            data.hasPixy = true;
            break;
            case Skin.Corrosion:
            data.hasCorrosion = true;
            break;
            case Skin.Sun:
            data.hasSun = true;
            break;
            case Skin.Invert:
            data.hasInvert = true;
            break;
            case Skin.Rainbow:
            data.hasRainbow = true;
            break;
            case Skin.Cosmo:
            data.hasCosmo = true;
            break;
            case Skin.Singularity:
            data.hasSingularity = true;
            break;
        }

        if (_sanctuariesManager != null)
            _sanctuariesManager.OnAddShells(-price);
        else
            Debug.LogWarning("Sanctuaries Manager Instance is null!");

        ProgressSaver.SaveData(data);
    }


}