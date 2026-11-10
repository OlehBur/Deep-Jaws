using System;
using UnityEngine;
using VContainer;


public interface IAlertWindowsManager {
    event Action<bool> OnAlerWndOpened;

    bool IsOpenedAnyAlertWnd { get; }
    void ActivateLoadingPanelWnd(bool isActive);
    void ActivateAreUSureNewGameWnd(bool isActive);
    void ActivateAreUSureExitGameWnd(bool isActive);
    void ActivateNoInternetConnectionWnd(bool isActive);
    void ActivateLicenseFailedWnd(bool isActive);
    void ActivateSettingsWnd(bool isActive);
    void ActivateGuidebookWnd(bool isActive);
    void FirstNoConnectionOpen();
}

public enum AlertWndType {
    AreUSureNew,
    AreUSureExit,
    Loading,
    NoInternetConnection,
    NewUpdate,
    LicenseFailed,
    Settings,
    Guidebook
}

public class AlertWindowsManager : MonoBehaviour, IAlertWindowsManager {
    [SerializeField] GameObject areUSureNewGameWnd;
    [SerializeField] GameObject areUSureExitGameWnd;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] GameObject newUpdWnd;
    [SerializeField] GameObject licenseFailedPanel;
    [SerializeField] SettingsWnd settingsWnd;
    [SerializeField] GameObject guideBookWnd;

    public event Action<bool> OnAlerWndOpened;
    public bool IsOpenedAnyAlertWnd => areUSureNewGameWnd.activeInHierarchy 
        || areUSureExitGameWnd.activeInHierarchy 
        || loadingPanel.activeInHierarchy 
        || noInternetConnectionWnd.activeInHierarchy 
        || newUpdWnd.activeInHierarchy 
        || licenseFailedPanel.activeInHierarchy 
        || settingsWnd.isActiveAndEnabled
        || guideBookWnd.activeInHierarchy;


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

    public void ActivateAreUSureNewGameWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.AreUSureNew, isActive);
    public void ActivateAreUSureExitGameWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.AreUSureExit, isActive);

    public void ActivateLicenseFailedWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.LicenseFailed, isActive);

    public void ActivateSettingsWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.Settings, isActive);

    public void ActivateGuidebookWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.Guidebook, isActive);

    void ActivateAlertWndHandler(AlertWndType type, bool isActive) {
        GameObject currentWnd = type switch {
            AlertWndType.AreUSureNew => areUSureNewGameWnd,
            AlertWndType.AreUSureExit => areUSureExitGameWnd,
            AlertWndType.Loading => loadingPanel,
            AlertWndType.NoInternetConnection => noInternetConnectionWnd,
            AlertWndType.NewUpdate => newUpdWnd,
            AlertWndType.LicenseFailed => licenseFailedPanel,
            AlertWndType.Settings => settingsWnd.gameObject,
            AlertWndType.Guidebook => guideBookWnd,
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

