using System;
using System.IO;
using UnityEngine;


//save amnd load game data with encryption
public static class ProgressSaver {
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data) {
        string json = JsonUtility.ToJson(data);
        string encrypted = Encryptor.Encrypt(json);

        File.WriteAllText(SavePath, encrypted);
    }

    public static bool IsDataExist() {
        return File.Exists(SavePath);
    }

    public static SaveData Load() {
        if (!IsDataExist())
            return null;

        string encrypted = File.ReadAllText(SavePath);
        string json = Encryptor.Decrypt(encrypted);

        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteOnlyGameData() {
        if (File.Exists(SavePath)) {
            SaveData data = Load();

            data.ResetGameData();
            Save(data);
        }
    }

    public static void Delete() {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    /// <summary>
    /// premium - delete interstitial and rewardeed ads, noAds - only del interstitial
    /// </summary>
    /// <returns></returns>
    public static (bool premium, bool noAds) IsPlayerHasAdImmunitet() {
        if (IsDataExist()) {
            SaveData data = Load();
            if (data != null)
                return (data.isPremiumUser, data.isNoAD);
        }
        return (false, false);
    }

    public static bool Exists() => File.Exists(SavePath);
}

[Serializable]
public class SaveData {
    private const int INVALID_VAL = -1;

    //game data
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerProgress = INVALID_VAL;
    public int playerLevel = INVALID_VAL;
    public int playerHealth = INVALID_VAL;

    //player data
    public bool isPremiumUser = false;
    public bool isNoAD = false;
    public bool isFounder = false;

    public bool hasAshflowSkin = false;
    public bool hasMercurySkin = false;
    public bool hasChameleon = false;
    public Skin selectedSkin;

    public bool confirmedLoyalty = false;

    public bool IsGameDataValid =>
        (playerPosition != null && playerRotation != null
        && playerLevel != INVALID_VAL && playerHealth != INVALID_VAL
        && playerProgress != INVALID_VAL);

    public void ResetGameData() {
        playerPosition = Vector3.zero;
        Quaternion playerRotation = Quaternion.identity;

        playerProgress = INVALID_VAL;
        playerLevel = INVALID_VAL;
        playerHealth = INVALID_VAL;
    }
}
