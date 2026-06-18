using UnityEngine;


public class PurchaseUIvisualizer : MonoBehaviour {
    [SerializeField] GameObject loyaltyRewardedWnd;
    [SerializeField] Animator titleAdFree;
    [SerializeField] Animator titlePremium;
    [SerializeField] Animator titleFounder;

    private const string PREF_LOYALTYCONFIRMED_KEY = "loyaltyWndConfirm";

    private void Start() {
        GamePurchaseManager.Instance.OnUpdateAccountTittles += InitAccountTittles;
    }

    void OpenLoyaltyRewardedWnd() {
        bool confirmedLoyalty = PlayerPrefs.GetInt(PREF_LOYALTYCONFIRMED_KEY, 0) == 1;

        loyaltyRewardedWnd.SetActive(true);
        PlayerPrefs.SetInt(PREF_LOYALTYCONFIRMED_KEY, 1);
    }

    private void InitAccountTittles(bool isNoAdd, bool isPremium, bool isFounder) {
        if (titleAdFree == null)
            return;

        titleAdFree.gameObject.SetActive(false);
        titlePremium.gameObject.SetActive(false);
        titleFounder.gameObject.SetActive(false);

        titleAdFree.enabled = false;
        titlePremium.enabled = false;
        titleFounder.enabled = false;

        if (isFounder) {
            titleFounder.gameObject.SetActive(true);
            titleFounder.enabled = true;
            OpenLoyaltyRewardedWnd();
        }
        else if (isPremium) {
            titlePremium.gameObject.SetActive(true);
            titlePremium.enabled = true;
        }
        else if (isNoAdd) {
            titleAdFree.gameObject.SetActive(true);
            titleAdFree.enabled = true;
        }
    }

    //[SerializeField] bool nd;
    //[SerializeField] bool pr;
    //[SerializeField] bool fn;
    //private void OnValidate() {
    //    Debug.LogWarning("Activated account titles preview mode");

    //    InitAccountTittles(nd, pr, fn);
    //}
}
