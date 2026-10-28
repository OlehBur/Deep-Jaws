//#define UNITY_ANDROID

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class ShellsBarVIsualize : MonoBehaviour {
    [SerializeField] TextMeshProUGUI shellsCnt;
    [SerializeField] Button claimB;

    private bool _isInteracted = false;

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;



    void Start() {
        if (_sanctuariesManager != null)
            UpdateShellsCnt(_sanctuariesManager.ShellsCnt);
    }

    void UpdateShellsCnt(int cnt) {
        shellsCnt.text = cnt.ToString();
    }

    void ClaimInteractable(bool interactable) {
        if (claimB != null)
            if (!_isInteracted)
                claimB.interactable = interactable;
            else
                claimB.interactable = false;
    }

    void OnExtraShellsReceive() {
        if (claimB != null)
            claimB.interactable = false;
        shellsCnt.color = Color.orange;
    }

    private void OnEnable() {
        if (_sanctuariesManager != null) {
            _sanctuariesManager.OnShellsAmountChanged += UpdateShellsCnt;
        }

        if (GameAdsManager.Instance != null) {
#if UNITY_ANDROID
            GameAdsManager.Instance.OnAdButtonsInteractable += ClaimInteractable;
            GameAdsManager.Instance.OnExtraShellsRewarded += OnExtraShellsReceive;
            claimB.onClick.AddListener(() => {
                GameAdsManager.Instance.OnClaimExtraShellsClick();
                _isInteracted = true;
            });
#endif
        }
    }


    private void OnDisable() {
        if (_sanctuariesManager != null)
            _sanctuariesManager.OnShellsAmountChanged -= UpdateShellsCnt;

#if UNITY_ANDROID
        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnAdButtonsInteractable -= ClaimInteractable;
            GameAdsManager.Instance.OnExtraShellsRewarded -= OnExtraShellsReceive;
        }
#endif

        claimB.onClick.RemoveAllListeners();
    }

}
