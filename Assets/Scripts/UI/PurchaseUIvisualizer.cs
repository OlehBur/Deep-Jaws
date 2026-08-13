using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Linq;


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
public class SkinsCards {
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
    public SkinStoreCard invert;

    public static readonly Skin[] ALL_SKINS_ITETATORS = Enum.GetValues(typeof(Skin))
        .Cast<Skin>()
        .ToArray();

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
    public static readonly Skin[] FREE_SKINS_ITERATORS = new Skin[] {
        Skin.Invert,
        Skin.Zebra,
        Skin.Singularity,
        Skin.Blinker,
        Skin.Checkerboard,
        Skin.Corrosion,
        Skin.Cosmo,
        Skin.Cupid,
        Skin.Fairylight,
        Skin.Pixy,
        Skin.Porcelain,
        Skin.Rainbow,
        Skin.Sun,
    };

    public SkinStoreCard GetSkinCard(Skin type) =>
    type switch {
        Skin.Chameleon => chameleon,
        Skin.Mercury => mercury,
        Skin.Ashflow => ashflow,
        Skin.Zebra => zebra,
        Skin.Singularity => singularity,
        Skin.Blinker => blinker,
        Skin.Checkerboard => checkerboard,
        Skin.Corrosion => corrosion,
        Skin.Cosmo => cosmo,
        Skin.Cupid => cupid,
        Skin.Fairylight => fairylights,
        Skin.Pixy => pixy,
        Skin.Porcelain => porcelain,
        Skin.Rainbow => rainbow,
        Skin.Sun => sun,
        Skin.Invert => invert,
        _ => chameleon
    };

    public void InitListeners() {
        if (GamePurchaseManager.Instance != null) {
            mercury.BuyOnClick.AddListener(GamePurchaseManager.Instance.OnBuy_MercurySkin);
            ashflow.BuyOnClick.AddListener(GamePurchaseManager.Instance.OnBuy_AshflowSkin);
            chameleon.BuyOnClick.AddListener(GamePurchaseManager.Instance.OnBuy_ChameleonSkin);
        }
        else
            Debug.LogWarning("GamePurchase manager Instance is null!");

        zebra.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Zebra);
            zebra.ShowSelectUI();
        });
        singularity.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Singularity);
            singularity.ShowSelectUI();
        });
        blinker.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Blinker);
            blinker.ShowSelectUI();
        });
        checkerboard.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Checkerboard);
            checkerboard.ShowSelectUI();
        });
        corrosion.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Corrosion);
            corrosion.ShowSelectUI();
        });
        cosmo.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Cosmo);
            cosmo.ShowSelectUI();
        });
        cupid.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Cupid);
            cupid.ShowSelectUI();
        });
        fairylights.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Fairylight);
            fairylights.ShowSelectUI();
        });
        pixy.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Pixy);
            pixy.ShowSelectUI();
        });
        porcelain.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Porcelain);
            porcelain.ShowSelectUI();
        });
        rainbow.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Rainbow);
            rainbow.ShowSelectUI();
        });
        sun.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Sun);
            sun.ShowSelectUI();
        });
        invert.BuyOnClick.AddListener(() => {
            GamePurchaseManager.Instance.OnBuy_FreeSkin(Skin.Invert);
            invert.ShowSelectUI();
        });
    }
}


public class PurchaseUIvisualizer : MonoBehaviour {
    [SerializeField] FishSkinManager fishSkinManager;
    [SerializeField] AccountsButtons accountsButtons;
    [SerializeField] SkinsCards skinsCards;
    [SerializeField] GameObject storeWnd;
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
    [SerializeField] SkinnedMeshRenderer invert_Seabass;

    //private MaterialPropertyBlock propBlock;
    private PurchasedSkins purchasedSkins;//buffer only for click listeners
    private int selectedSkinOnPreview = 0;

    //private const string PREF_LOYALTYCONFIRMED_KEY = "loyaltyWndConfirm";
    //private const bool SELECT_BUTTON_TEXT_ID = true;
    //private const bool BUY_BUTTON_TEXT_ID = false;
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

        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnShellsAmountChanged += UpdateFreeSkinsBuyInteractable;
            UpdateFreeSkinsBuyInteractable(SanctuariesManager.Instance.ShellsCnt);
        }

        UpdateFreeSkinsSection(null);
        InitPreviewSkins();
        InitButtonsListeners();
        GamePurchaseManager.Instance?.RefreshUI();
    }

    private void OnDisable() {
        if (GamePurchaseManager.Instance != null) {
            GamePurchaseManager.Instance.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= UpdateAccountSection;
            GamePurchaseManager.Instance.OnUpdateAccountTittles -= InitMainMenuAccountTittles;
        }

        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnShellsAmountChanged -= UpdateFreeSkinsBuyInteractable;
        }
    }

    void UpdateFreeSkinsBuyInteractable(int cntShells) {
        bool enoughShellsBuff = false;
        foreach (Skin s in SkinsCards.FREE_SKINS_ITERATORS) {
            enoughShellsBuff = GameStaticInfo.GetFreeSkinPrice(s) <= cntShells;
            skinsCards.GetSkinCard(s).SetBuyInterctable(enoughShellsBuff);
        }
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
        fishSkinManager.InitSkin(invert_Seabass, Skin.Invert, FishLevel.SeaBass);

        foreach (Skin s in SkinsCards.ALL_SKINS_ITETATORS)
            skinsCards.GetSkinCard(s).PreviewOnClick.AddListener(() => OpenPreviewWithCurrentSkin(s));
        //Debug.LogWarning("Todo init view buttons for paid skins");
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
        //skinsPreview.GetChild(selectedSkinOnPreview).gameObject.SetActive(false);
        foreach (Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

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
        //if (skinsButtons.rainbow.buyB == null) {
        //    Debug.LogWarning("Skin buttons = null");
        //    return;
        //}

        //skins
        skinsCards.InitListeners();

        //accounts
        accountsButtons.buyNoAd.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_NoAd);
        accountsButtons.buyPremium.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_Premium);
        accountsButtons.buySupportDev.onClick.AddListener(GamePurchaseManager.Instance.OnBuy_SupportDev);
        //other
        resetSkinsButton.onClick.AddListener(SetDefaultSkin);
        restorePurchaseButton.onClick.AddListener(GamePurchaseManager.Instance.OnRestorePurchases);
    }

    void OpenPreviewWithCurrentSkin(Skin skin) {
        storeWnd.SetActive(false);
        skinsPreview.gameObject.SetActive(true);
        skinsPreviewWnd.SetActive(true);

        Renderer neededRenderer = skin switch {
            Skin.Chameleon => chameleon_Seabass,
            Skin.Mercury => mercury_Tuna,
            Skin.Ashflow => ashflow_Mackarel,
            Skin.Zebra => zebra_Barracuda,
            Skin.Singularity => singularity_Tuna,
            Skin.Blinker => blinker_Lionfish,
            Skin.Checkerboard => checkerboard_Seabass,
            Skin.Corrosion => corrosion_Mackarel,
            Skin.Cosmo => cosmo_Seabass,
            Skin.Cupid => cupid_Barracuda,
            Skin.Fairylight => fairylight_Lionfish,
            Skin.Pixy => pixy_Mackarel,
            Skin.Porcelain => porcelain_Barracuda,
            Skin.Rainbow => rainbow_Tuna,
            Skin.Sun => sun_Lionfish,
            Skin.Invert => invert_Seabass,
            _ => chameleon_Seabass
        };

        foreach (Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

        GameObject skinObject = neededRenderer.gameObject;
        skinObject.SetActive(true);
    }

    void SetDefaultSkin() {
        //purchasedSkins.Selected = Skin.Default;

        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        //if (currentData != null) {
        currentData.selectedSkin = /*purchasedSkins.Selected*/Skin.Default;
        ProgressSaver.SaveData(currentData);        //}

        UpdatePaidSkinsSection(purchasedSkins, currentData);
        UpdateFreeSkinsSection(currentData);
    }

    void UpdateFreeSkinsSection(AccountData data) {
        if (data == null)
            data = ProgressSaver.IsAccountDataExist ?
                ProgressSaver.LoadAccountData() : new();
        bool skinPurchasedBuffer;
        int price;

        foreach (Skin s in SkinsCards.FREE_SKINS_ITERATORS) {
            skinPurchasedBuffer = data.IsHasSkin(s);
            price = GameStaticInfo.GetFreeSkinPrice(s);
            skinsCards.GetSkinCard(s).UpdateData(data.selectedSkin == s, skinPurchasedBuffer, price.ToString());
        }
    }

    void UpdatePaidSkinsSection(PurchasedSkins purchasedSkins, AccountData data) {
        if (data == null)
            data = ProgressSaver.IsAccountDataExist ?
                   ProgressSaver.LoadAccountData() : new AccountData();

        this.purchasedSkins = purchasedSkins;//if fetch from purchase manager

        foreach (Skin s in SkinsCards.PURCHASE_SKINS_ITERATORS)
            skinsCards.GetSkinCard(s).ShowPriceUI(purchasedSkins.GetPrice(s));

        if (purchasedSkins.Chameleon)
            skinsCards.GetSkinCard(Skin.Chameleon).ShowSelectUI(data.selectedSkin == Skin.Chameleon);
        if (purchasedSkins.Mercury)
            skinsCards.GetSkinCard(Skin.Mercury).ShowSelectUI(data.selectedSkin == Skin.Mercury);
        if (purchasedSkins.Ashflow)
            skinsCards.GetSkinCard(Skin.Ashflow).ShowSelectUI(data.selectedSkin == Skin.Ashflow);


    }

    void SaveSelectedSkinToProgress(Skin skin, out AccountData data) {
        if (ProgressSaver.IsAccountDataExist) {
            data = ProgressSaver.LoadAccountData();

            if (data != null) {
                data.selectedSkin = skin;
                ProgressSaver.SaveData(data);
            }
        }
        else
            data = new();
    }

    public void OnSelectMercurySkinClick() {
        SaveSelectedSkinToProgress(Skin.Mercury, out AccountData data);
        //purchasedSkins.Selected = Skin.Mercury;
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectChameleonSkinClick() {
        SaveSelectedSkinToProgress(Skin.Chameleon, out AccountData data);
        //purchasedSkins.Selected = Skin.Chameleon;
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectAshflowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Ashflow, out AccountData data);
        //purchasedSkins.Selected = Skin.Ashflow;
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectZebraSkinClick() {
        SaveSelectedSkinToProgress(Skin.Zebra, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Zebra).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }

    public void OnSelectInvertSkinClick() {
        SaveSelectedSkinToProgress(Skin.Invert, out AccountData data);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectSingularitySkinClick() {
        SaveSelectedSkinToProgress(Skin.Singularity, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Singularity).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectBlinkerSkinClick() {
        SaveSelectedSkinToProgress(Skin.Blinker, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Blinker).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCheckerboardSkinClick() {
        SaveSelectedSkinToProgress(Skin.Checkerboard, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Checkerboard).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCorrosionSkinClick() {
        SaveSelectedSkinToProgress(Skin.Corrosion, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Corrosion).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCosmoSkinClick() {
        SaveSelectedSkinToProgress(Skin.Cosmo, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Cosmo).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCupidSkinClick() {
        SaveSelectedSkinToProgress(Skin.Cupid, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Cupid).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectFairylightSkinClick() {
        SaveSelectedSkinToProgress(Skin.Fairylight, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Fairylight).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectPixySkinClick() {
        SaveSelectedSkinToProgress(Skin.Pixy, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Pixy).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectPorcelainSkinClick() {
        SaveSelectedSkinToProgress(Skin.Porcelain, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Porcelain).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectRainbowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Rainbow, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Rainbow).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectSunSkinClick() {
        SaveSelectedSkinToProgress(Skin.Sun, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Sun).UpdateData(true);
        UpdatePaidSkinsSection(purchasedSkins, data);
        UpdateFreeSkinsSection(data);
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
        UpdatePaidSkinsSection(ps, null);
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
