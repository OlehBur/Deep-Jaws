#define UNITY_ANDROID 
#undef UNITY_EDITOR 
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using VContainer;



public class StoreWnd : WindowBase {
    [SerializeField] Button resetSkinsButton;
    [SerializeField] Button restorePurchaseButton;
    [SerializeField] Button previewSkinsButton;
    [SerializeField] Button exitStoreButton;

    [SerializeField] GameObject storeBackgroundLiveShadows;
    [SerializeField] SkinsCards skinsCards;
    [SerializeField] AccountsButtons accountsButtons;
    [SerializeField] SkinsPreviewWnd skinsPreviewWnd;

    [Inject] private readonly IGamePurchaseManager _gamePurchaseManager;
    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly ISanctuariesManager _sanctuariesManager;

    private PurchasedSkins _purchasedSkins;//buffer only for click listeners


    private void Start() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged += UpdateFreeSkinsBuyInteractable;
            UpdateFreeSkinsBuyInteractable(_sanctuariesManager.ShellsCnt);
        }

        InitButtonsListeners();
        UpdateFreeSkinsSection(null);
        UpdatePaidSkinsSection(_purchasedSkins, null);

        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= UpdateAccountSection;

            _gamePurchaseManager.OnUpdatePurchasedSkins += UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles += UpdateAccountSection;
        }
        skinsPreviewWnd.OnChangeStage += SkinsPreviewWndActivationHandler;
        OnChangeStage += storeBackgroundLiveShadows.SetActive;//auto enable live backgr

        PurchasedSkins ps = _gamePurchaseManager.GetCurrentPurchasedSkins();
        AccountTitles at = _gamePurchaseManager.GetCurrentAccountTitles();
        UpdateAccountSection(at);
        UpdatePaidSkinsSection(ps, null);
    }

    private void OnDestroy() {
        if (_sanctuariesManager != null)
            _sanctuariesManager.OnShellsAmountChanged -= UpdateFreeSkinsBuyInteractable;
        if (_gamePurchaseManager != null) {
            _gamePurchaseManager.OnUpdatePurchasedSkins -= UpdatePaidSkinsSection;
            _gamePurchaseManager.OnUpdateAccountTittles -= UpdateAccountSection;
        }
        if (skinsPreviewWnd != null)
            skinsPreviewWnd.OnChangeStage -= SkinsPreviewWndActivationHandler;
        OnChangeStage -= storeBackgroundLiveShadows.SetActive;
    }

    void SkinsPreviewWndActivationHandler(bool isOpen) => SetActivation(!isOpen);

    public void SetDefaultSkin() {
        AccountData currentData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        currentData.selectedSkin = Skin.Default;
        ProgressSaver.SaveData(currentData);

        UpdatePaidSkinsSection(_purchasedSkins, currentData);
        UpdateFreeSkinsSection(currentData);
    }

    public void OnSelectMercurySkinClick() => SelectSkinHandler(Skin.Mercury);
    public void OnSelectChameleonSkinClick() => SelectSkinHandler(Skin.Chameleon);
    public void OnSelectAshflowSkinClick() => SelectSkinHandler(Skin.Ashflow);
    public void OnSelectZebraSkinClick() => SelectSkinHandler(Skin.Zebra);

    public void OnSelectInvertSkinClick() => SelectSkinHandler(Skin.Invert);
    public void OnSelectSingularitySkinClick() => SelectSkinHandler(Skin.Singularity);
    public void OnSelectBlinkerSkinClick() => SelectSkinHandler(Skin.Blinker);
    public void OnSelectCheckerboardSkinClick() => SelectSkinHandler(Skin.Checkerboard);
    public void OnSelectCorrosionSkinClick() => SelectSkinHandler(Skin.Corrosion);
    public void OnSelectCosmoSkinClick() => SelectSkinHandler(Skin.Cosmo);
    public void OnSelectCupidSkinClick() => SelectSkinHandler(Skin.Cupid);
    public void OnSelectFairylightSkinClick() => SelectSkinHandler(Skin.Fairylight);
    public void OnSelectPixySkinClick() => SelectSkinHandler(Skin.Pixy);
    public void OnSelectPorcelainSkinClick() => SelectSkinHandler(Skin.Porcelain);
    public void OnSelectRainbowSkinClick() => SelectSkinHandler(Skin.Rainbow);
    public void OnSelectSunSkinClick() => SelectSkinHandler(Skin.Sun);

    void SelectSkinHandler(Skin type) {
        SaveSelectedSkinToProgress(type, out AccountData data);
        UpdatePaidSkinsSection(_purchasedSkins, data);
        UpdateFreeSkinsSection(data);
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
#if UNITY_ANDROID && !UNITY_EDITOR
        if (data == null)
            data = ProgressSaver.IsAccountDataExist ?
                   ProgressSaver.LoadAccountData() : new AccountData();

        _purchasedSkins = purchasedSkins;//if fetch from purchase manager

        foreach (Skin s in SkinsCards.PURCHASE_SKINS_ITERATORS)
            skinsCards.GetSkinCard(s).SetAndShowPrice(purchasedSkins.GetPrice(s));

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

#if UNITY_ANDROID && !UNITY_EDITOR
    void OpenPreviewWithCurrentSkin(Skin skin) {
        skinsPreviewWnd.SetVisualizeSkin(skin);
        skinsPreviewWnd.OnOpen();
    }

    void UpdateFreeSkinsBuyInteractable(int cntShells) {
        bool enoughShellsBuff = false;
        foreach (Skin s in SkinsCards.FREE_SKINS_ITERATORS) {
            enoughShellsBuff = GameStaticInfo.GetFreeSkinPrice(s) <= cntShells;
            skinsCards.GetSkinCard(s).SetBuyInterctable(enoughShellsBuff);
        }
    }

    void InitButtonsListeners() {
        //skins
        skinsCards.InitListeners(_gamePurchaseManager);
        foreach (Skin s in SkinsCards.ALL_SKINS_ITETATORS)
            skinsCards.GetSkinCard(s).PreviewOnClick.AddListener(() => OpenPreviewWithCurrentSkin(s));
        //Debug.LogWarning("Todo init view buttons for paid skins");

        //accounts
        accountsButtons.InitListeners(_gamePurchaseManager);

        //main
        resetSkinsButton.onClick.RemoveAllListeners();
        restorePurchaseButton.onClick.RemoveAllListeners();
        previewSkinsButton.onClick.RemoveAllListeners();
        exitStoreButton.onClick.RemoveAllListeners();

        resetSkinsButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        restorePurchaseButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        previewSkinsButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        exitStoreButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        resetSkinsButton.onClick.AddListener(SetDefaultSkin);
        restorePurchaseButton.onClick.AddListener(_gamePurchaseManager.OnRestorePurchases);
        previewSkinsButton.onClick.AddListener(() => OpenPreviewWithCurrentSkin(Skin.Default));
        exitStoreButton.onClick.AddListener(OnClose);
    }

    void UpdateAccountSection(AccountTitles accountTitles) {
        StoreCardView cardBuffer = null;
        foreach (AccountPurchase ap in AccountsButtons.ACCOUNT_PURCH_ITERATORS) {
            cardBuffer = accountsButtons.GetStoreCard(ap);

            cardBuffer.SetBuyInterctable(true);
            cardBuffer.SetAndShowPrice(accountTitles.GetPrice(ap));
        }

        if (accountTitles.HasNoAd)
            BuyedAccountPurchUIBeh(AccountPurchase.NoAd);
        if (accountTitles.HasPremium)
            BuyedAccountPurchUIBeh(AccountPurchase.Premium);
        //support dev can buyed multiple times
    }

    void BuyedAccountPurchUIBeh(AccountPurchase ap) {
        StoreCardView cardBuffer = accountsButtons.GetStoreCard(ap);

        cardBuffer.SetBuyInterctable(false);//buy disable
        cardBuffer.SetAndShowPrice(LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_purchased"));//AccountsButtons.PURCHASED_TEXT;
    }
#endif
}
