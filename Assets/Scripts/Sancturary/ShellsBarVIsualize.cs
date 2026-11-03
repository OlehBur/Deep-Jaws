#define UNITY_ANDROID

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class ShellsBarVIsualize : MonoBehaviour {
    [SerializeField] TextMeshProUGUI shellsCnt;
    [SerializeField] Button claimB;

    private bool _isInteracted = false;

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;
    [Inject] private readonly IAdsManager _adsManager;



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

        //if (GameAdsManager.Instance != null) {
#if UNITY_ANDROID
        _adsManager.OnAdButtonsInteractable += ClaimInteractable;
        _adsManager.OnExtraShellsRewarded += OnExtraShellsReceive;
        claimB.onClick.AddListener(() => {
            _adsManager.OnClaimExtraShellsClick();
            _isInteracted = true;
        });
#endif
        //}
    }


    private void OnDisable() {
        if (_sanctuariesManager != null)
            _sanctuariesManager.OnShellsAmountChanged -= UpdateShellsCnt;

#if UNITY_ANDROID
        //if (GameAdsManager.Instance != null) {
        _adsManager.OnAdButtonsInteractable -= ClaimInteractable;
        _adsManager.OnExtraShellsRewarded -= OnExtraShellsReceive;
        //}
#endif

        claimB.onClick.RemoveAllListeners();
    }

}
