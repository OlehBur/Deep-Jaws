using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RespawnWnd : MonoBehaviour {
    [SerializeField] TextMeshProUGUI limitRevives;
    [SerializeField] TextMeshProUGUI respawnTimer;
    [SerializeField] Button reviveB;
    [SerializeField] Button retryB;

    private int respawnTime;

    private void Awake() {
        if (GameAdsManager.Instance != null) {
            reviveB.onClick.AddListener(GameAdsManager.Instance.OnReviveClicked);
            retryB.onClick.AddListener(GameAdsManager.Instance.OnRetryClicked);
            GameAdsManager.Instance.OnAdButtonsInteractable += OnReviveAdInteractable;
            GameAdsManager.Instance.OnReviveRewarded += RecalculateLimit;
        }
    }

    private void OnDestroy() {
        reviveB.onClick.RemoveAllListeners();
        retryB.onClick.RemoveAllListeners();
        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnAdButtonsInteractable -= OnReviveAdInteractable;
            GameAdsManager.Instance.OnReviveRewarded -= RecalculateLimit;
        }
    }

    void OnEnable() {
        limitRevives.text = GetRevivesTxt();
        reviveB.interactable = CanWatchRewardedForRevive();
        respawnTime = GameStaticInfo.AUTO_RESPAWN_TIME;
        respawnTimer.text = respawnTime.ToString();
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
        respawnTimer.text = respawnTime.ToString();
        --respawnTime;

        if (GameAdsManager.Instance.IsAdStartShow || !gameObject.activeInHierarchy) {
            CancelInvoke(nameof(RespawnWndTimer));
            return;
        }

        if (respawnTime < 0)//auto retry
            GameAdsManager.Instance.OnRetryClicked();
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
