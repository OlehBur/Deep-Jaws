using System;
using System.IO;
using UnityEngine;


public interface IData {
    //public const int INVALID_VAL = -1;
    string SavePath { get; }
    public bool IsDataExist() => File.Exists(SavePath);
    public void DeleteData() {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}

//save amnd load game data with encryption
public static class ProgressSaver {

    public static void SaveData(IData data) {//same but faster? (onl interface fields passes)
        string json = JsonUtility.ToJson(data);
        string encrypted = Encryptor.Encrypt(json);

        File.WriteAllText(data.SavePath, encrypted);
    }
    //public static void SaveData<T>(T data) where T : IData {//same?
    //    string json = JsonUtility.ToJson(data);
    //    string encrypted = Encryptor.Encrypt(json);

    //    File.WriteAllText(data.SavePath, encrypted);
    //}
    //public static void SaveWorldData() => SaveData(new WorldData());
    //public static void SaveAccountData() => SaveData(new AccountData());
    //public static void SaveSettingsData() => SaveData(new SettingsData());

    private static bool IsDataExist(IData data) =>
        File.Exists(data.SavePath);
    public static bool IsWorldDataExist => IsDataExist(new WorldData());
    public static bool IsAccountDataExist => IsDataExist(new AccountData());
    public static bool IsSettingsDataExist => IsDataExist(new SettingsData());
    public static bool IsSanctuaryDataExist => IsDataExist(new SanctuaryData());

    private static T LoadData<T>(T data) where T : IData {
        if (!data.IsDataExist())
            return default;

        string encrypted = File.ReadAllText(data.SavePath);
        string json = Encryptor.Decrypt(encrypted);

        return JsonUtility.FromJson<T>(json);
    }
    public static WorldData LoadWorldData() => LoadData(new WorldData());
    public static AccountData LoadAccountData() => LoadData(new AccountData());
    public static SettingsData LoadSettingsData() => LoadData(new SettingsData());
    public static SanctuaryData LoadSanctuaryData() => LoadData(new SanctuaryData());

    private static void DeleteData(IData data) =>
        data.DeleteData();
    public static void DeleteWorldData() => DeleteData(new WorldData());
    public static void DeleteAccountData() => DeleteData(new AccountData());
    public static void DeleteSettingsData() => DeleteData(new SettingsData());
    public static void DeleteSanctuaryData() => DeleteData(new SanctuaryData());


    /// <summary>
    /// premium - delete interstitial and rewardeed ads, noAds - only del interstitial
    /// </summary>
    /// <returns></returns>
    public static (bool premium, bool noAds) IsPlayerHasAdImmunitet() {
        if (IsAccountDataExist) {
            AccountData data = LoadAccountData();
            if (data != null)
                return (data.isPremiumUser, data.isNoAD);
        }
        return (false, false);
    }
}

[Serializable]
public class SettingsData : IData {
    public string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    //audio
    public float volume_master = GameStaticInfo.SETTINGS_AUDIO_DEFAULT_VOLUME;
    public float volume_music = GameStaticInfo.SETTINGS_AUDIO_DEFAULT_VOLUME;
    public float volume_sfx = GameStaticInfo.SETTINGS_AUDIO_DEFAULT_VOLUME;

    //graphics
    public bool bloom = true;
    public bool blur = true;
    public bool vignette = true;
    public bool shadow_casting = true;
    public float shadow_distance = GameStaticInfo.SETTINGS_SHADOW_DIST_MULTIPLIER;
    public float shadow_fixer = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_FIX;
    public float game_resolution = GameStaticInfo.SETTINGS_DEFAYLT_GAME_RESOLUTION;
    public int shadow_resolution = GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_2;
    public int framerate = GameStaticInfo.DEFAULT_SCREEN_FRAMERATE;

    //language & ui
    public bool autoUI = true;
    public int language = GameStaticInfo.SETTINGS_DEFAULT_LANGUAGE;
}

public class WorldData : IData {
    public string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public bool IsAllTipsShowed() => isVictimTipShowed && isEnemyTipShowed
        && isEffectTipShowed && isStageTipShowed && isClearCoralsTipShowed
        && isClearSeaweedShowed && isAlgaesBarrierTipShowed && isDeepCoralBarrierTipShowed;

    //game data
    //public bool isNewSaveType = false;//only for transfer old players data
    public GameMode gameMode = GameMode.None;//(GameMode)PlayerPrefs.GetInt("GameMode", 0);
    public Vector3 playerPosition = Vector3.zero;
    public Quaternion playerRotation = Quaternion.identity;
    public float playerProgress = 0;
    public FishLevel playerLevel = GameStaticInfo.PLAYER_INIT_LVL;
    public int playerHealth = 0;

    //tips
    public bool isVictimTipShowed = false;
    public bool isEnemyTipShowed = false;
    public bool isEffectTipShowed = false;
    public bool isStageTipShowed = false;
    public bool isClearCoralsTipShowed = false;
    public bool isClearSeaweedShowed = false;
    public bool isAlgaesBarrierTipShowed = false;
    public bool isDeepCoralBarrierTipShowed = false;

    public string rewardedLimitDate = "";
    public int revivesLeft = GameStaticInfo.MAX_DAILY_REVIVALS;
}

[Serializable]
public class AccountData : IData {
    public string SavePath => Path.Combine(Application.persistentDataPath, "account.json");

    //player data
    public int shellsCnt = 0;
    public bool isPremiumUser = false;
    public bool isNoAD = false;
    public bool isFounder = false;

    //skins
    public bool hasAshflowSkin = false;
    public bool hasMercurySkin = false;
    public bool hasChameleon = false;
    public bool hasZebra = false;
    public bool hasSingularity = false;
    public bool hasBlinker = false;
    public bool hasCheckerboard = false;
    public bool hasCorrosion = false;
    public bool hasCosmo = false;
    public bool hasCupid = false;
    public bool hasFairylight = false;
    public bool hasPixy = false;
    public bool hasPorcelain = false;
    public bool hasRainbow = false;
    public bool hasSun = false;
    public bool hasInvert = false;
    public Skin selectedSkin;

    //stats
    public bool confirmedLoyalty = false;
    public bool isConfirmedNoInternetDisadvantages = false;//in case user first open app without internet connection
    public bool isLicensed = false;
    public bool firstUpdOpen1_7 = true;

    public bool IsHasSkin(Skin skin) =>
        skin switch {
            Skin.Mercury => hasMercurySkin,
            Skin.Chameleon => hasChameleon,
            Skin.Ashflow => hasAshflowSkin,
            Skin.Porcelain => hasPorcelain,
            Skin.Zebra => hasZebra,
            Skin.Checkerboard => hasCheckerboard,
            Skin.Blinker => hasBlinker,
            Skin.Cupid => hasCupid,
            Skin.Fairylight => hasFairylight,
            Skin.Pixy => hasPixy,
            Skin.Corrosion => hasCorrosion,
            Skin.Sun => hasSun,
            Skin.Rainbow => hasRainbow,
            Skin.Cosmo => hasCosmo,
            Skin.Singularity => hasSingularity,
            Skin.Invert => hasInvert,
            _ => false
        };
}

[Serializable]
public class SanctuaryData : IData {
    public const int NON_UPGRADE_TIME = -1;
    public string SavePath => Path.Combine(Application.persistentDataPath, "sanctuary.json");

    public int coralReef_Lvl = 0;
    public int cliffsSeaweed_Lvl = 0;
    public int pelagial_Lvl = 0;
    public int oceanicAbyss_Lvl = 0;

    public int coralReef_SecondsToUpgrade = NON_UPGRADE_TIME;
    public int cliffsSeaweed_SecondsToUpgrade = NON_UPGRADE_TIME;
    public int pelagial_SecondsToUpgrade = NON_UPGRADE_TIME;
    public int oceanicAbyss_SecondsToUpgrade = NON_UPGRADE_TIME;

    public bool isDartfishNeutralized = false;
    public bool isCrabNeutralized = false;
    public bool isSeabassNeutralized = false;
    public bool isMackarelNeutralized = false;
    public bool isMorayNeutralized = false;
    public bool isLionfishNeutralized = false;
    public bool isBarracudaNeutralized = false;
    public bool isTunaNeutralized = false;
    public bool isHammerheadNeutralized = false;
    public bool isTigerNeutralized = false;
    public bool isWhiteNeutralized = false;
    public bool isCuttleNeutralized = false;
    public bool isPufferNeutralized = false;

    public bool this[FishLevel level] {
        get {
            return level switch {
                FishLevel.Dartfish => isDartfishNeutralized,
                FishLevel.Crab => isCrabNeutralized,
                FishLevel.SeaBass => isSeabassNeutralized,
                FishLevel.Mackerel => isMackarelNeutralized,
                FishLevel.Moray => isMorayNeutralized,
                FishLevel.Lionfish => isLionfishNeutralized,
                FishLevel.Barracuda => isBarracudaNeutralized,
                FishLevel.Tuna => isTunaNeutralized,
                FishLevel.HammerShark => isHammerheadNeutralized,
                FishLevel.TigerShark => isTigerNeutralized,
                FishLevel.WhiteShark => isWhiteNeutralized,
                FishLevel.Cuttlefish => isCuttleNeutralized,
                FishLevel.Pufferfish => isPufferNeutralized,
                _ => false
            };
        }
        set {
            switch (level) {
                case FishLevel.Dartfish:
                isDartfishNeutralized = value;
                break;
                case FishLevel.Crab:
                isCrabNeutralized = value;
                break;
                case FishLevel.SeaBass:
                isSeabassNeutralized = value;
                break;
                case FishLevel.Mackerel:
                isMackarelNeutralized = value;
                break;
                case FishLevel.Moray:
                isMorayNeutralized = value;
                break;
                case FishLevel.Lionfish:
                isLionfishNeutralized = value;
                break;
                case FishLevel.Barracuda:
                isBarracudaNeutralized = value;
                break;
                case FishLevel.Tuna:
                isTunaNeutralized = value;
                break;
                case FishLevel.HammerShark:
                isHammerheadNeutralized = value;
                break;
                case FishLevel.TigerShark:
                isTigerNeutralized = value;
                break;
                case FishLevel.WhiteShark:
                isWhiteNeutralized = value;
                break;
                case FishLevel.Cuttlefish:
                isCuttleNeutralized = value;
                break;
                case FishLevel.Pufferfish:
                isPufferNeutralized = value;
                break;
            }
        }
    }
}