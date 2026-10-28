using System;
using TMPro;
using UnityEngine.UI;



[Serializable]
public class AccountsButtons {
    public Button buyNoAd;
    public Button buyPremium;
    public Button buySupportDev;
    public TextMeshProUGUI noAdTxt;//from price to bought and interactable false
    public TextMeshProUGUI premiumTxt;
    public TextMeshProUGUI supportDevTxt;


    public static readonly AccountPurchase[] ACCOUNT_PURCH_ITERATORS = (AccountPurchase[])Enum.GetValues(typeof(AccountPurchase)); //Enum.GetValues(typeof(AccountPurchase))                    .Cast<AccountPurchase>()                    .ToArray();

    public Button GetButton(AccountPurchase s) =>
        s switch {
            AccountPurchase.Premium => buyPremium,
            AccountPurchase.NoAd => buyNoAd,
            AccountPurchase.SupportDev => buySupportDev,
            _ => null
        };

    public TextMeshProUGUI GetText(AccountPurchase s) =>
        s switch {
            AccountPurchase.Premium => premiumTxt,
            AccountPurchase.NoAd => noAdTxt,
            AccountPurchase.SupportDev => supportDevTxt,
            _ => null
        };
}