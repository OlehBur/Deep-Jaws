using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;


[Serializable]
public class AccountsButtons {
    public Button buyNoAd;
    public Button buyPremium;
    public Button buySupportDev;
    public TextMeshProUGUI noAdTxt;//from price to bought and interactable false
    public TextMeshProUGUI premiumTxt;
    public TextMeshProUGUI supportDevTxt;

    //[Header("Discount Badges")]
    //public GameObject premiumDiscountObj;
    //public TextMeshProUGUI premiumDiscountTxt;
    //public GameObject noAdDiscountObj;
    //public TextMeshProUGUI noAdDiscountTxt;
    //public GameObject supportDevDiscountObj;
    //public TextMeshProUGUI supportDevDiscountTxt;

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

    //public GameObject GetDiscountObj(AccountPurchase s) =>
    //    s switch {
    //        AccountPurchase.Premium => premiumDiscountObj,
    //        AccountPurchase.NoAd => noAdDiscountObj,
    //        AccountPurchase.SupportDev => supportDevDiscountObj,
    //        _ => null
    //    };

    //public TextMeshProUGUI GetDiscountTxt(AccountPurchase s) =>
    //    s switch {
    //        AccountPurchase.Premium => premiumDiscountTxt,
    //        AccountPurchase.NoAd => noAdDiscountTxt,
    //        AccountPurchase.SupportDev => supportDevDiscountTxt,
    //        _ => null
    //    };
}

[Serializable]
public class SkinsButtons {
    public Button buyMercury;
    public Button selectMercury;
    public TextMeshProUGUI mercuryPriceTxt;
    public TextMeshProUGUI mercurySelectTxt;
    public Button buyChameleon;
    public Button selectChameleon;
    public TextMeshProUGUI chameleonPriceTxt;
    public TextMeshProUGUI chameleonSelectTxt;
    public Button buyAshflow;
    public Button selectAshflow;
    public TextMeshProUGUI ashflowPriceTxt;
    public TextMeshProUGUI ashflowSelectTxt;

    //[Header("Discount Badges")]
    //public GameObject mercuryDiscountObj;
    //public TextMeshProUGUI mercuryDiscountTxt;
    //public GameObject chameleonDiscountObj;
    //public TextMeshProUGUI chameleonDiscountTxt;
    //public GameObject ashflowDiscountObj;
    //public TextMeshProUGUI ashflowDiscountTxt;

    public static readonly Skin[] SKINS_ITERATORS = Enum.GetValues(typeof(Skin))
                    .Cast<Skin>()
                    .Skip(1)//without default
                    .ToArray();

    //public const string SELECT_TEXT = "SELECT";
    //public const string SELECTED_TEXT = "SELECTED";

    public Button GetButton(Skin s, bool isSelect) =>
        isSelect ?
        s switch {
            Skin.Chameleon => selectChameleon,
            Skin.Mercury => selectMercury,
            Skin.Ashflow => selectAshflow,
            _ => null
        } :
         s switch {
             Skin.Chameleon => buyChameleon,
             Skin.Mercury => buyMercury,
             Skin.Ashflow => buyAshflow,
             _ => null
         };

    /// <summary>
    /// return select button text for "select (interactable) or selected (non interactable)"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public TextMeshProUGUI GetText(Skin s, bool isSelectTxt) =>
        isSelectTxt ?
        s switch {
            Skin.Chameleon => chameleonSelectTxt,
            Skin.Mercury => mercurySelectTxt,
            Skin.Ashflow => ashflowSelectTxt,
            _ => null
        } :
        s switch {
            Skin.Chameleon => chameleonPriceTxt,
            Skin.Mercury => mercuryPriceTxt,
            Skin.Ashflow => ashflowPriceTxt,
            _ => null
        };

    //public GameObject GetDiscountObj(Skin s) =>
    //    s switch {
    //        Skin.Chameleon => chameleonDiscountObj,
    //        Skin.Mercury => mercuryDiscountObj,
    //        Skin.Ashflow => ashflowDiscountObj,
    //        _ => null
    //    };

    //public TextMeshProUGUI GetDiscountTxt(Skin s) =>
    //    s switch {
    //        Skin.Chameleon => chameleonDiscountTxt,
    //        Skin.Mercury => mercuryDiscountTxt,
    //        Skin.Ashflow => ashflowDiscountTxt,
    //        _ => null
    //    };
}


public class PurchaseUIvisualizer : MonoBehaviour {
    [SerializeField] AccountsButtons accountsButtons;
    [SerializeField] SkinsButtons skinsButtons;
    [SerializeField] Button resetSkinsButton;
    [SerializeField] Button restorePurchaseButton;
    [SerializeField] GameObject loyaltyRewardedWnd;
    [SerializeField] Animator titleAdFree;
    [SerializeField] Animator titlePremium;
    [SerializeField] Animator titleFounder;
    [SerializeField] GameObject NoInternetConnectionWnd;

    [SerializeField] GameObject skinsPreviewWnd;
    [SerializeField] Transform skinsPreview;
    [SerializeField] SkinnedMeshRenderer chameleonRenderer;
    [SerializeField] SkinnedMeshRenderer mercuryRenderer;
    [SerializeField] SkinnedMeshRenderer ashflowRenderer;

    private MaterialPropertyBlock propBlock;
    private PurchasedSkins purchasedSkins;//buffer only for click listeners
    private int selectedSkinsOnPreview = 0;

    //private const string PREF_LOYALTYCONFIRMED_KEY = "loyaltyWndConfirm";
    private const bool SELECT_BUTTON_TEXT_ID = true;
    private const bool BUY_BUTTON_TEXT_ID = false;
    private const float SKINS_PREVIEW_ROTATION_SPEED = 8f;


    private void Start() {
        if (GamePurchaseManager.Instance != null) {
            GamePurchaseManager.Instance.OnUpdatePurchasedSkins -= UpdateSkinsSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= UpdateAccountSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= InitMainMenuAccountTittles;

            GamePurchaseManager.Instance.OnUpdatePurchasedSkins += UpdateSkinsSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles += UpdateAccountSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles += InitMainMenuAccountTittles;

            LocalLoadTitlesAndSkins();
        }

        InitPreviewSkins();
        InitButtonsListeners();
        GamePurchaseManager.Instance?.RefreshUI();
    }


    void InitPreviewSkins() {
        propBlock = new MaterialPropertyBlock();

        chameleonRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_CHAMELEON_PROP, 1);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_MERCURY_PROP, 0);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_ASHFLOW_PROP, 0);
        chameleonRenderer.SetPropertyBlock(propBlock);

        mercuryRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_MERCURY_PROP, 1);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_MERCURYSCALE_PROP, (int)FishLevel.Moray + GameStaticInfo.MERCURY_SKIN_BASE_SCALE);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_CHAMELEON_PROP, 0);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_ASHFLOW_PROP, 0);
        mercuryRenderer.SetPropertyBlock(propBlock);

        ashflowRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_ASHFLOW_PROP, 1);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_MERCURY_PROP, 0);
        propBlock.SetFloat(GameStaticInfo.FISH_WAVING_CHAMELEON_PROP, 0);
        ashflowRenderer.SetPropertyBlock(propBlock);
    }

    private void Update() {
        if (skinsPreview.gameObject.activeInHierarchy)
            skinsPreview.Rotate(Vector3.up, Time.deltaTime * SKINS_PREVIEW_ROTATION_SPEED);
    }

    public void OnOpen_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(true);
        skinsPreviewWnd.SetActive(true);

        foreach(Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

        skinsPreview.GetChild(0).gameObject.SetActive(true);
    }

    public void OnClose_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(false);
        skinsPreviewWnd.SetActive(false);
    }

    public void OnNextSkinPreview() {
        skinsPreview.GetChild(selectedSkinsOnPreview).gameObject.SetActive(false);
        ++selectedSkinsOnPreview;

        selectedSkinsOnPreview = skinsPreview.childCount > selectedSkinsOnPreview ?
            selectedSkinsOnPreview :
            0;

        skinsPreview.GetChild(selectedSkinsOnPreview).gameObject.SetActive(true);
    }

    public void SetActiveNoInternetWnd(bool isActivate) {
        NoInternetConnectionWnd.SetActive(isActivate);
    }

    void InitButtonsListeners() {
        //skins
        skinsButtons.buyMercury.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_MercurySkin);
        skinsButtons.buyAshflow.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_AshflowSkin);
        skinsButtons.buyChameleon.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_ChameleonSkin);
        //accounts
        accountsButtons.buyNoAd.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_NoAd);
        accountsButtons.buyPremium.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_Premium);
        accountsButtons.buySupportDev.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_SupportDev);
        //other
        resetSkinsButton.onClick.AddListener(SetDefaultSkin);
        restorePurchaseButton.onClick.AddListener(GamePurchaseManager.Instance.OnRestorePurchases);
    }

    void SetDefaultSkin() {
        purchasedSkins.Selected = Skin.Default;

        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        //if (currentData != null) {
        currentData.selectedSkin = purchasedSkins.Selected;
        ProgressSaver.SaveData(currentData);        //}

        UpdateSkinsSection(purchasedSkins);
    }

    void UpdateSkinsSection(PurchasedSkins purchasedSkins) {
        this.purchasedSkins = purchasedSkins;

        foreach (Skin s in SkinsButtons.SKINS_ITERATORS) {
            skinsButtons.GetButton(s, BUY_BUTTON_TEXT_ID).gameObject.SetActive(true);
            skinsButtons.GetText(s, BUY_BUTTON_TEXT_ID).text = purchasedSkins.GetPrice(s);
            skinsButtons.GetButton(s, SELECT_BUTTON_TEXT_ID).gameObject.SetActive(false);
        }

        if (purchasedSkins.Chameleon)
            BuyedSkinUIBeh(Skin.Chameleon);
        if (purchasedSkins.Mercury)
            BuyedSkinUIBeh(Skin.Mercury);
        if (purchasedSkins.Ashflow)
            BuyedSkinUIBeh(Skin.Ashflow);

        //if (purchasedSkins.Selected != Skin.Default) {
        //    skinsButtons.GetText(purchasedSkins.Selected, SELECT_BUTTON_TEXT_ID).text = SkinsButtons.SELECTED_TEXT;
        //    skinsButtons.GetButton(purchasedSkins.Selected, SELECT_BUTTON_TEXT_ID).interactable = false;
        //}
        if (purchasedSkins.Selected != Skin.Default) {
            bool isOwned = purchasedSkins.Selected switch {
                Skin.Mercury => purchasedSkins.Mercury,
                Skin.Chameleon => purchasedSkins.Chameleon,
                Skin.Ashflow => purchasedSkins.Ashflow,
                _ => false
            };

            if (isOwned) {
                skinsButtons.GetButton(purchasedSkins.Selected, SELECT_BUTTON_TEXT_ID).interactable = false;
                skinsButtons.GetText(purchasedSkins.Selected, SELECT_BUTTON_TEXT_ID).text = /*SkinsButtons.SELECTED_TEXT*/LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_selected");
            }
        }
    }

    void BuyedSkinUIBeh(Skin s) {
        skinsButtons.GetButton(s, BUY_BUTTON_TEXT_ID).gameObject.SetActive(false);//buy disable
        skinsButtons.GetButton(s, SELECT_BUTTON_TEXT_ID).gameObject.SetActive(true);//select button active
        skinsButtons.GetButton(s, SELECT_BUTTON_TEXT_ID).interactable = true;//select interactable
        skinsButtons.GetText(s, SELECT_BUTTON_TEXT_ID).text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_select")/*SkinsButtons.SELECT_TEXT*/;
    }

    void SaveSelectedSkinToProgress(Skin skin) {
        if (ProgressSaver.IsAccountDataExist) {
            AccountData currentData = ProgressSaver.LoadAccountData();

            if (currentData != null) {
                currentData.selectedSkin = skin;
                ProgressSaver.SaveData(currentData);
            }
        }
    }

    public void OnSelectMercurySkinClick() {
        SaveSelectedSkinToProgress(Skin.Mercury);
        purchasedSkins.Selected = Skin.Mercury;
        UpdateSkinsSection(purchasedSkins);
    }
    public void OnSelectChameleonSkinClick() {
        SaveSelectedSkinToProgress(Skin.Chameleon);
        purchasedSkins.Selected = Skin.Chameleon;
        UpdateSkinsSection(purchasedSkins);
    }
    public void OnSelectAshflowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Ashflow);
        purchasedSkins.Selected = Skin.Ashflow;
        UpdateSkinsSection(purchasedSkins);
    }

    void UpdateAccountSection(AccountTitles accountTitles) {
        foreach (AccountPurchase ap in AccountsButtons.ACCOUNT_PURCH_ITERATORS) {
            accountsButtons.GetButton(ap).interactable = true;
            accountsButtons.GetText(ap).text = accountTitles.GetPrice(ap);
        }

        if (accountTitles.HasNoAd)
            BuyedAccountPurchUIBeh(AccountPurchase.NoAd);
        if (accountTitles.HasPremium)
            BuyedAccountPurchUIBeh(AccountPurchase.Premium);
        //support dev can buyed multiple times
    }

    void BuyedAccountPurchUIBeh(AccountPurchase ap) {
        accountsButtons.GetButton(ap).interactable = false;//buy disable
        accountsButtons.GetText(ap).text = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_purchased");//AccountsButtons.PURCHASED_TEXT;
    }

    void LocalLoadTitlesAndSkins() {
        AccountTitles at = GamePurchaseManager.Instance.GetCurrentAccountTitles();
        PurchasedSkins ps = GamePurchaseManager.Instance.GetCurrentPurchasedSkins();

        InitMainMenuAccountTittles(at);
        UpdateAccountSection(at);
        UpdateSkinsSection(ps);
    }

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

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;

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
}
