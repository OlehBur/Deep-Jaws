using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;


public interface IGameModeController {
    void OnGameModeSet(int gamemode);
    void ChangeSignInColor(bool isSignIn);
    void ActivateGamemodesWnd(bool isActivate);
}

public class GameModeController : MonoBehaviour, IGameModeController {
    [SerializeField] GameObject gamemodeWnd;
    [SerializeField] Light mainMenuLight;
    [SerializeField] Animator gmWndAnimator;
    [SerializeField] Image gameModeWndBackground;

    [SerializeField] Image signInIcon;
    [SerializeField] GameObject kingOfDeepsObj;
    [SerializeField] TextMeshProUGUI kingOfDeepsTxt;
    [SerializeField] LocalizedString kingOfDeepsLocalized;
    [SerializeField] Animator kingOfDeepsAnimator;

    public static GameMode CurrentMode { get; set; } = GameMode.None;

    private bool _isGMneedAnimation = true;
    private Color _targetGMwndColor;
    private Color _targetLightGMcolor;


    void Start() {
        if (kingOfDeepsObj != null) {
            kingOfDeepsObj.SetActive(false);
            kingOfDeepsAnimator.enabled = false;
        }
    }

    private void OnEnable() {
#if UNITY_ANDROID
        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
            GPG_Manager.Instance.OnLoadKingOfDeeps += KingOfDeepsUpdate;
#endif
    }

    private void OnDisable() {
#if UNITY_ANDROID
        if (kingOfDeepsObj != null && GPG_Manager.Instance != null)
            GPG_Manager.Instance.OnLoadKingOfDeeps -= KingOfDeepsUpdate;
#endif
    }


#if UNITY_ANDROID
    void KingOfDeepsUpdate(string playerNick) {
        if (kingOfDeepsObj == null)
            return;

        kingOfDeepsObj.SetActive(true);
        kingOfDeepsTxt.text = kingOfDeepsLocalized.GetLocalizedString() + " " + playerNick;
        kingOfDeepsAnimator.enabled = true;
    }
#endif

    public void ActivateGamemodesWnd(bool isActivate) =>
        gamemodeWnd.SetActive(isActivate);

    public void OnServicesButton(int buttonIndex) {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (GPG_Manager.Instance != null)
            switch (buttonIndex) {
                case 0:
                /*gpg_manager*/
                GPG_Manager.Instance.OnShowLeaderboard();
                break;
                case 1:
                GPG_Manager.Instance.OnShowAchievements();
                break;
                case 2:
                GPG_Manager.Instance.OnSign();
                break;
            }
#endif
    }
    public void ChangeSignInColor(bool isSignIn) {
        if (signInIcon != null)
            signInIcon.color = (isSignIn) ?
               GameStaticInfo.SUCCESS_SIGN_IN_COLOR : GameStaticInfo.FAILED_SIGN_IN_COLOR;
    }

    IEnumerator ChangeColorGMwnd() {
        //Debug.Log("gm changing color started");
        while (gameModeWndBackground.color != _targetGMwndColor) {
            gameModeWndBackground.color = Color.Lerp(gameModeWndBackground.color, _targetGMwndColor, DJTime.DeltaTime);
            yield return null;
        }

        //Debug.Log("gm Color changed");
    }

    IEnumerator ChangeColorGMmainMenu() {
        while (mainMenuLight.color != _targetLightGMcolor) {
            mainMenuLight.color = Color.Lerp(mainMenuLight.color, _targetLightGMcolor, DJTime.DeltaTime);
            yield return null;
        }
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
}
