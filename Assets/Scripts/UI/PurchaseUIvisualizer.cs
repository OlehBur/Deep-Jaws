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
public class SkinStoreCard {
    public Button buyB;
    public Button selectB;
    public TextMeshProUGUI priceTxt;
    public TextMeshProUGUI selectTxt;
}

[Serializable]
public class SkinsButtons {
    public SkinStoreCard mercury;
    public SkinStoreCard chameleon;
    public SkinStoreCard ashflow;
    public SkinStoreCard zebra;
    public SkinStoreCard singularity;
    public SkinStoreCard blinker;
    public SkinStoreCard checkerboard;
    public SkinStoreCard corrosion;
    public SkinStoreCard cosmo;
    public SkinStoreCard cupid;
    public SkinStoreCard fairylights;
    public SkinStoreCard pixy;
    public SkinStoreCard porcelain;
    public SkinStoreCard rainbow;
    public SkinStoreCard sun;

    //public Button buyMercury;
    //public Button selectMercury;
    //public TextMeshProUGUI mercuryPriceTxt;
    //public TextMeshProUGUI mercurySelectTxt;
    //public Button buyChameleon;
    //public Button selectChameleon;
    //public TextMeshProUGUI chameleonPriceTxt;
    //public TextMeshProUGUI chameleonSelectTxt;
    //public Button buyAshflow;
    //public Button selectAshflow;
    //public TextMeshProUGUI ashflowPriceTxt;
    //public TextMeshProUGUI ashflowSelectTxt;

    //[Header("Discount Badges")]
    //public GameObject mercuryDiscountObj;
    //public TextMeshProUGUI mercuryDiscountTxt;
    //public GameObject chameleonDiscountObj;
    //public TextMeshProUGUI chameleonDiscountTxt;
    //public GameObject ashflowDiscountObj;
    //public TextMeshProUGUI ashflowDiscountTxt;

    public static readonly Skin[] PURCHASE_SKINS_ITERATORS = new Skin[]
    {
        Skin.Mercury,
        Skin.Chameleon,
        Skin.Ashflow
    };
    //Enum.GetValues(typeof(Skin))
    //            .Cast<Skin>()
    //            .Skip(1)//without default
    //            .ToArray();

    //public const string SELECT_TEXT = "SELECT";
    //public const string SELECTED_TEXT = "SELECTED";

    public Button GetButton(Skin s, bool isSelect) =>
        isSelect ?
        s switch {
            Skin.Chameleon => chameleon.selectB,
            Skin.Mercury => mercury.selectB,
            Skin.Ashflow => ashflow.selectB,

            Skin.Zebra => zebra.selectB,
            Skin.Singularity => singularity.selectB,
            Skin.Blinker => blinker.selectB,
            Skin.Checkerboard => checkerboard.selectB,
            Skin.Corrosion => corrosion.selectB,
            Skin.Cosmo => cosmo.selectB,
            Skin.Cupid => cupid.selectB,
            Skin.Fairylight => fairylights.selectB,
            Skin.Pixy => pixy.selectB,
            Skin.Porcelain => porcelain.selectB,
            Skin.Rainbow => rainbow.selectB,
            Skin.Sun => sun.selectB,
            _ => null
        } :
         s switch {
             Skin.Chameleon => chameleon.buyB,
             Skin.Mercury => mercury.buyB,
             Skin.Ashflow => ashflow.buyB,

             Skin.Zebra => zebra.buyB,
             Skin.Singularity => singularity.buyB,
             Skin.Blinker => blinker.buyB,
             Skin.Checkerboard => checkerboard.buyB,
             Skin.Corrosion => corrosion.buyB,
             Skin.Cosmo => cosmo.buyB,
             Skin.Cupid => cupid.buyB,
             Skin.Fairylight => fairylights.buyB,
             Skin.Pixy => pixy.buyB,
             Skin.Porcelain => porcelain.buyB,
             Skin.Rainbow => rainbow.buyB,
             Skin.Sun => sun.buyB,
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
            Skin.Chameleon => chameleon.selectTxt,
            Skin.Mercury => mercury.selectTxt,
            Skin.Ashflow => ashflow.selectTxt,

            Skin.Zebra => zebra.selectTxt,
            Skin.Singularity => singularity.selectTxt,
            Skin.Blinker => blinker.selectTxt,
            Skin.Checkerboard => checkerboard.selectTxt,
            Skin.Corrosion => corrosion.selectTxt,
            Skin.Cosmo => cosmo.selectTxt,
            Skin.Cupid => cupid.selectTxt,
            Skin.Fairylight => fairylights.selectTxt,
            Skin.Pixy => pixy.selectTxt,
            Skin.Porcelain => porcelain.selectTxt,
            Skin.Rainbow => rainbow.selectTxt,
            Skin.Sun => sun.selectTxt,
            _ => null
        } :
        s switch {
            Skin.Chameleon => chameleon.priceTxt,
            Skin.Mercury => mercury.priceTxt,
            Skin.Ashflow => ashflow.priceTxt,

            Skin.Zebra => zebra.priceTxt,
            Skin.Singularity => singularity.priceTxt,
            Skin.Blinker => blinker.priceTxt,
            Skin.Checkerboard => checkerboard.priceTxt,
            Skin.Corrosion => corrosion.priceTxt,
            Skin.Cosmo => cosmo.priceTxt,
            Skin.Cupid => cupid.priceTxt,
            Skin.Fairylight => fairylights.priceTxt,
            Skin.Pixy => pixy.priceTxt,
            Skin.Porcelain => porcelain.priceTxt,
            Skin.Rainbow => rainbow.priceTxt,
            Skin.Sun => sun.priceTxt,
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
    [SerializeField] FishSkinManager fishSkinManager;
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


    [SerializeField] SkinnedMeshRenderer chameleon_Seabass;
    [SerializeField] SkinnedMeshRenderer mercury_Tuna;
    [SerializeField] SkinnedMeshRenderer ashflow_Mackarel;
    [SerializeField] SkinnedMeshRenderer zebra_Barracuda;
    [SerializeField] SkinnedMeshRenderer singularity_Tuna;
    [SerializeField] SkinnedMeshRenderer blinker_Lionfish;
    [SerializeField] SkinnedMeshRenderer checkerboard_Seabass;
    [SerializeField] SkinnedMeshRenderer corrosion_Mackarel;
    [SerializeField] SkinnedMeshRenderer cosmo_Seabass;
    [SerializeField] SkinnedMeshRenderer cupid_Barracuda;
    [SerializeField] SkinnedMeshRenderer fairylight_Lionfish;
    [SerializeField] SkinnedMeshRenderer pixy_Mackarel;
    [SerializeField] SkinnedMeshRenderer porcelain_Barracuda;
    [SerializeField] SkinnedMeshRenderer rainbow_Tuna;    
    [SerializeField] SkinnedMeshRenderer sun_Lionfish;

    //private MaterialPropertyBlock propBlock;
    private PurchasedSkins purchasedSkins;//buffer only for click listeners
    private int selectedSkinOnPreview = 0;

    //private const string PREF_LOYALTYCONFIRMED_KEY = "loyaltyWndConfirm";
    private const bool SELECT_BUTTON_TEXT_ID = true;
    private const bool BUY_BUTTON_TEXT_ID = false;
    private const float SKINS_PREVIEW_ROTATION_SPEED = 8f;


    private void Start() {
        if (GamePurchaseManager.Instance != null) {
            GamePurchaseManager.Instance.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= UpdateAccountSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= InitMainMenuAccountTittles;

            GamePurchaseManager.Instance.OnUpdatePurchasedSkins += UpdatePaidSkinsSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles += UpdateAccountSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles += InitMainMenuAccountTittles;

            LocalLoadTitlesAndSkins();
        }

        InitPreviewSkins();
        InitButtonsListeners();
        GamePurchaseManager.Instance?.RefreshUI();
    }


    void InitPreviewSkins() {
        fishSkinManager.InitSkin(chameleon_Seabass, Skin.Chameleon, FishLevel.SeaBass);
        fishSkinManager.InitSkin(mercury_Tuna, Skin.Mercury, FishLevel.Tuna);
        fishSkinManager.InitSkin(ashflow_Mackarel, Skin.Ashflow, FishLevel.Mackerel);
        fishSkinManager.InitSkin(zebra_Barracuda, Skin.Zebra, FishLevel.Barracuda);
        fishSkinManager.InitSkin(singularity_Tuna, Skin.Singularity, FishLevel.Tuna);
        fishSkinManager.InitSkin(blinker_Lionfish, Skin.Blinker, FishLevel.Lionfish);
        fishSkinManager.InitSkin(checkerboard_Seabass, Skin.Checkerboard, FishLevel.SeaBass);
        fishSkinManager.InitSkin(corrosion_Mackarel, Skin.Corrosion, FishLevel.Mackerel);
        fishSkinManager.InitSkin(cosmo_Seabass, Skin.Cosmo, FishLevel.SeaBass);
        fishSkinManager.InitSkin(cupid_Barracuda, Skin.Cupid, FishLevel.Barracuda);
        fishSkinManager.InitSkin(fairylight_Lionfish, Skin.Fairylight, FishLevel.Lionfish);
        fishSkinManager.InitSkin(pixy_Mackarel, Skin.Pixy, FishLevel.Mackerel);
        fishSkinManager.InitSkin(porcelain_Barracuda, Skin.Porcelain, FishLevel.Barracuda);
        fishSkinManager.InitSkin(rainbow_Tuna, Skin.Rainbow, FishLevel.Tuna);
        fishSkinManager.InitSkin(sun_Lionfish, Skin.Sun, FishLevel.Lionfish);

    }

    private void Update() {
        if (skinsPreview.gameObject.activeInHierarchy)
            skinsPreview.Rotate(Vector3.up, Time.deltaTime * SKINS_PREVIEW_ROTATION_SPEED);
    }

    public void OnOpen_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(true);
        skinsPreviewWnd.SetActive(true);

        foreach (Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

        selectedSkinOnPreview = 0;
        skinsPreview.GetChild(selectedSkinOnPreview).gameObject.SetActive(true);
    }

    public void OnClose_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(false);
        skinsPreviewWnd.SetActive(false);
    }

    public void OnNextSkinPreview() {
        skinsPreview.GetChild(selectedSkinOnPreview).gameObject.SetActive(false);
        ++selectedSkinOnPreview;

        selectedSkinOnPreview = skinsPreview.childCount > selectedSkinOnPreview ?
            selectedSkinOnPreview :
            0;

        skinsPreview.GetChild(selectedSkinOnPreview).gameObject.SetActive(true);
    }

    public void SetActiveNoInternetWnd(bool isActivate) {
        NoInternetConnectionWnd.SetActive(isActivate);
    }

    void InitButtonsListeners() {
        if (skinsButtons.rainbow.buyB == null) {
            Debug.LogWarning("Skin buttons = null");
            return;
        }

        //skins
        skinsButtons.mercury.buyB.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_MercurySkin);
        skinsButtons.ashflow.buyB.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_AshflowSkin);
        skinsButtons.chameleon.buyB.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_ChameleonSkin);
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

        UpdatePaidSkinsSection(purchasedSkins);
    }

    void UpdatePaidSkinsSection(PurchasedSkins purchasedSkins) {
        this.purchasedSkins = purchasedSkins;
        if (skinsButtons.rainbow.buyB == null) {
            Debug.LogWarning("Skin buttons = null");
            return;
        }

        foreach (Skin s in SkinsButtons.PURCHASE_SKINS_ITERATORS) {
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
        UpdatePaidSkinsSection(purchasedSkins);
    }
    public void OnSelectChameleonSkinClick() {
        SaveSelectedSkinToProgress(Skin.Chameleon);
        purchasedSkins.Selected = Skin.Chameleon;
        UpdatePaidSkinsSection(purchasedSkins);
    }
    public void OnSelectAshflowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Ashflow);
        purchasedSkins.Selected = Skin.Ashflow;
        UpdatePaidSkinsSection(purchasedSkins);
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
        UpdatePaidSkinsSection(ps);
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
