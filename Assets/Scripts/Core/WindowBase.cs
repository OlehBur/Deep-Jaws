using System;
using UnityEngine;

public abstract class WindowBase : MonoBehaviour, IWindow {
    public event Action<bool> OnChangeStage;

    public void OnOpen() {
        gameObject.SetActive(true);
        OnChangeStage?.Invoke(true);
        OpenInternal();
    }

    public void OnClose() {
        CloseInternal();
        gameObject.SetActive(false);
        OnChangeStage?.Invoke(false);
    }

    public void SetActivation(bool isActive) {
        gameObject.SetActive(isActive);
        OnChangeStage?.Invoke(isActive);
    }

    protected virtual void OpenInternal() { }
    protected virtual void CloseInternal() { }
}
