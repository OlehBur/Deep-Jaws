using System;
using UnityEngine;
using VContainer;


public interface IAlertWindowsManager {
    event Action<bool> OnAlerWndOpened;
    //event Action OnNewGameConfirmTrigger;
    //event Action OnSureExitGameTrigger;

    //int GetLegthNewGameTrigger();
    bool IsOpenedAnyAlertWnd { get; }
    void ActivateLoadingPanelWnd(bool isActive);
    void ActivateAreUSureNewGameWnd(bool isActive, Action onConfirm);
    void ActivateAreUSureExitGameWnd(bool isActive, Action onConfirm);
    void ActivateNoInternetConnectionWnd(bool isActive);
    void ActivateLicenseFailedWnd(bool isActive);
    void ActivateSettingsWnd(bool isActive);
    void ActivateGuidebookWnd(bool isActive);
    void ActivateDemoWnd(bool isActive);
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
    Guidebook,
    Demo
}

public class AlertWindowsManager : MonoBehaviour, IAlertWindowsManager {
    [SerializeField] GameObject areUSureNewGameWnd;
    [SerializeField] GameObject areUSureExitGameWnd;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject noInternetConnectionWnd;
    [SerializeField] GameObject newUpdWnd;
    [SerializeField] GameObject licenseFailedPanel;
    [SerializeField] GameObject guideBookWnd;

    [SerializeField] SettingsWnd settingsWnd;
    [SerializeField] DemoFinWnd demoFinWnd;

    public event Action<bool> OnAlerWndOpened;
    //public event Action OnNewGameConfirmTrigger;
    //public event Action OnSureExitGameTrigger;

    public bool IsOpenedAnyAlertWnd => areUSureNewGameWnd.activeInHierarchy
        || areUSureExitGameWnd.activeInHierarchy
        || loadingPanel.activeInHierarchy
        || noInternetConnectionWnd.activeInHierarchy
        || newUpdWnd.activeInHierarchy
        || licenseFailedPanel.activeInHierarchy
        || settingsWnd.isActiveAndEnabled
        || demoFinWnd.isActiveAndEnabled
        || guideBookWnd.activeInHierarchy;

    private Action _confirmNewGameAction;
    private Action _confirmExitGameAction;


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

    //public int GetLegthNewGameTrigger() {
    //    return OnNewGameConfirmTrigger?.GetInvocationList().Length ?? -1;
    //}

    public void NewGameConfirmTrigger() {
        //Debug.Log(OnNewGameConfirmTrigger?.GetInvocationList().Length);
        //OnNewGameConfirmTrigger?.Invoke();
        _confirmNewGameAction?.Invoke();
        _confirmNewGameAction = null;
    }
    public void SaveExitGameTrigger() {
        //OnSureExitGameTrigger?.Invoke();
        _confirmExitGameAction?.Invoke();
        _confirmExitGameAction = null;
    }

    public void ActivateAreUSureNewGameWnd(bool isActive, Action onConfirm) {
        if (isActive)
            _confirmNewGameAction = onConfirm;

        ActivateAlertWndHandler(AlertWndType.AreUSureNew, isActive);
    }

    public void ActivateAreUSureExitGameWnd(bool isActive, Action onConfirm) {
        if (isActive)
            _confirmExitGameAction = onConfirm;

        ActivateAlertWndHandler(AlertWndType.AreUSureExit, isActive);
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
    public void ActivateDemoWnd(bool isActive) =>
        ActivateAlertWndHandler(AlertWndType.Demo, isActive);

    void ActivateAlertWndHandler(AlertWndType type, bool isActive) {
        GameObject currentWnd = type switch {
            AlertWndType.AreUSureNew => areUSureNewGameWnd,
            AlertWndType.AreUSureExit => areUSureExitGameWnd,
            AlertWndType.Loading => loadingPanel,
            AlertWndType.NoInternetConnection => noInternetConnectionWnd,
            AlertWndType.NewUpdate => newUpdWnd,
            AlertWndType.LicenseFailed => licenseFailedPanel,
            AlertWndType.Guidebook => guideBookWnd,
            _ => null
        };
        if (currentWnd != null) {
            currentWnd.SetActive(isActive);
        }
        else {
            WindowBase window = type switch {
                AlertWndType.Settings => settingsWnd,
                AlertWndType.Demo => demoFinWnd,
                _ => null
            };
            window.SetActivation(isActive);
        }

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

