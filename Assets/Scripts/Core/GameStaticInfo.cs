using System;
using UnityEngine;

public enum FishLevel {
    Pufferfish = -5,
    Cuttlefish = -4,
    Dartfish = -1,
    Crab = 0,
    SeaBass = 1,
    Mackerel,
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
    Medium = 1,
    Hard = 2,
    None
}

public enum AccountPurchase {
    NoAd,
    Premium,
    SupportDev
}

public struct AccountTitles {
    public bool HasNoAd;
    public bool HasPremium;
    public bool HasFounder;
    public string NoAdPrice;
    public string PremiumPrice;
    public string SupportDevPrice;

    public string GetPrice(AccountPurchase ap) =>
    ap switch {
        AccountPurchase.SupportDev => SupportDevPrice,
        AccountPurchase.Premium => PremiumPrice,
        AccountPurchase.NoAd => NoAdPrice,
        _ => "---"
    };
}

public enum Skin {
    Default,
    Mercury,
    Chameleon,
    Ashflow,
    //free
    Zebra,
    Singularity,
    Blinker,
    Checkerboard,
    Corrosion,
    Cosmo,
    Cupid,
    Fairylight,
    Pixy,
    Porcelain,
    Rainbow,
    Sun,
    Invert
}

public struct PurchasedSkins {
    public bool Mercury;//1
    public bool Chameleon;//2
    public bool Ashflow;//3
    public string MercuryPrice;
    public string ChameleonPrice;
    public string AshflowPrice;

    public string GetPrice(Skin s) =>
        s switch {
            Skin.Chameleon => ChameleonPrice,
            Skin.Mercury => MercuryPrice,
            Skin.Ashflow => AshflowPrice,
            _ => "---"
        };
}

public static class GameStaticInfo {
    public const int SECONDS_IN_HOUR = 3600;
    public const int SECONDS_IN_MINUTE = 60;
    public const float PERCENT_MULTIPLIER = 100f;
    
    public const string GAME_SCENE_NAME = "Game";
    public const string MAINMENU_SCENE_NAME = "MainMenu";
    public const string LOADING_SCENE_NAME = "LoadScene";
    public const string SANCTUARY_SCENE_NAME = "Sanctuary";

    public const int DEFAULT_SCREEN_FRAMERATE = 60;
    public const int FRAMERATE_30 = 30;
    public const int FRAMERATE_60 = 60;
    public const int FRAMERATE_120 = 120;

    public const float SETTINGS_AUDIO_DEFAULT_VOLUME = 1f;
    public const float SETTINGS_SHADOW_DIST_MULTIPLIER = 100f;
    public const float SETTINGS_DEFAULT_SHADOW_DIST = .5f;
    public const float SETTINGS_DEFAYLT_GAME_RESOLUTION = .8f;
    public const int SETTINGS_DEFAULT_GAME_RESOLUTION_INDEX = 2;
    public const float SETTINGS_SHADOW_FIX_MULTIPLIER = 10f;
    public const float SETTINGS_DEFAULT_SHADOW_FIX = .1f;
    public const int SETTINGS_SHADOWMAP_RESOLUTION_0 = 256;
    public const int SETTINGS_SHADOWMAP_RESOLUTION_1 = 512;
    public const int SETTINGS_SHADOWMAP_RESOLUTION_2 = 1024;
    public const int SETTINGS_SHADOWMAP_RESOLUTION_3 = 2048;
    public const int SETTINGS_DEFAULT_LANGUAGE = 0;

    //gamemode
    public const int GAMEMODES_COUNT = 3;
    public const float EASY_MODE_SHELLS_MULTIPLIER = 1f;
    public const float MEDIUM_MODE_SHELLS_MULTIPLIER = 1.2f;
    public const float HARD_MODE_SHELLS_MULTIPLIER = 1.5f;
    public const int MAX_DAILY_REVIVALS = 10;
    public const int AUTO_RESPAWN_TIME = 5;
    public static readonly Color EASY_GM_COLOR = new Color(0.7058824f, 1f, 0.5411765f, 1f);
    public static readonly Color MEDIUM_GM_COLOR = new Color(0.8207547f, 0.5232103f, 0f, 1f);
    public static readonly Color HARD_GM_COLOR = new Color(0.6698113f, 0.09033603f, 0f, 1f);
    public static readonly Color LIGHT_DEFAULT_COLOR = new Color(1f, 0.8671192f, 0.504717f, 1f);
    public static readonly Color LIGHT_MEDIUM_COLOR = new Color(0.4544322f, 0.4999117f, 0.8679245f, 1f);
    public static readonly Color LIGHT_HARD_COLOR = new Color(0f, 0f, 0f, 1f);
    public static readonly Color FAILED_SIGN_IN_COLOR = new Color(1f, 0.4764151f, 0.4863637f, 1f);
    public static readonly Color SUCCESS_SIGN_IN_COLOR = new Color(0.678066f, 1f, 0.5707547f, 1f);

    //ui
    public const float MAX_EFFECTS_CNT = 5; // max effects count that can be displayed in the UI

    //pools
    public const float HORIZONTAL_SPAWN_MAX_OFFSET = 6f;
    public const float FISH_SPAWN_HEINGT = 1.78f;
    public const int PUFFERFISH_SPAWN_COUNT = 20;
    public const int CUTTLEFISH_SPAWN_COUNT = 30;
    public const int DARTFISH_SPAWN_COUNT = 50;
    public const int ENEMY_POOL_SIZE = 90;
    public const int PLAYER_POOL_SIZE = (int)MAX_FISH_LEVEL;

    public const float PLAYER_INIT_SHIELD_DURATION = 13f; //initial shield duration after player respawn
    public const int SHIELDS_POOL_CNT = 30; // POOL_SIZE/4
    public const int WHIRLE_POOL_CNT = 18;
    public const int BITE_PARTICLES_POOL_CNT = 25;
    public const int SKELETON_PARTICLE_POOL_CNT = 12;
    public const int DEAD_BUBBLES_POOL_CNT = 12;
    public const int INK_PARTICLES_POOL_CNT = 5;//CUTTLEFISH_SPAWN_COUNT / 6
    public const int HEAL_PARTICLES_POOL_MAX_CNT = 3;
    public const int SPRINT_PARTICLES_POOL_MAX_CNT = 2;

    //fish
    public const FishLevel PLAYER_INIT_LVL = FishLevel.SeaBass; // initial level of the player fish
    /// <summary>
    /// use for separating sharks from other fish, where index 6 is the barrier fish, and 
    /// then 7, 8, 9 are sharks (they attack the player even with the same level)
    /// </summary>
    public const FishLevel SHARK_INDEX_BARRIER = FishLevel.HammerShark;
    public const FishLevel MAX_FISH_LEVEL = FishLevel.WhiteShark;
    public const int PEACEFUL_FISH_LVL = 0;//barrier level for peaceful fish (crab and below)
    public const FishLevel UNBEATABLE_FISH_LVL = FishLevel.Dartfish/*-1*/;//barrier level for unbeatable fish (Cuttlefish and below)
    public const FishLevel POISON_IMMUNE_FISH_TRIGGER = FishLevel.Cuttlefish;
    public static readonly FishLevel[] AllFishLevels = (FishLevel[])Enum.GetValues(typeof(FishLevel));

    //world
    private const string LAYER_ENTITY = "Entity"; //layer for entities, where fish are located
    public const string LAYER_OBSTACLE = "Obstacle";
    public const string LAYER_CAMERA_AVOID_COLLIDERS = "AvoidDynamicCameraColliders";
    public static int GetEntityLayerMask() => LayerMask.GetMask(LAYER_ENTITY);//layer id for entities
    public static int GetCuttlefishOverlapLayerMask() => LayerMask.GetMask("Default", "Obstacle");
    public static int GetEntityLayerID() => LayerMask.NameToLayer("Entity");
    public const string TAG_SPAWN_ZONE = "SpawnZone";
    public const string TAG_BUBBLESHIELD = "Shield";
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENTITY = "Entity";
    public const string TAG_MAP_PREV = "MapReview";
    public const string TAG_POISON_TRIGGER = "PoisonTrigger";
    public const string TAG_OBSTACLE = "Obstacle";
    public const string NAME_PLAYER = "Player ";
    public const string NAME_ENEMY = "Enemy ";
    public const string NAME_PERMAMENT = "Permament ";


    /// <summary>
    /// RigidbodyConstraints for all fish ( rot: freze X and Z  pos: freeze Y )
    /// </summary>
    public const RigidbodyConstraints DEFAULT_FISH_CONSTRAINS_RIQIDBODY = RigidbodyConstraints.FreezeRotationX
    | RigidbodyConstraints.FreezeRotationY
    | RigidbodyConstraints.FreezeRotationZ
    | RigidbodyConstraints.FreezePositionY;

    public const float PLAYER_MAX_DISTANCE_VIEW = 25f; //to avoid spawn too close to player OR unseen animations etc. 
    public const float PLAYER_MAX_DISTANCE_ACTIVITY = 100f; //only fishes in that radius - active
    public const float STUN_SONAR_MAX_SCALE = 35f; //radius of stun effect
    public const float STUN_SONAR_SPEED = 16f;

    public const float STUN_DEFAULT_DURATION = 15f; //default stun duration

    public static bool IS_ENEMY_DETECTOR_ENABLED { get; set; } = false;


    //settings
    public const string AUDIO_MASTER_VOLUME_PARAM = "MasterVol";
    public const string AUDIO_MUSIC_VOLUME_PARAM = "MusicVol";
    public const string AUDIO_VFX_VOLUME_PARAM = "SFXVol";
    public const float MAX_MASTER_DB = 0f;
    public const float MAX_MUSIC_DB = -13f;//-10
    public const float MAX_SFX_DB = -5f;
    public const float MIN_DB = -80f;
    public enum LocalizationID {
        English = 0,
        Ukrainian = 1
    }
    public static float SliderToDB(float sliderValue, float maxDB) {
        const float CURVE_FACTOR = .2f; // Adjust this factor to change the curve steepness
        float curvedValue = Mathf.Pow(sliderValue, CURVE_FACTOR);

        return Mathf.Lerp(MIN_DB, maxDB, curvedValue);
    }

    //animators
    public static readonly int FISH_ANIMATOR_IDLE_STATE = Animator.StringToHash("Idle");
    public static readonly int BITE_ANIMATOR_TRIGGER = Animator.StringToHash("bite"); // name of the trigger for bite animation


    //shaders
    public static readonly int FISH_WAVING_UNSCAL_TIME_PROP = Shader.PropertyToID("_UnscaledTime");
    public static readonly int FISH_WAVING_FREQUENCY_PROP = Shader.PropertyToID("_Frequency");
    public static readonly int FISH_WAVING_MAX_ROT_PROP = Shader.PropertyToID("_MaxRotationValue");
    public static readonly int FISH_WAVING_ROT_PROP = Shader.PropertyToID("_RotationRad");
    //public static readonly int FISH_WAVING_CHAMELEON_PROP = Shader.PropertyToID("_ChameleonSkin");
    //public static readonly int FISH_WAVING_MERCURY_PROP = Shader.PropertyToID("_MercurySkin");
    public static readonly int FISH_WAVING_MERCURYSCALE_PROP = Shader.PropertyToID("_MercuryScale");
    //public static readonly int FISH_WAVING_ASHFLOW_PROP = Shader.PropertyToID("_AshflowSkin");
    //public static readonly int FISH_WAVING_ZEBRA_PROP = Shader.PropertyToID("_ZebraSkin");
    public static readonly int CUTTLEFISH_PROGRESS_PROP = Shader.PropertyToID("_Progress");
    public static readonly int CUTTLEFISH_COLOR_PROP = Shader.PropertyToID("_Color");
    public static readonly int ZONE_PROGRESS_PROP = Shader.PropertyToID("_Progress");
    public static readonly int ZONE_TOUCH_POS_PROP = Shader.PropertyToID("_TouchPosition");
    public const int MERCURY_SKIN_BASE_SCALE = 3;


    //rewards
    public const int EXTRA_SHELLS_CNT = 100;

    //store prices
    public const int PORCELAIN_SKIN_PRICE = 500;
    public const int ZEBRA_SKIN_PRICE = 1300;
    public const int INVERT_SKIN_PRICE = 1500;
    public const int CHECKER_SKIN_PRICE = 1800;
    public const int BLINKER_SKIN_PRICE = 2700;
    public const int CUPID_SKIN_PRICE = 4000;
    public const int FAIRYLIGHT_SKIN_PRICE = 4600;
    public const int PIXY_SKIN_PRICE = 4900;
    public const int COROSSION_SKIN_PRICE = 6000;
    public const int SUN_SKIN_PRICE = 6226;
    public const int RAINBOW_SKIN_PRICE = 8765;
    public const int COSMO_SKIN_PRICE = 9000;
    public const int SINGULAR_SKIN_PRICE = 11111;

    public const int CHAMELEON_SKIN_PRICE = 7337;
    public const int MERCURY_SKIN_PRICE = 12321;
    public const int ASHFLOW_SKIN_PRICE = 5555;

    public static int GetFreeSkinPrice(Skin skin) =>
        skin switch {
            Skin.Porcelain => PORCELAIN_SKIN_PRICE,
            Skin.Zebra => ZEBRA_SKIN_PRICE,
            Skin.Checkerboard => CHECKER_SKIN_PRICE,
            Skin.Blinker => BLINKER_SKIN_PRICE,
            Skin.Cupid => CUPID_SKIN_PRICE,
            Skin.Fairylight => FAIRYLIGHT_SKIN_PRICE,
            Skin.Pixy => PIXY_SKIN_PRICE,
            Skin.Corrosion => COROSSION_SKIN_PRICE,
            Skin.Sun => SUN_SKIN_PRICE,
            Skin.Rainbow => RAINBOW_SKIN_PRICE,
            Skin.Cosmo => COSMO_SKIN_PRICE,
            Skin.Singularity => SINGULAR_SKIN_PRICE,
            Skin.Invert => INVERT_SKIN_PRICE,

            Skin.Chameleon => CHAMELEON_SKIN_PRICE,
            Skin.Mercury => MERCURY_SKIN_PRICE,
            Skin.Ashflow => ASHFLOW_SKIN_PRICE,
            _ => -1
        } /*/ 100*/;

    public const int BIOME_RESEARCH_BOOST_TIME = 15 * 60;//15 MIN

    public static Vector2 WorldToNormalizeScreenPos(Vector3 pos, Camera targetCamera = null) {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(pos);
        return new Vector2(viewportPos.x, viewportPos.y);
    }

    public static bool Approximately(Color a, Color b, float tolerance = 0.01f) {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }

    public static Vector3 RandomXZOffset(float maxOffset = HORIZONTAL_SPAWN_MAX_OFFSET) {
        float offsetMultiplierX = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]
        float offsetMultiplierZ = (UnityEngine.Random.value - .5f) * 2f; //to get coefficient in range [-1, 1]

        return new Vector3(offsetMultiplierX * maxOffset, 0, offsetMultiplierZ * maxOffset);
    }


    public static bool IsRunningOnPlayGamesPC() {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
                {
                    //  check uniq GPG PC mark
                    return packageManager.Call<bool>("hasSystemFeature", "com.google.android.play.feature.HPE_EXPERIENCE");
                }
            }
        }
    }
    catch (System.Exception e)
    {
        UnityEngine.Debug.LogError("Помилка перевірки платформи: " + e.Message);
        return false;
    }
#else
        return false;
#endif
    }
}
