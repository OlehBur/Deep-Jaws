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

    public static void Delete() {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static bool Exists() => File.Exists(SavePath);
}

[Serializable]
public class SaveData {
    private const int UNVALID_VAL = -1;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerProgress = UNVALID_VAL;
    public int playerLevel = UNVALID_VAL;
    public int playerHealth = UNVALID_VAL;
    public bool IsDataValid =>
        (playerPosition != null && playerRotation != null
        && playerLevel != UNVALID_VAL && playerHealth != UNVALID_VAL && playerProgress != UNVALID_VAL);
}
