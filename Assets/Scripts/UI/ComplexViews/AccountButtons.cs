using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



[Serializable]
public class AccountsButtons {
    public StoreCardView noAdCard;
    public StoreCardView premiumCard;
    public StoreCardView supportDevCard;
    //public Button buyNoAd;
    //public Button buyPremium;
    //public Button buySupportDev;
    //public TextMeshProUGUI noAdTxt;//from price to bought and interactable false
    //public TextMeshProUGUI premiumTxt;
    //public TextMeshProUGUI supportDevTxt;


    public static readonly AccountPurchase[] ACCOUNT_PURCH_ITERATORS = (AccountPurchase[])Enum.GetValues(typeof(AccountPurchase)); //Enum.GetValues(typeof(AccountPurchase))                    .Cast<AccountPurchase>()                    .ToArray();

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