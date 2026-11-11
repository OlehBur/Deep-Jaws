//#define UNITY_ANDROID

using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;



public class GPGServicesWnd : WindowBase {
    [SerializeField] Button signInButton;
    [SerializeField] Button achievementsButton;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Button storeButton;
    [SerializeField] GameObject storePromotion;

    [SerializeField] Image signInIcon;
    [SerializeField] GameObject kingOfDeepsObj;
    [SerializeField] TextMeshProUGUI kingOfDeepsTxt;
    [SerializeField] LocalizedString kingOfDeepsLocalized;
    [SerializeField] Animator kingOfDeepsAnimator;
    [SerializeField] StoreWnd storeWnd;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    [Inject] private readonly IGPGManager _gpgManager;


    void Start() {
        if (kingOfDeepsObj != null) {
            kingOfDeepsObj.SetActive(false);
            kingOfDeepsAnimator.enabled = false;
        }
#if UNITY_ANDROID
        _gpgManager.OnSignInStatusChanged += ChangeSignInColor;
#endif
        InitServiceButtonsListeners();

#if UNITY_STANDALONE_WIN
        signInButton.gameObject.SetActive(false);
        achievementsButton.gameObject.SetActive(false);
        leaderboardButton.gameObject.SetActive(false);
        storePromotion.SetActive(false);
#endif
    }

#if UNITY_ANDROID
    private void OnEnable() {
        if (kingOfDeepsObj != null)
            _gpgManager.OnLoadKingOfDeeps += KingOfDeepsUpdate;
    }

    private void OnDisable() {
        if (kingOfDeepsObj != null)
            _gpgManager.OnLoadKingOfDeeps -= KingOfDeepsUpdate;
    }

    private void OnDestroy() {
        if (_gpgManager != null)
            _gpgManager.OnSignInStatusChanged -= ChangeSignInColor;
    }
#endif

    void InitServiceButtonsListeners() {
        storeButton.onClick.RemoveAllListeners();
        signInButton.onClick.RemoveAllListeners();
        achievementsButton.onClick.RemoveAllListeners();
        leaderboardButton.onClick.RemoveAllListeners();

        storeButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        signInButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        achievementsButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));
        leaderboardButton.onClick.AddListener(() => _audioPlayService.PlayVFX(VFX_clip.UI_ButtonClick));

        storeButton.onClick.AddListener(storeWnd.OnOpen);
#if UNITY_ANDROID
        signInButton.onClick.AddListener(_gpgManager.OnSign);
        achievementsButton.onClick.AddListener(_gpgManager.OnShowAchievements);
        leaderboardButton.onClick.AddListener(_gpgManager.OnShowLeaderboard);
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
}
