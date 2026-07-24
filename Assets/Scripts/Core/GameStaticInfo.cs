using System;
using UnityEngine;

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

    private const string LAYER_ENTITY = "Entity"; //layer for entities, where fish are located
    public const string LAYER_OBSTACLE = "Obstacle";
    public const string LAYER_CAMERA_AVOID_COLLIDERS = "AvoidDynamicCameraColliders"; 

    public static int LAYER_MASK_ENTITY => LayerMask.GetMask(LAYER_ENTITY);//layer id for entities


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
    | RigidbodyConstraints.FreezeRotationZ
    | RigidbodyConstraints.FreezePositionY;

    public const float PLAYER_MAX_DISTANCE_VIEW = 25f; //to avoid spawn too close to player OR unseen animations etc. 
    public const float PLAYER_MAX_DISTANCE_ACTIVITY = 100f; //only fishes in that radius - active
    public const float STUN_SONAR_RADIUS = 100f; //radius of stun effect

    public const float STUN_DEFAULT_DURATION = 15f; //default stun duration

    public static bool IS_ENEMY_DETECTOR_ENABLED { get; set; } = false;

    public static readonly int FISH_WAVING_MERCURY_PROP = Shader.PropertyToID("_MercurySkin");
    public static readonly int FISH_WAVING_CHAMELEON_PROP = Shader.PropertyToID("_ChameleonSkin");
    public static readonly int FISH_WAVING_MERCURYSCALE_PROP = Shader.PropertyToID("_MercuryScale");
    public static readonly int FISH_WAVING_ASHFLOW_PROP = Shader.PropertyToID("_AshflowSkin");
    public const int MERCURY_SKIN_BASE_SCALE = 3;

    public enum LocalizationID {
        English = 0,
        Ukrainian = 1
    }

    public static Vector2 WorldToNormalizeScreenPos(Vector3 pos) {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);

        return new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
    }

    public static bool TryGetFish(GameObject fishObject, out Fish fish) {
        //Debug.LogWarning("Player inviolability Activated!");
        return TryGetFishFromPool(fishObject, SpawnManager.playerPool_Objects, SpawnManager.playerPool_FishComponents, out fish) ||//players
            TryGetFishFromPool(fishObject, SpawnManager.visible_poolFishObjects, SpawnManager.visible_poolFishComponents, out fish) ||//default enemies
        TryGetFishFromPool(fishObject, SpawnManager.cuttlefishPool_Objects, SpawnManager.cuttlefishPool_FishComponents, out fish) ||//cucttlefishes
        TryGetFishFromPool(fishObject, SpawnManager.pufferfishPool_Objects, SpawnManager.pufferfishPool_FishComponents, out fish);//pufferfishes
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
