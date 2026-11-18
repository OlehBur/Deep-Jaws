using System;

public interface IAlertWindowsManager {
    event Action<bool> OnAlerWndOpened;

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
