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

    public static bool IsDataExist(IData data) =>
        File.Exists(data.SavePath);
    public static bool IsWorldDataExist => IsDataExist(new WorldData());
    public static bool IsAccountDataExist => IsDataExist(new AccountData());
    public static bool IsSettingsDataExist => IsDataExist(new SettingsData());

    public static T LoadData<T>(T data) where T : IData {
        if (!data.IsDataExist())
            return default;

        string encrypted = File.ReadAllText(data.SavePath);
        string json = Encryptor.Decrypt(encrypted);

        return JsonUtility.FromJson<T>(json);
    }
    public static WorldData LoadWorldData() => LoadData(new WorldData());
    public static AccountData LoadAccountData() => LoadData(new AccountData());
    public static SettingsData LoadSettingsData() => LoadData(new SettingsData());

    public static void DeleteData(IData data) =>
        data.DeleteData();
    public static void DeleteWorldData() => DeleteData(new WorldData());
    public static void DeleteAccountData() => DeleteData(new AccountData());
    public static void DeleteSettingsData() => DeleteData(new SettingsData());


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
    public bool vignette = true;
    public bool shadow_casting = true;
    public float shadow_distance = GameStaticInfo.SETTINGS_SHADOW_DIST_MULTIPLIER;
    public float shadow_fixer = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_FIX;
    public float game_resolution = GameStaticInfo.SETTINGS_DEFAYLT_GAME_RESOLUTION;
    public int shadow_resolution = GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_2;
    public int framerate = GameStaticInfo.DEFAULT_SCREEN_FRAMERATE;

    //language
    public int language = GameStaticInfo.SETTINGS_DEFAULT_LANGUAGE;
}

public class WorldData : IData {
    public string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    //game data
    public Vector3 playerPosition = Vector3.zero;
    public Quaternion playerRotation = Quaternion.identity;
    public float playerProgress = 0;
    public FishLevel playerLevel = 0;
    public int playerHealth = 0;

    //public bool IsWorldDataValid =>
    //(playerPosition != Vector3.zero && playerRotation != Quaternion.identity
    //&& playerLevel != IData.INVALID_VAL && playerHealth != IData.INVALID_VAL
    //&& playerProgress != IData.INVALID_VAL);

    //public void ResetData() {
    //    playerPosition = Vector3.zero;
    //    Quaternion playerRotation = Quaternion.identity;

    //    playerProgress = IData.INVALID_VAL;
    //    playerLevel = IData.INVALID_VAL;
    //    playerHealth = IData.INVALID_VAL;
    //}
}

[Serializable]
public class AccountData : IData {
    public string SavePath => Path.Combine(Application.persistentDataPath, "account.json");

    //player data
    public bool isPremiumUser = false;
    public bool isNoAD = false;
    public bool isFounder = false;

    public bool hasAshflowSkin = false;
    public bool hasMercurySkin = false;
    public bool hasChameleon = false;
    public Skin selectedSkin;

    public bool confirmedLoyalty = false;
    public bool isConfirmedNoInternetDisadvantages = false;//in case user first open app without internet connection
    public bool isLicensed = false;
}
