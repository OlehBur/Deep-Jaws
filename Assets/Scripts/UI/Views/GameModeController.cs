using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public interface IGameModeController {
    void OnGameModeSet(int gamemode);
    void ActivateGamemodesWnd(bool isActivate);
}

public class GameModeController : MonoBehaviour, IGameModeController {
    [SerializeField] GameObject gamemodeWnd;
    //[SerializeField] Light mainMenuLight;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] Image gameModeWndBackground;

    [Inject] private readonly IGraphicManager _graphicManager;
    [Inject] private /*readonly*/ IAlertWindowsManager _alertWindowsManager;

    public static GameMode CurrentMode { get; set; } = GameMode.None;

    private bool _isGMneedAnimation = true;
    private Color _targetGMwndColor;
    private Color _targetLightGMcolor;

    [Inject]
    private void Construct(IAlertWindowsManager alertWindowsManager) {
        _alertWindowsManager = alertWindowsManager;

        _alertWindowsManager.OnNewGameConfirmTrigger += NewGameTriggerHandler;
    }

    void OnDestroy() {
        if (_alertWindowsManager != null)
            _alertWindowsManager.OnNewGameConfirmTrigger -= NewGameTriggerHandler;
    }

    void NewGameTriggerHandler() => ActivateGamemodesWnd(true);

    public void ActivateGamemodesWnd(bool isActivate) => gamemodeWnd.SetActive(true);


    IEnumerator ChangeColorGMwnd() {
        //Debug.Log("gm changing color started");
        while (gameModeWndBackground.color != _targetGMwndColor) {
            gameModeWndBackground.color = Color.Lerp(gameModeWndBackground.color, _targetGMwndColor, DJTime.DeltaTime);
            yield return null;
        }

        //Debug.Log("gm Color changed");
    }

    IEnumerator ChangeColorGMmainMenu() {
        while (_graphicManager.MainLightColor != _targetLightGMcolor) {
            _graphicManager.MainLightColor = Color.Lerp(_graphicManager.MainLightColor, _targetLightGMcolor, DJTime.DeltaTime);
            yield return null;
        }

        Debug.LogWarning("TODO: call OnGameStartLoad() after extit fr gamemode wnd");
    }


    public void OnGameModeSet(int gamemode) {
        CurrentMode = (GameMode)gamemode;

        _targetGMwndColor = CurrentMode switch {
            GameMode.Easy => GameStaticInfo.EASY_GM_COLOR,
            GameMode.Medium => GameStaticInfo.MEDIUM_GM_COLOR,
            GameMode.Hard => GameStaticInfo.HARD_GM_COLOR,
            _ => Color.white
        };

        _targetLightGMcolor = CurrentMode switch {
            GameMode.Easy => GameStaticInfo.LIGHT_DEFAULT_COLOR,
            GameMode.Medium => GameStaticInfo.LIGHT_MEDIUM_COLOR,
            GameMode.Hard => GameStaticInfo.LIGHT_HARD_COLOR,
            _ => Color.white
        };

        StopCoroutine(ChangeColorGMwnd());
        StartCoroutine(ChangeColorGMwnd());
        StopCoroutine(ChangeColorGMmainMenu());
        StartCoroutine(ChangeColorGMmainMenu());

        //PlayerPrefs.SetInt("GameMode", gamemode);
        //Debug.Log("DELETED WORDL AUTO SAVE");

        if (_isGMneedAnimation) {
            gmWndAnimator.SetTrigger("Anim");
            _isGMneedAnimation = false;
        }
    }

    public void OnGameStartLoad() {
        _graphicManager.ResetLightColor();
    }
}
