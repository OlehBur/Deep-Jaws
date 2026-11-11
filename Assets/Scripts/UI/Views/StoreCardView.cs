using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;

public class StoreCardView : MonoBehaviour {
    [SerializeField] Button buyB;
    [SerializeField] Button selectB;
    [SerializeField] Button previewB;
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] TextMeshProUGUI selectTxt;

    //[Inject] private readonly UILocalizationInfo _localizationInfo;

    public Button.ButtonClickedEvent BuyOnClick => buyB.onClick;
    public Button.ButtonClickedEvent PreviewOnClick => previewB.onClick;
    public Button.ButtonClickedEvent SelectOnClick => selectB.onClick;

    private UILocalizationInfo _localizationInfo;

    public void InitDI(UILocalizationInfo localizationInfo) {
        _localizationInfo = localizationInfo;
    }

    private void OnEnable() {
        if (_localizationInfo == null)
            return;

        _localizationInfo.SelectedTxt.StringChanged += OnStringUpdated;
        _localizationInfo.SelectTxt.StringChanged += OnStringUpdated;
    }

    private void OnDisable() {
        if (_localizationInfo == null)
            return;

        _localizationInfo.SelectedTxt.StringChanged -= OnStringUpdated;
        _localizationInfo.SelectTxt.StringChanged -= OnStringUpdated;
    }
    public void SetBuyInterctable(bool interactable) => buyB.interactable = interactable;

    public void SetAndShowPrice(string price) {
        if (buyB == null)
            return;

        priceTxt.text = price;
        buyB.gameObject.SetActive(true);
        selectB.gameObject.SetActive(false);
    }

    public void ShowSelectUI(bool isSelected = false) {
        if (buyB == null)
            return;

        LocalizedString currentStr = isSelected ? 
            _localizationInfo.SelectedTxt :
            _localizationInfo.SelectTxt;
        currentStr.RefreshString();//force text upd
        selectTxt.text = currentStr.GetLocalizedString();

        selectB.interactable = !isSelected;

        buyB.gameObject.SetActive(false);
        selectB.gameObject.SetActive(true);
    }

    private void OnStringUpdated(string translatedValue) {
        selectTxt.text = selectB.interactable ?
            _localizationInfo.SelectTxt.GetLocalizedString() :
            _localizationInfo.SelectedTxt.GetLocalizedString();
    }

    public void UpdateData(bool isSelected, bool isPurchased = true, string price = "") {
        if (!isPurchased)
            SetAndShowPrice(price);
        else
            ShowSelectUI(isSelected);
    }
}