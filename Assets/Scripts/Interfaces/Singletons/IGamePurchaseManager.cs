
using System;

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