using System;
using UnityEngine;

public enum FishLevel {
    Pufferfish = -5,
    Cuttlefish = -4,
    Dartfish = -1,
    Crab = 0,
    SeaBass,
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
    Medium,
    Hard,
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
    //public bool NoAdHasDiscount;
    //public bool PremiumHasDiscount;
    //public bool SupportDevHasDiscount;
    //public string NoAdBadgeTxt;
    //public string PremiumBadgeTxt;
    //public string SupportDevBadgeTxt;

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
    Ashflow
}

public struct PurchasedSkins {
    public bool Mercury;//1
    public bool Chameleon;//2
    public bool Ashflow;//3
    public string MercuryPrice;
    public string ChameleonPrice;
    public string AshflowPrice;
    public Skin Selected;
    //public bool MercuryHasDiscount;
    //public bool ChameleonHasDiscount;
    //public bool AshflowHasDiscount;
    //public string MercuryBadgeTxt;
    //public string ChameleonBadgeTxt;
    //public string AshflowBadgeTxt;

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

    public static readonly string GAME_SCENE_NAME = "Game";
    public static readonly string MAINMENU_SCENE_NAME = "MainMenu";
    public static readonly string LOADING_SCENE_NAME = "LoadScene";
    public static readonly string SANCTUARY_SCENE_NAME = "Sanctuary";

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
    public static GameMode Gamemode { get; set; } = GameMode.Easy;
    public const int GAMEMODES_COUNT = 3;
    public const float EASY_MODE_SHELLS_MULTIPLIER = 1f;
    public const float MEDIUM_MODE_SHELLS_MULTIPLIER = 1.2f;
    public const float HARD_MODE_SHELLS_MULTIPLIER = 1.5f;

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
    public static int LAYER_MASK_ENTITY => LayerMask.GetMask(LAYER_ENTITY);//layer id for entities
    public static int CUTTLEFISH_OVERLAP_LAYER_MASK => LayerMask.GetMask("Default", "Obstacle");
    public const string TAG_SPAWN_ZONE = "SpawnZone";
    public const string TAG_BUBBLESHIELD = "Shield";
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENTITY = "Entity";
    public const string TAG_MAP_PREV = "MapReview";
    public const string TAG_POISON_TRIGGER = "PoisonTrigger";
    public const string TAG_OBSTACLE = "Obstacle";
    public const string NAME_PLAYER = "Player ";
    public const string NAME_ENEMY = "Enemy ";

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
    public enum LocalizationID {
        English = 0,
        Ukrainian = 1
    }

    //animators
    public static readonly int FISH_ANIMATOR_IDLE_STATE = Animator.StringToHash("Idle");
    public static readonly int BITE_ANIMATOR_TRIGGER = Animator.StringToHash("bite"); // name of the trigger for bite animation


    //shaders
    public static readonly int FISH_WAVING_UNSCAL_TIME_PROP = Shader.PropertyToID("_UnscaledTime");
    public static readonly int FISH_WAVING_FREQUENCY_PROP = Shader.PropertyToID("_Frequency");
    public static readonly int FISH_WAVING_MAX_ROT_PROP = Shader.PropertyToID("_MaxRotationValue");
    public static readonly int FISH_WAVING_ROT_PROP = Shader.PropertyToID("_RotationRad");
    public static readonly int FISH_WAVING_CHAMELEON_PROP = Shader.PropertyToID("_ChameleonSkin");
    public static readonly int FISH_WAVING_MERCURY_PROP = Shader.PropertyToID("_MercurySkin");
    public static readonly int FISH_WAVING_MERCURYSCALE_PROP = Shader.PropertyToID("_MercuryScale");
    public static readonly int FISH_WAVING_ASHFLOW_PROP = Shader.PropertyToID("_AshflowSkin");
    public static readonly int FISH_WAVING_ZEBRA_PROP = Shader.PropertyToID("_ZebraSkin");
    public static readonly int CUTTLEFISH_PROGRESS_PROP = Shader.PropertyToID("_Progress");
    public static readonly int CUTTLEFISH_COLOR_PROP = Shader.PropertyToID("_Color");
    public static readonly int ZONE_PROGRESS_PROP = Shader.PropertyToID("_Progress");
    public static readonly int ZONE_TOUCH_POS_PROP = Shader.PropertyToID("_TouchPosition");
    public const int MERCURY_SKIN_BASE_SCALE = 3;

    public const int BIOME_RESEARCH_BOOST_TIME = 15 * 60;//15 MIN

    public static Vector2 WorldToNormalizeScreenPos(Vector3 pos) {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);

        return new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
    }

    public static bool TryGetFish(GameObject fishObject, out Fish fish) {
        //Debug.LogWarning("Player inviolability Activated!");
        return TryGetFishFromPool(fishObject, SpawnManager.playerPool_Objects, SpawnManager.playerPool_FishComponents, out fish) ||//players
            TryGetFishFromPool(fishObject, SpawnManager.visible_poolFishObjects, SpawnManager.visible_poolFishComponents, out fish) ||//default enemies
            TryGetFishFromPool(fishObject, SpawnManager.cuttlefishPool_Objects, SpawnManager.cuttlefishPool_FishComponents, out fish) ||//cucttlefishes
            TryGetFishFromPool(fishObject, SpawnManager.pufferfishPool_Objects, SpawnManager.pufferfishPool_FishComponents, out fish) || //dartfish
            TryGetFishFromPool(fishObject, SpawnManager.dartfishPool_Objects, SpawnManager.dartfishPool_FishComponents, out fish);
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
