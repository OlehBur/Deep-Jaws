using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;



public class GPGServicesUIView : MonoBehaviour {
    [SerializeField] Image signInIcon;
    [SerializeField] GameObject kingOfDeepsObj;
    [SerializeField] TextMeshProUGUI kingOfDeepsTxt;
    [SerializeField] LocalizedString kingOfDeepsLocalized;
    [SerializeField] Animator kingOfDeepsAnimator;


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
