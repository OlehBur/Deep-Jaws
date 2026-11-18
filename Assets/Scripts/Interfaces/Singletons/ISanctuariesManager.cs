using System;

public interface ISanctuariesManager {
    event Action<int> OnShellsAmountChanged;
    event Action<BiomeType, int> OnResearchUpdated;
    event Action<BiomeType, int> OnBiomeResearched;

    int ShellsCnt { get; }
    BiomeWrapper CoralReef { get; }
    BiomeWrapper CliffsSeaweed { get; }
    BiomeWrapper Pelagial { get; }
    BiomeWrapper OceanicAbyss { get; }
    SanctuaryData SanctuaryData { get; }

    BiomeWrapper GetBiomeWrapper(BiomeType type);
    void BoostResearch(BiomeType type);
    void OnAddShells(int shellsCnt);
    void SaveSanctuaryNueutralizedData(FishLevel fish);
}