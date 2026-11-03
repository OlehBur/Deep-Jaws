using System;
using UnityEngine;
using VContainer;


public interface IAlertWindowsManager {
    event Action<bool> OnAlerWndOpened;

    bool IsOpenedAnyAlertWnd { get; }
    void ActivateLoadingPanelWnd(bool isActive);
    void ActivateAreUSureWnd(bool isActive);
    void ActivateNoInternetConnectionWnd(bool isActive);
    void ActivateLicenseFailedWnd(bool isActive);
    void ActivateSettingsWnd(bool isActive);
    void FirstNoConnectionOpen();
}

public enum AlertWndType {
    AreUSure,
    Loading,
    NoInternetConnection,
    NewUpdate,
    LicenseFailed,
    Settings
}

public class AlertWindowsManager : MonoBehaviour, IAlertWindowsManager {
    [SerializeField] GameObject areUSureWnd;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] GameObject newUpdWnd;
    [SerializeField] GameObject licenseFailedPanel;
    [SerializeField] GameObject settingsWnd;

    public event Action<bool> OnAlerWndOpened;
    public bool IsOpenedAnyAlertWnd => areUSureWnd.activeInHierarchy 
        || loadingPanel.activeInHierarchy 
        || noInternetConnectionWnd.activeInHierarchy 
        || newUpdWnd.activeInHierarchy 
        || licenseFailedPanel.activeInHierarchy 
        || settingsWnd.activeInHierarchy;


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
        ActivateAlertWndHandler(AlertWndType.Loading, isActive);

    public void ActivateAreUSureWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.AreUSure, isActive);

    public void ActivateLicenseFailedWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.LicenseFailed, isActive);

    public void ActivateSettingsWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.Settings, isActive);

    void ActivateAlertWndHandler(AlertWndType type, bool isActive) {
        GameObject currentWnd = type switch {
            AlertWndType.AreUSure => areUSureWnd,
            AlertWndType.Loading => loadingPanel,
            AlertWndType.NoInternetConnection => noInternetConnectionWnd,
            AlertWndType.NewUpdate => newUpdWnd,
            AlertWndType.LicenseFailed => licenseFailedPanel,
            AlertWndType.Settings => settingsWnd,
            _ => null
        };

        currentWnd.SetActive(isActive);
        OnAlerWndOpened?.Invoke(IsOpenedAnyAlertWnd);
    }


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

