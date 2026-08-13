using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SkinStoreCard : MonoBehaviour {
    [SerializeField] Button buyB;
    [SerializeField] Button selectB;
    [SerializeField] Button previewB;
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] TextMeshProUGUI selectTxt;
    [SerializeField] LocalizedString SELECTED_TXT;
    [SerializeField] LocalizedString SELECT_TXT;

    public Button.ButtonClickedEvent BuyOnClick => buyB.onClick;
    public Button.ButtonClickedEvent PreviewOnClick => previewB.onClick;

    //private static string SELECTED_TXT => LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_selected");
    //private static string SELECT_TXT => LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_select");


    public void SetBuyInterctable(bool interactable) => buyB.interactable = interactable;

    public void ShowPriceUI(string price) {
        if (buyB == null)
            return;

        priceTxt.text = price;
        buyB.gameObject.SetActive(true);
        selectB.gameObject.SetActive(false);
    }

    public void ShowSelectUI(bool isSelected = false) {
        //if (SELECTED_TXT == null) {//not inited
        //SELECTED_TXT = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_selected");
        //SELECT_TXT = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "store_select");
        //}
        if(buyB==null)
            return;

        selectTxt.text = isSelected ?
            SELECTED_TXT.GetLocalizedString() : SELECT_TXT.GetLocalizedString();
        selectB.interactable = !isSelected;

        buyB.gameObject.SetActive(false);
        selectB.gameObject.SetActive(true);
    }

    public void UpdateData(bool isSelected, bool isPurchased = true, string price = "") {
        if (!isPurchased)
            ShowPriceUI(price);
        else
            ShowSelectUI(isSelected);
    }
}