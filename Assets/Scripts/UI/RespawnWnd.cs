#define UNITY_ANDROID

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class RespawnWnd : MonoBehaviour {
    [SerializeField] TextMeshProUGUI limitRevives;
    [SerializeField] TextMeshProUGUI respawnTimer;
    [SerializeField] Button reviveB;
    [SerializeField] Button retryB;

    [Inject] private readonly IAdsManager _adsManager;

    private int _respawnTime;


    private void Awake() {
        //if (GameAdsManager.Instance != null) {
#if UNITY_ANDROID
        reviveB.onClick.AddListener(_adsManager.OnReviveClicked);
        retryB.onClick.AddListener(_adsManager.OnRetryClicked);
        _adsManager.OnAdButtonsInteractable += OnReviveAdInteractable;
        _adsManager.OnReviveRewarded += RecalculateLimit;
#endif
        //}
    }

    private void OnDestroy() {
        reviveB.onClick.RemoveAllListeners();
        retryB.onClick.RemoveAllListeners();
#if UNITY_ANDROID
        //if (GameAdsManager.Instance != null) {
            _adsManager.OnAdButtonsInteractable -= OnReviveAdInteractable;
            _adsManager.OnReviveRewarded -= RecalculateLimit;
        //}
#endif
    }

    void OnEnable() {
        limitRevives.text = GetRevivesTxt();
        reviveB.interactable = CanWatchRewardedForRevive();
        _respawnTime = GameStaticInfo.AUTO_RESPAWN_TIME;
        respawnTimer.text = _respawnTime.ToString();
        InvokeRepeating(nameof(RespawnWndTimer), 0f, 1f);
    }

    private void OnDisable() {
        CancelInvoke(nameof(RespawnWndTimer));
    }

    void OnReviveAdInteractable(bool interactable) {
        if (CanWatchRewardedForRevive())
            reviveB.interactable = interactable;
        else
            reviveB.interactable = false;
    }

    void RespawnWndTimer() {
        respawnTimer.text = _respawnTime.ToString();
        --_respawnTime;

        if (_adsManager.IsAdStartShow || !gameObject.activeInHierarchy) {
            CancelInvoke(nameof(RespawnWndTimer));
            return;
        }

        if (_respawnTime < 0)//auto retry
            _adsManager.OnRetryClicked();
    }

    private bool CanWatchRewardedForRevive() {
        WorldData data = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        string today = DateTime.Today.ToString("yyyy-MM-dd");
        string lastDate = data.rewardedLimitDate;

        if (lastDate != today) {//reset limit
            data.rewardedLimitDate = today;
            data.revivesLeft = GameStaticInfo.MAX_DAILY_REVIVALS;
            ProgressSaver.SaveData(data);
        }

        return data.revivesLeft > 0;
    }

    void RecalculateLimit() {
        WorldData data = ProgressSaver.IsWorldDataExist ?
           ProgressSaver.LoadWorldData() : new WorldData();

        --data.revivesLeft;
        limitRevives.text = data.revivesLeft.ToString() + "/" + GameStaticInfo.MAX_DAILY_REVIVALS;

        ProgressSaver.SaveData(data);
    }

    string GetRevivesTxt() {
        WorldData data = ProgressSaver.IsWorldDataExist ?
            ProgressSaver.LoadWorldData() : new WorldData();

        return data.revivesLeft.ToString() + "/" + GameStaticInfo.MAX_DAILY_REVIVALS;
    }

}
