
using System;

public interface IGPGManager {
#if UNITY_ANDROID
    event Action<string> OnLoadKingOfDeeps;
    event Action<bool> OnSignInStatusChanged;
#endif

    void OnShowAchievements();
    void OnShowLeaderboard();
    void CheckFounderStatus(Action<bool> onComplete);
    void OnSign();
}
