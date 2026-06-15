using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
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

    private const float FIXED_DELTA_TINE_LOW = .05f;
    private const float FIXED_DELTA_TINE_HIGH = .02f;
    public static float currentFixedDeltaTime { get; private set; } = FIXED_DELTA_TINE_LOW;
    public static bool IsSceneReady { get; private set; } = false;
    private const float CANVAS_ACTIVATE_DELAY = .5f;
    private const float SCENE_ACTIVATE_DELAY = .1f;

    //game init
    void Awake() {
        //AudioListener.volume = 0f;
        gameCanvas.SetActive(false);
        GameSetPause(true);//wait until the spawn manager finishes creating pools
        spawnManager.Init();
        //foreach (JellyfishController jellyManager in jellyfishControllerArray)
        //    jellyManager.InitMovementArrays();
        cameraFollower.Init(spawnManager); // initialize the camera follower with the spawn manager
        fishHeartbeatsManager.Init(spawnManager);
        playerEffectsUI.Init(spawnManager);
        playerLeaderboardScore.Init(spawnManager);
        CurrentDefaultGameTime = DEFAULT_GAME_TIME;
        //spawnManager.OnFinishPoolCreation += PreSceneActivate;

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
            currentFixedDeltaTime = Time.fixedDeltaTime = FIXED_DELTA_TINE_LOW;
        }
        else {
            Application.targetFrameRate = 60;
            currentFixedDeltaTime = Time.fixedDeltaTime = FIXED_DELTA_TINE_HIGH;
        }
    }

    public static void SetFixedDeltaTime(bool isRestoreTime, float newDelta = FIXED_DELTA_TINE_LOW) {
        if (isRestoreTime)
            Time.fixedDeltaTime = currentFixedDeltaTime;
        else
            Time.fixedDeltaTime = newDelta;
    }

    public void GameSetPause(bool isPause) {
        Time.timeScale = (isPause) ?
            0f : CurrentDefaultGameTime; // pause or resume the game

        if (spawnManager != null && spawnManager.PlayerFish != null) {
            spawnManager.PlayerFish.gameObject.TryGetComponent(out EffectManager playerEffManager);
            if (isPause && pauseExitButton != null)
                //if (/*playerEffManager != null && */)
                //can exist only if the player is not being pursued by enemies and has no active effects
                pauseExitButton.interactable = playerEffManager.IsNoNegativeEffects() && !GameStaticInfo.IS_ENEMY_DETECTOR_ENABLED;
        }
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

public enum FishLevel {
    Pufferfish = -5,
    Cuttlefish = -4,
    Crab = 0,
    SeaBass,
    Mackarel,
    Moray,
    Lionfish,
    Barracuda,
    Tuna,
    HammerShark,
    TigerShark,
    WhiteShark
}

public enum GameMode {
    Easy = 0,
    Medium,
    Hard,
    None
}

public static class GameStaticInfo {
    public static GameMode Gamemode { get; set; } = GameMode.Easy;
    public const int GAMEMODES_COUNT = 3;
    public const FishLevel PLAYER_INIT_LVL = FishLevel.SeaBass; // initial level of the player fish
    /// <summary>
    /// use for separating sharks from other fish, where index 6 is the barrier fish, and 
    /// then 7, 8, 9 are sharks (they attack the player even with the same level)
    /// </summary>
    public const FishLevel SHARK_INDEX_BARRIER = FishLevel.HammerShark;
    public const FishLevel MAX_FISH_LEVEL = FishLevel.WhiteShark;
    public const int PEACEFUL_FISH_LVL = 0;//barrier level for peaceful fish (crab and below)
    public const FishLevel UNBEATABLE_FISH_LVL = FishLevel.Cuttlefish/*-1*/;//barrier level for unbeatable fish (Cuttlefish and below)

    private const string ENTITY_LAYER_STR = "Entity"; //layer for entities, where fish are located
    public static int ENTITY_LAYER_MASK { get; } = LayerMask.GetMask(ENTITY_LAYER_STR);//layer id for entities

    /// <summary>
    /// RigidbodyConstraints for all fish ( rot: freze X and Z  pos: freeze Y )
    /// </summary>
    public const RigidbodyConstraints DEFAULT_FISH_CONSTRAINS_RIQIDBODY = RigidbodyConstraints.FreezeRotationX
    | RigidbodyConstraints.FreezeRotationZ
    | RigidbodyConstraints.FreezePositionY;

    public const float PLAYER_MAX_DISTANCE_VIEW = 25f; //to avoid spawn too close to player OR unseen animations etc. 
    public const float PLAYER_MAX_DISTANCE_ACTIVITY = 100f; //only fishes in that radius - active
    public const float STUN_SONAR_RADIUS = 100f; //radius of stun effect

    public const float STUN_DEFAULT_DURATION = 15f; //default stun duration

    public static bool IS_ENEMY_DETECTOR_ENABLED { get; set; } = false;

    public enum LocalizationID {
        English = 0,
        Ukrainian = 1
    }

    public static Vector2 WorldToNormalizeScreenPos(Vector3 pos) {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);

        return new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
    }

    public static bool TryGetFish(GameObject fishObject, out Fish fish) {
        return TryGetFishFromPool(fishObject, SpawnManager.visibleFishPool, SpawnManager.VisPool_FishComp, out fish) ||
        TryGetFishFromPool(fishObject, SpawnManager.cuttlefishPool, SpawnManager.cuttlefishPool_FishComp, out fish) ||
        TryGetFishFromPool(fishObject, SpawnManager.pufferfishPool, SpawnManager.pufferfishPool_FishComp, out fish);
    }

    static bool TryGetFishFromPool(GameObject fishObject, GameObject[] pool, Fish[] fishPool, out Fish fish) {
        int index = Array.IndexOf(pool, fishObject);
        if (index < 0) {
            fish = null;
            return false;
        }
        else {
            fish = fishPool[index];
            return true;
        }
    }
}
