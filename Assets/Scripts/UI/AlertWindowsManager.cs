using UnityEngine;
using VContainer;


public interface IAlertWindowsManager {
    void ActivateLoadingPanelWnd(bool isActive);
    void ActivateAreUSureWnd(bool isActive);
    void ActivateNoInternetConnectionWnd(bool isActive);
    void ActivateLicenseFailedWnd(bool isActive);
    void FirstNoConnectionOpen();
}

public class AlertWindowsManager : MonoBehaviour {
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] GameObject newUpdWnd;
    [SerializeField] GameObject licenseFailedPanel;


    private void Awake() {
        AccountData accountData = ProgressSaver.IsAccountDataExist ?
    ProgressSaver.LoadAccountData() : new AccountData();

        if (accountData.firstUpdOpen1_7 && newUpdWnd != null) {
            Debug.Log("Confirmed first open on New Update 1.7");
            newUpdWnd.SetActive(true);
            accountData.firstUpdOpen1_7 = false;
            ProgressSaver.SaveData(accountData);
        }

    }


    public void ActivateLoadingPanelWnd(bool isActive) =>
        loadingPanel.SetActive(isActive);

    public void ActivateAreUSureWnd(bool isActive) =>
        areUSureWnd.SetActive(isActive);

    public void ActivateLicenseFailedPanel(bool isActive) =>
        licenseFailedPanel.SetActive(isActive);


    public void ActivateNoInternetConnectionWnd(bool isActive) {
#if UNITY_ANDROID
        if (noInternetConnectionWnd != null)
            noInternetConnectionWnd.SetActive(isActive);
#endif
    }

    public void FirstNoConnectionOpen() {
        AccountData accountData = ProgressSaver.IsAccountDataExist ?
            ProgressSaver.LoadAccountData() : new AccountData();

        if (accountData.isConfirmedNoInternetDisadvantages)
            return;

        ActivateNoInternetConnectionWnd(true);
        accountData.isConfirmedNoInternetDisadvantages = true;
        ProgressSaver.SaveData(accountData);
    }


}

