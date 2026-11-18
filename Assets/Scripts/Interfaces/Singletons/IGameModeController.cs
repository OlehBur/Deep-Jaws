
public interface IGameModeController {
    WindowBase Window { get; }
    void OnGameModeSet(int gamemode);
    void ActivateGamemodesWnd(bool isActivate);
}