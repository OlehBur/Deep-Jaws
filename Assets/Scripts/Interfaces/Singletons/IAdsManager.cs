public interface IAdsManager {
#if UNITY_ANDROID
    event Action<bool> OnAdButtonsInteractable;
    event Action OnDoubleShellsRewarded;
    event Action OnExtraShellsRewarded;
    event Action OnReviveRewarded;
    event Action OnDeadRespawn;
    event Action OnDeadRevive;

    bool IsAdStartShow { get; }
    void OnReviveClicked();
    void OnDoubleShellsClick();
    void OnClaimExtraShellsClick();
    void OnBoostClicked(BiomeType type);
    bool OnShowInterstitialAd();
    void OnRetryClicked();
#endif
}