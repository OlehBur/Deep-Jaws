using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultWnd : MonoBehaviour {
    [SerializeField] TextMeshProUGUI points;
    [SerializeField] TextMeshProUGUI multiplier;
    [SerializeField] TextMeshProUGUI finalShells;
    [SerializeField] TextMeshProUGUI doubleShells;
    [SerializeField] Button doubleShellsB;

    private int receivedShells = 0;
    //private bool isAlreadyReceivedDoublePrice = false;

    void OnEnable() {
        InitWnd();

        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnAdButtonsInteractable += AdInteractable;
            GameAdsManager.Instance.OnDoubleShellsRewarded += OnDoubleShellsReceive;
            doubleShellsB.onClick.AddListener(() => GameAdsManager.Instance.OnDoubleShellsClick());
        }
    }
    private void OnDisable() {
        doubleShellsB.onClick.RemoveAllListeners();
        receivedShells = 0;

        if (GameAdsManager.Instance != null) {
            GameAdsManager.Instance.OnDoubleShellsRewarded -= OnDoubleShellsReceive;
            GameAdsManager.Instance.OnAdButtonsInteractable -= AdInteractable;
        }
    }

    void AdInteractable(bool interactable) {
        if (receivedShells > 0)
            doubleShellsB.interactable = interactable;
        else
            doubleShellsB.interactable = false;
    }

    public void AddShellsWithoutWnd() {
        float reachedScore = PlayerLeaderboardScore.Score;
        float currentMultiplier = GameStaticInfo.Gamemode switch {
            GameMode.Easy => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
            GameMode.Medium => GameStaticInfo.MEDIUM_MODE_SHELLS_MULTIPLIER,
            GameMode.Hard => GameStaticInfo.HARD_MODE_SHELLS_MULTIPLIER,
            _ => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
        };
        receivedShells = (int)(reachedScore * currentMultiplier);

        if (SanctuariesManager.Instance != null)//default shells cnt
            SanctuariesManager.Instance.OnAddShells(receivedShells);
    }

    void InitWnd() {
        float reachedScore = PlayerLeaderboardScore.Score;
        float currentMultiplier = GameStaticInfo.Gamemode switch {
            GameMode.Easy => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
            GameMode.Medium => GameStaticInfo.MEDIUM_MODE_SHELLS_MULTIPLIER,
            GameMode.Hard => GameStaticInfo.HARD_MODE_SHELLS_MULTIPLIER,
            _ => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
        };
        receivedShells = (int)(reachedScore * currentMultiplier);

        points.text = reachedScore.ToString();
        multiplier.text = "X" + currentMultiplier.ToString("F1");
        finalShells.text = receivedShells.ToString();
        doubleShells.text = $"(+{receivedShells})";
        doubleShellsB.interactable = receivedShells > 0;//don`t need double score without score)

        if (SanctuariesManager.Instance != null)//default shells cnt
            SanctuariesManager.Instance.OnAddShells(receivedShells);
    }

    void OnDoubleShellsReceive() {
        if (SanctuariesManager.Instance != null) {
            SanctuariesManager.Instance.OnAddShells(receivedShells);
            doubleShellsB.interactable = false;
        }
    }
}
