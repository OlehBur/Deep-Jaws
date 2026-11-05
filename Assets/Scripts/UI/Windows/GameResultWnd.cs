//#define UNITY_ANDROID

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;



public class GameResultWnd : MonoBehaviour {
    [SerializeField] TextMeshProUGUI points;
    [SerializeField] TextMeshProUGUI multiplier;
    [SerializeField] TextMeshProUGUI finalShells;
    [SerializeField] TextMeshProUGUI doubleShells;
    [SerializeField] Button doubleShellsB;

    [Inject] private readonly IAdsManager _adsManager;
    [Inject] private readonly ISanctuariesManager _sanctuariesManager;

    private int _receivedShells = 0;
    //private bool isAlreadyReceivedDoublePrice = false;


    void OnEnable() {
        InitWnd();

#if UNITY_ANDROID
        if (_adsManager != null) {
            _adsManager.OnAdButtonsInteractable += AdInteractable;
            _adsManager.OnDoubleShellsRewarded += OnDoubleShellsReceive;
            doubleShellsB.onClick.AddListener(() => _adsManager.OnDoubleShellsClick());
        }
#endif
    }
    private void OnDisable() {
        doubleShellsB.onClick.RemoveAllListeners();
        _receivedShells = 0;

#if UNITY_ANDROID
        if (_adsManager != null) {
            _adsManager.OnDoubleShellsRewarded -= OnDoubleShellsReceive;
            _adsManager.OnAdButtonsInteractable -= AdInteractable;
        }
#endif
    }

    void AdInteractable(bool interactable) {
        if (doubleShellsB != null)
            if (_receivedShells > 0)
                doubleShellsB.interactable = interactable;
            else
                doubleShellsB.interactable = false;
    }

    public void AddShellsWithoutWnd() {
        float reachedScore = PlayerLeaderboardScore.Score;
        float currentMultiplier = GameModeController.CurrentMode switch {
            GameMode.Easy => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
            GameMode.Medium => GameStaticInfo.MEDIUM_MODE_SHELLS_MULTIPLIER,
            GameMode.Hard => GameStaticInfo.HARD_MODE_SHELLS_MULTIPLIER,
            _ => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
        };
        _receivedShells = (int)(reachedScore * currentMultiplier);

        if (_sanctuariesManager != null)//default shells cnt
            _sanctuariesManager.OnAddShells(_receivedShells);
    }

    void InitWnd() {
        float reachedScore = PlayerLeaderboardScore.Score;
        float currentMultiplier = GameModeController.CurrentMode switch {
            GameMode.Easy => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
            GameMode.Medium => GameStaticInfo.MEDIUM_MODE_SHELLS_MULTIPLIER,
            GameMode.Hard => GameStaticInfo.HARD_MODE_SHELLS_MULTIPLIER,
            _ => GameStaticInfo.EASY_MODE_SHELLS_MULTIPLIER,
        };
        _receivedShells = (int)(reachedScore * currentMultiplier);

        points.text = reachedScore.ToString();
        multiplier.text = "X" + currentMultiplier.ToString("F1");
        finalShells.color = Color.white;
        finalShells.text = _receivedShells.ToString();
        doubleShells.text = $"(+{_receivedShells})";
        doubleShellsB.interactable = _receivedShells > 0;//don`t need double score without score)
        doubleShellsB.gameObject.SetActive(true);

        if (_sanctuariesManager != null)//default shells cnt
            _sanctuariesManager.OnAddShells(_receivedShells);
    }

    void OnDoubleShellsReceive() {
        if (_sanctuariesManager != null)
            _sanctuariesManager.OnAddShells(_receivedShells);

        doubleShellsB.interactable = false;
        doubleShellsB.gameObject.SetActive(false);
        finalShells.text = (_receivedShells * 2).ToString();
        finalShells.color = Color.orange;
    }
}
