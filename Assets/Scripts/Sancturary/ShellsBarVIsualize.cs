using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShellsBarVIsualize : MonoBehaviour {
    [SerializeField] TextMeshProUGUI shellsCnt;
    [SerializeField] Button claimB;

    private bool isInteracted = false;


    void Start() {
        if (SanctuariesManager.Instance != null)
            UpdateShellsCnt(SanctuariesManager.Instance.ShellsCnt);
    }

    void UpdateShellsCnt(int cnt) {
        shellsCnt.text = cnt.ToString();
    }

    void ClaimInteractable(bool interactable) {
        if (claimB != null)
            if (!isInteracted)
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
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnShellsAmountChanged += UpdateShellsCnt;
        }

        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnAdButtonsInteractable += ClaimInteractable;
            GameAdsManager.Instance.OnExtraShellsRewarded += OnExtraShellsReceive;
            claimB.onClick.AddListener(() => {
                GameAdsManager.Instance.OnClaimExtraShellsClick();
                isInteracted = true;
            });
        }
    }


    private void OnDisable() {
        if (SanctuariesManager.Instance != null)
            SanctuariesManager.Instance.OnShellsAmountChanged -= UpdateShellsCnt;
        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnAdButtonsInteractable -= ClaimInteractable;
            GameAdsManager.Instance.OnExtraShellsRewarded -= OnExtraShellsReceive;
        }

            claimB.onClick.RemoveAllListeners();
    }

}
