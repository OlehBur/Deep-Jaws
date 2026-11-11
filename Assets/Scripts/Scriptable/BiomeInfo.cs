using UnityEngine;
using UnityEngine.Localization;


public enum BiomeType {
    CoralReef = 1,
    CliffsSeaweed = 2,
    Pelagial = 3,
    OceanicAbyss = 4
}


[CreateAssetMenu(fileName = "BiomeInfo", menuName = "Scriptable Objects/BiomeInfo")]
public class BiomeInfo : ScriptableObject {
    [field: SerializeField] public BiomeType Type { get; private set; }
    [field: SerializeField] public LocalizedString Tittle { get; private set; }
    [field: SerializeField] public int MaxLevel { get; private set; }
    [field: SerializeField] public float[] UpgradeTimeByLevel_Hours { get; private set; }
    [field: SerializeField] public int[] LevelPrices { get; private set; }
    [field: SerializeField] public FishLevel[] Requirements { get; private set; }

    public float GetUpgradeTimeByLevel_Hours(int level) {
        float boostMultiplier = 1f;

#if UNITY_STANDALONE_WIN
        boostMultiplier = .33f;
#endif
        if (level < UpgradeTimeByLevel_Hours.Length)
            return UpgradeTimeByLevel_Hours[level] * boostMultiplier;
        else
            return 0f;
    }
}