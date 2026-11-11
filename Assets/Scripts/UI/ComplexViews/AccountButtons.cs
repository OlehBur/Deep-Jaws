using System;
using UnityEngine;



[Serializable]
public class AccountsButtons {
    public StoreCardView noAdCard;
    public StoreCardView premiumCard;
    public StoreCardView supportDevCard;

    public static readonly AccountPurchase[] ACCOUNT_PURCH_ITERATORS = (AccountPurchase[])Enum.GetValues(typeof(AccountPurchase)); //Enum.GetValues(typeof(AccountPurchase))                    .Cast<AccountPurchase>()                    .ToArray();


    public void InitDI(UILocalizationInfo localizationInfo) {
        foreach(AccountPurchase ac in ACCOUNT_PURCH_ITERATORS)
            GetStoreCard(ac).InitDI(localizationInfo);
    }

    public StoreCardView GetStoreCard(AccountPurchase s) =>
        s switch {
            AccountPurchase.Premium => premiumCard,
            AccountPurchase.NoAd => noAdCard,
            AccountPurchase.SupportDev => supportDevCard,
            _ => null
        };

    public void InitListeners(IGamePurchaseManager gamePurchaseManager) {
        if (gamePurchaseManager != null) {
            noAdCard.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_NoAd);
            premiumCard.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_Premium);
            supportDevCard.BuyOnClick.AddListener(gamePurchaseManager.OnBuy_SupportDev);
        }
        else
            Debug.LogWarning("GamePurchase manager Instance is null!");
    }
}