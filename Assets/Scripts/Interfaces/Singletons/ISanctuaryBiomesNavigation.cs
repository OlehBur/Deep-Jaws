using System;

public interface ISanctuaryBiomesNavigation {
    BiomeType CurrentBiomeType { get; }
    Action<BiomeType> OnBiomeChanged { get; set; }
}