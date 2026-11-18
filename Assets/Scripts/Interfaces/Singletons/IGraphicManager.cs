using UnityEngine;
using UnityEngine.Rendering;

public interface IGraphicManager {
    Color MainLightColor { get; set; }
    Volume Volume { get; }

    void SaveGraphicsData();
    void LoadGraphicsData();
    void ResetSettingsToDefault();
    void ResetLightColor();
}
