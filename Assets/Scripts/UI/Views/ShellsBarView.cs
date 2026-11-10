#define UNITY_ANDROID

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class ShellsBarView : MonoBehaviour {
    [SerializeField] TextMeshProUGUI shellsCnt;
    [SerializeField] Button claimB;

    private bool _isInteracted = false;

    [Inject] private readonly ISanctuariesManager _sanctuariesManager;
    [Inject] private readonly IAdsManager _adsManager;



    void Start() {
        if (_sanctuariesManager != null)
            UpdateShellsCnt(_sanctuariesManager.ShellsCnt);
    }

    public void UpdateShellsCnt(int cnt) {
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

#if UNITY_ANDROID
        if (_adsManager != null) {
            _adsManager.OnAdButtonsInteractable += ClaimInteractable;
            _adsManager.OnExtraShellsRewarded += OnExtraShellsReceive;
            claimB.onClick.AddListener(() => {
                _adsManager.OnClaimExtraShellsClick();
                _isInteracted = true;
            });
            claimB.gameObject.SetActive(true);
        }
        else
            claimB.gameObject.SetActive(false);
#endif
    }


    private void OnDisable() {
        if (_sanctuariesManager != null)
            _sanctuariesManager.OnShellsAmountChanged -= UpdateShellsCnt;

#if UNITY_ANDROID
        if (_adsManager != null) {
            _adsManager.OnAdButtonsInteractable -= ClaimInteractable;
            _adsManager.OnExtraShellsRewarded -= OnExtraShellsReceive;
        }
#endif

        claimB.onClick.RemoveAllListeners();
    }

}
