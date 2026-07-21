using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    //[SerializeField] MainmenuManager mainmenuManager;
    [SerializeField] CameraFollower cameraFollower;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] FishHeartbeatsManager fishHeartbeatsManager;
    [SerializeField] JellyfishController[] jellyfishControllerArray;
    [SerializeField] PlayerUIController playerEffectsUI;
    [SerializeField] PlayerLeaderboardScore playerLeaderboardScore;
    [SerializeField] Button pauseExitButton;
    [SerializeField] GameObject gameCanvas;
    /// <summary>
    /// позначає час який має бути встановлено під час виходу з паузи (може бути як стандартний 1, так і змінений певними ефектами)
    /// </summary>
    public static float CurrentDefaultGameTime { get; private set; }
    public static readonly float DEFAULT_GAME_TIME = 1f;
    public static readonly bool IsRunOnPC = GameStaticInfo.IsRunningOnPlayGamesPC() || Application.isEditor;
    private const float FIXED_DELTA_TINE_LOW = .05f;
    private const float FIXED_DELTA_TINE_HIGH = .02f;
    public static float CurrentFixedDeltaTime { get; private set; } = FIXED_DELTA_TINE_LOW;
    public static bool IsGamePaused => Time.timeScale == 0f;
    public static bool IsSceneReady { get; private set; } = false;

    private const float CANVAS_ACTIVATE_DELAY = .5f;
    private const float SCENE_ACTIVATE_DELAY = .1f;

    //game init
    void Awake() {
        //Debug.LogWarning("Run On ANDROID Enanbled");

        gameCanvas.SetActive(false);
        GameSetPause(true);//wait until the spawn manager finishes creating pools
        spawnManager.Init();
        cameraFollower.Init(spawnManager); // initialize the camera follower with the spawn manager
        fishHeartbeatsManager.Init(spawnManager);
        playerEffectsUI.Init(spawnManager);
        playerLeaderboardScore.Init(spawnManager);
        CurrentDefaultGameTime = DEFAULT_GAME_TIME;

        SetFPSSettings();
        GameSetPause(false);
        Invoke(nameof(DelayCanvasActivate), CANVAS_ACTIVATE_DELAY);
        Invoke(nameof(DelaySceneActivate), SCENE_ACTIVATE_DELAY);
    }

    void DelaySceneActivate() {
        IsSceneReady = true;
        AudioListener.volume = 1f; // enable vol
    }

    void DelayCanvasActivate() {
        gameCanvas.SetActive(true);
    }

    void SetFPSSettings() {
        if (SystemInfo.processorCount < 5) {
            Application.targetFrameRate = 30;
            CurrentFixedDeltaTime = Time.fixedDeltaTime = FIXED_DELTA_TINE_LOW;
        }
        else {
            Application.targetFrameRate = 60;
            CurrentFixedDeltaTime = Time.fixedDeltaTime = FIXED_DELTA_TINE_HIGH;
        }
    }

    public static void SetFixedDeltaTime(bool isRestoreTime, float newDelta = FIXED_DELTA_TINE_LOW) {
        if (isRestoreTime)
            Time.fixedDeltaTime = CurrentFixedDeltaTime;
        else
            Time.fixedDeltaTime = newDelta;
    }

    public void GameSetPause(bool isPause/*, bool activatePauseWnd*/) {
        Time.timeScale = (isPause) ?
            0f : CurrentDefaultGameTime; // pause or resume the game

        if (spawnManager != null && spawnManager.PlayerFish != null) {
            spawnManager.PlayerFish.gameObject.TryGetComponent(out EffectManager playerEffManager);
            if (isPause && pauseExitButton != null)
                //if (/*playerEffManager != null && */)
                //can exist only if the player is not being pursued by enemies and has no active effects
                pauseExitButton.interactable = playerEffManager.IsNoNegativeEffects() && !GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED;
        }

        //if ((activatePauseWnd && isPause) || !isPause)//when unpause or pause if bool activate = true
        //    mainmenuManager.ActivatePauseWnd(isPause);
    }

    public static void ChangeGameTime(float time) {
        CurrentDefaultGameTime = time;
        Time.timeScale = time;
    }

    private void OnDestroy() {
        Time.timeScale = DEFAULT_GAME_TIME;
        SetFixedDeltaTime(true);
        IsSceneReady = false;
    }
}