using System;

public interface IWindow {
    event Action<bool> OnChangeStage;

    void OnOpen();
    void OnClose();
}