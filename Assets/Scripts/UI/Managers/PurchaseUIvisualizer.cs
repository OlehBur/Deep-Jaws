//#define UNITY_ANDROID 
//#undef UNITY_EDITOR 

using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class PurchaseUIvisualizer : MonoBehaviour {
    [SerializeField] FishSkinInfo fishSkinManager;
    [SerializeField] AccountsButtons accountsButtons;
    [SerializeField] SkinsCards skinsCards;
    [SerializeField] GameObject storeWnd;
    [SerializeField] Button resetSkinsButton;
    [SerializeField] Button restorePurchaseButton;
    [SerializeField] GameObject loyaltyRewardedWnd;
    [SerializeField] Animator titleAdFree;
    [SerializeField] Animator titlePremium;
    [SerializeField] Animator titleFounder;
    [SerializeField] Animator titleDemo;
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

    [Inject] private readonly IGamePurchaseManager _gamePurchaseManager;
    [Inject] private readonly ISanctuariesManager _sanctuariesManager;

    //private MaterialPropertyBlock propBlock;
    private PurchasedSkins _purchasedSkins;//buffer only for click listeners
    private int _selectedSkinOnPreview = 0;

    //private const string PREF_LOYALTYCONFIRMED_KEY = "loyaltyWndConfirm";
    //private const bool SELECT_BUTTON_TEXT_ID = true;
    //private const bool BUY_BUTTON_TEXT_ID = false;
    private const float SKINS_PREVIEW_ROTATION_SPEED = 8f;


    private void Start() {
#if UNITY_ANDROID && !UNITY_EDITOR
        UpdateFreeSkinsSection(null);
        UpdatePaidSkinsSection(_purchasedSkins, null);

        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= UpdateAccountSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= InitMainMenuAccountTittles;

            _gamePurchaseManager.OnUpdatePurchasedSkins += UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles += UpdateAccountSection;
            _gamePurchaseManager.OnUpdateAccountTittles += InitMainMenuAccountTittles;
            _gamePurchaseManager.OnNonInternetConnection += OpenNoInternetWnd;
            LocalLoadTitlesAndSkins();
        }

        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged += UpdateFreeSkinsBuyInteractable;
            UpdateFreeSkinsBuyInteractable(_sanctuariesManager.ShellsCnt);
        }

        InitPreviewSkins();
        InitButtonsListeners();
        _gamePurchaseManager?.RefreshUI();
#elif UNITY_STANDALONE_WIN
        titleAdFree.gameObject.SetActive(false);
        titlePremium.gameObject.SetActive(false);
        titleFounder.gameObject.SetActive(false);
        titleDemo.gameObject.SetActive(true);

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;
        titleDemo.enabled = true;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnDisable() {
        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= UpdateAccountSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= InitMainMenuAccountTittles;
            _gamePurchaseManager.OnNonInternetConnection -= OpenNoInternetWnd;
        }

        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged -= UpdateFreeSkinsBuyInteractable;
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
            skinsPreview.Rotate(Vector3.up, DJTime.UnscaledDeltaTime * SKINS_PREVIEW_ROTATION_SPEED);
    }
#endif

    public void OnOpen_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(true);
        skinsPreviewWnd.SetActive(true);

        foreach (Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

        _selectedSkinOnPreview = 0;
        skinsPreview.GetChild(_selectedSkinOnPreview).gameObject.SetActive(true);
    }

    public void OnClose_ScinsPreviewWnd() {
        skinsPreview.gameObject.SetActive(false);
        skinsPreviewWnd.SetActive(false);
    }

    public void OnNextSkinPreview() {
        //skinsPreview.GetChild(selectedSkinOnPreview).gameObject.SetActive(false);
        foreach (Transform t in skinsPreview.transform)
            t.gameObject.SetActive(false);

        ++_selectedSkinOnPreview;

        _selectedSkinOnPreview = skinsPreview.childCount > _selectedSkinOnPreview ?
            _selectedSkinOnPreview :
            0;

        skinsPreview.GetChild(_selectedSkinOnPreview).gameObject.SetActive(true);
    }

    public void OpenNoInternetWnd(/*bool isActivate*/) {
        NoInternetConnectionWnd.SetActive(true);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void InitButtonsListeners() {
        //if (skinsButtons.rainbow.buyB == null) {
        //    Debug.LogWarning("Skin buttons = null");
        //    return;
        //}

        //skins
        skinsCards.InitListeners(_gamePurchaseManager);

        //accounts
        accountsButtons.buyNoAd.onClick.AddListener(_gamePurchaseManager.OnBuy_NoAd);
        accountsButtons.buyPremium.onClick.AddListener(_gamePurchaseManager.OnBuy_Premium);
        accountsButtons.buySupportDev.onClick.AddListener(_gamePurchaseManager.OnBuy_SupportDev);
        //other
        resetSkinsButton.onClick.AddListener(SetDefaultSkin);
        restorePurchaseButton.onClick.AddListener(_gamePurchaseManager.OnRestorePurchases);
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

        UpdatePaidSkinsSection(_purchasedSkins, currentData);
        UpdateFreeSkinsSection(currentData);
    }
#endif

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
#if UNITY_ANDROID && !UNITY_EDITOR
        if (data == null)
            data = ProgressSaver.IsAccountDataExist ?
                   ProgressSaver.LoadAccountData() : new AccountData();

        this._purchasedSkins = purchasedSkins;//if fetch from purchase manager

        foreach (Skin s in SkinsCards.PURCHASE_SKINS_ITERATORS)
            skinsCards.GetSkinCard(s).ShowPriceUI(purchasedSkins.GetPrice(s));

        if (purchasedSkins.Chameleon)
            skinsCards.GetSkinCard(Skin.Chameleon).ShowSelectUI(data.selectedSkin == Skin.Chameleon);
        if (purchasedSkins.Mercury)
            skinsCards.GetSkinCard(Skin.Mercury).ShowSelectUI(data.selectedSkin == Skin.Mercury);
        if (purchasedSkins.Ashflow)
            skinsCards.GetSkinCard(Skin.Ashflow).ShowSelectUI(data.selectedSkin == Skin.Ashflow);
#endif
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
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectChameleonSkinClick() {
        SaveSelectedSkinToProgress(Skin.Chameleon, out AccountData data);
        //purchasedSkins.Selected = Skin.Chameleon;
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectAshflowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Ashflow, out AccountData data);
        //purchasedSkins.Selected = Skin.Ashflow;
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectZebraSkinClick() {
        SaveSelectedSkinToProgress(Skin.Zebra, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Zebra).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }

    public void OnSelectInvertSkinClick() {
        SaveSelectedSkinToProgress(Skin.Invert, out AccountData data);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectSingularitySkinClick() {
        SaveSelectedSkinToProgress(Skin.Singularity, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Singularity).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectBlinkerSkinClick() {
        SaveSelectedSkinToProgress(Skin.Blinker, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Blinker).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCheckerboardSkinClick() {
        SaveSelectedSkinToProgress(Skin.Checkerboard, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Checkerboard).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCorrosionSkinClick() {
        SaveSelectedSkinToProgress(Skin.Corrosion, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Corrosion).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCosmoSkinClick() {
        SaveSelectedSkinToProgress(Skin.Cosmo, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Cosmo).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectCupidSkinClick() {
        SaveSelectedSkinToProgress(Skin.Cupid, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Cupid).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectFairylightSkinClick() {
        SaveSelectedSkinToProgress(Skin.Fairylight, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Fairylight).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectPixySkinClick() {
        SaveSelectedSkinToProgress(Skin.Pixy, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Pixy).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectPorcelainSkinClick() {
        SaveSelectedSkinToProgress(Skin.Porcelain, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Porcelain).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectRainbowSkinClick() {
        SaveSelectedSkinToProgress(Skin.Rainbow, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Rainbow).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }
    public void OnSelectSunSkinClick() {
        SaveSelectedSkinToProgress(Skin.Sun, out AccountData data);
        //skinsCards.GetSkinCard(Skin.Sun).UpdateData(true);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
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
        AccountTitles at = _gamePurchaseManager.GetCurrentAccountTitles();
        PurchasedSkins ps = _gamePurchaseManager.GetCurrentPurchasedSkins();

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
        titleDemo.gameObject.SetActive(false);

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;
        titleDemo.enabled = false;

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
#endif
}
