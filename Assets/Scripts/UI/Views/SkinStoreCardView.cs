using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SkinStoreCardView : MonoBehaviour {
    [SerializeField] Button buyB;
    [SerializeField] Button selectB;
    [SerializeField] Button previewB;
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] TextMeshProUGUI selectTxt;
    [SerializeField] LocalizedString SELECTED_TXT;
    [SerializeField] LocalizedString SELECT_TXT;

    public Button.ButtonClickedEvent BuyOnClick => buyB.onClick;
    public Button.ButtonClickedEvent PreviewOnClick => previewB.onClick;



    private void OnEnable() {
        SELECTED_TXT.StringChanged += OnStringUpdated;
        SELECT_TXT.StringChanged += OnStringUpdated;
    }

    private void OnDisable() {
        SELECTED_TXT.StringChanged -= OnStringUpdated;
        SELECT_TXT.StringChanged -= OnStringUpdated;
    }
    public void SetBuyInterctable(bool interactable) => buyB.interactable = interactable;

    public void ShowPriceUI(string price) {
        if (buyB == null)
            return;

        priceTxt.text = price;
        buyB.gameObject.SetActive(true);
        selectB.gameObject.SetActive(false);
    }

    public void ShowSelectUI(bool isSelected = false) {
        if (buyB == null)
            return;

        LocalizedString currentStr = isSelected ? SELECTED_TXT : SELECT_TXT;
        currentStr.RefreshString();//force text upd

        selectB.interactable = !isSelected;

        buyB.gameObject.SetActive(false);
        selectB.gameObject.SetActive(true);
    }

    private void OnStringUpdated(string translatedValue) {
        selectTxt.text = selectB.interactable ?
            SELECT_TXT.GetLocalizedString() : SELECTED_TXT.GetLocalizedString();
    }

    public void UpdateData(bool isSelected, bool isPurchased = true, string price = "") {
        if (!isPurchased)
            ShowPriceUI(price);
        else
            ShowSelectUI(isSelected);
    }
}