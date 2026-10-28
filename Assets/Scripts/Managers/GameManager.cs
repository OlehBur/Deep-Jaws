using UnityEngine;
using UnityEngine.UI;
using VContainer;



public interface IGameManager {
    void GameSetPause(bool isPause);
    void ResetWorldData();
}

public class GameManager : MonoBehaviour, IGameManager {
    //[SerializeField] MainmenuManager mainmenuManager;
    //[SerializeField] CameraFollower cameraFollower;
    //[SerializeField] SpawnManager spawnManager;
    [SerializeField] FishHeartbeatsManager fishHeartbeatsManager;
    //[SerializeField] JellyfishController[] jellyfishControllerArray;
    [SerializeField] PlayerUIController playerEffectsUI;
    [SerializeField] PlayerLeaderboardScore playerLeaderboardScore;
    [SerializeField] Button pauseExitButton;
    [SerializeField] GameObject gameCanvas;

    [Inject] private readonly IPlayerFishPoolService _playerProvider;
    [Inject] private readonly ISettingsManager _settingsControllerUI;
    [Inject] private readonly IGraphicManager _graphicManager;

    public static int ScreenMaxFramerate {
        get {
            int screenMaxFramerate = 0;
            screenMaxFramerate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
            if (screenMaxFramerate <= 0)
                screenMaxFramerate = GameStaticInfo.DEFAULT_SCREEN_FRAMERATE;

            return screenMaxFramerate;
        }
    }
    public static float CurrentDefaultGameTime { get; private set; }
    public static readonly float DEFAULT_GAME_TIME = 1f;
    public static readonly bool IsRunOnPC = GameStaticInfo.IsRunningOnPlayGamesPC() || Application.isEditor || IS_PC_DEMO;
    public const /*static*/ bool IS_PC_DEMO = false;
    //#if UNITY_STANDALONE_WIN// && !UNITY_EDITOR
    //return true;
    //#else
    //return false;
    //#endif
    //}
    public static float CurrentFixedDeltaTime { get; private set; } = FIXED_DELTA_TINE_LOW;
    public static bool IsGamePaused => DJTime.TimeScale == 0f;
    public static bool IsSceneReady { get; private set; } = false;

    private const float CANVAS_ACTIVATE_DELAY = .5f;
    private const float SCENE_ACTIVATE_DELAY = .14f;
    private const float FIXED_DELTA_TINE_HIGH = .02f;
    private const float FIXED_DELTA_TINE_LOW = .05f;

    //game init
    void Awake() {

        //Debug.LogWarning("Run On ANDROID Enanbled");

        gameCanvas.SetActive(false);
        //GameSetPause(true);//wait until the spawn manager finishes creating pools

        //spawnManager.Init();
        //cameraFollower.Init(); // initialize the camera follower with the spawn manager
        //fishHeartbeatsManager.Init();
        //playerEffectsUI.Init();
        //playerLeaderboardScore.Init();
        CurrentDefaultGameTime = DEFAULT_GAME_TIME;

        SetFixedDelta();
        //GameSetPause(false);

        Invoke(nameof(DelayCanvasActivate), CANVAS_ACTIVATE_DELAY);
        Invoke(nameof(DelaySceneActivate), SCENE_ACTIVATE_DELAY);
    }

    public void ResetWorldData() {
        PlayerLeaderboardScore.LoadSavedData();
        PlayerPrefs.DeleteAll();//delete all player prefs data 
        _settingsControllerUI.SaveAudioData(); // Save current audio settings
        _graphicManager.SaveGraphicsData(); // Save current graphic settings
        PlayerLeaderboardScore.SaveLoadedData();
        ProgressSaver.DeleteWorldData();//DeleteOnlyWorldData();
    }

    void DelaySceneActivate() {
        IsSceneReady = true;
        AudioListener.volume = 1f; // enable vol
    }

    void DelayCanvasActivate() {
        gameCanvas.SetActive(true);
    }

    void SetFixedDelta() {
        if (SystemInfo.processorCount < 5) {
            //Application.targetFrameRate = 30;
            DJTime.SetFixedTime(FIXED_DELTA_TINE_LOW);
            CurrentFixedDeltaTime = FIXED_DELTA_TINE_LOW;
        }
        else {
            //Application.targetFrameRate = 60;
            DJTime.SetFixedTime(FIXED_DELTA_TINE_HIGH);
            CurrentFixedDeltaTime = FIXED_DELTA_TINE_HIGH;
        }
    }

    public static void SetFixedDeltaTime(bool isRestoreTime, float newDelta = FIXED_DELTA_TINE_LOW) {
        if (isRestoreTime)
            DJTime.SetFixedTime(CurrentFixedDeltaTime);
        else
            DJTime.SetFixedTime(newDelta);
    }

    public void GameSetPause(bool isPause/*, bool activatePauseWnd*/) {
        DJTime.TimeScale = (isPause) ?
            0f : CurrentDefaultGameTime; // pause or resume the game

        if (_playerProvider.PlayerCurrentFish != null) {
            _playerProvider.PlayerCurrentFish.gameObject.TryGetComponent(out EffectManager playerEffManager);
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
        DJTime.TimeScale = time;
    }

    public static bool IsInternetNotAvailable =>
        Application.internetReachability == NetworkReachability.NotReachable;

    private void OnDestroy() {
        DJTime.TimeScale = DEFAULT_GAME_TIME;
        SetFixedDeltaTime(true);
        //IsSceneReady = false;
    }
}