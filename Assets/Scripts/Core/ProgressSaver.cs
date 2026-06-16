using System;
using System.IO;
using UnityEngine;


//save amnd load game data with encryption
public static class ProgressSaver {
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData_Player data) {
        string json = JsonUtility.ToJson(data);
        string encrypted = Encryptor.Encrypt(json);

        File.WriteAllText(SavePath, encrypted);
    }

    public static bool IsDataExist() {
        return File.Exists(SavePath);
    }

    public static SaveData_Player Load() {
        if (!IsDataExist())
            return null;

        string encrypted = File.ReadAllText(SavePath);
        string json = Encryptor.Decrypt(encrypted);

        return JsonUtility.FromJson<SaveData_Player>(json);
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
            SaveData_Player data = Load();
            if (data != null)
                return (data.isPremiumUser, data.isNoAD);
        }
        return (false, false);
    }

    public static bool Exists() => File.Exists(SavePath);
}

[Serializable]
public class SaveData_Player {
    private const int INVALID_VAL = -1;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerProgress = INVALID_VAL;
    public int playerLevel = INVALID_VAL;
    public int playerHealth = INVALID_VAL;

    public bool isPremiumUser = false;
    public bool isNoAD = false;
    public bool isFounder = false;

    public bool IsDataValid =>
        (playerPosition != null && playerRotation != null
        && playerLevel != INVALID_VAL && playerHealth != INVALID_VAL 
        && playerProgress != INVALID_VAL);
}
