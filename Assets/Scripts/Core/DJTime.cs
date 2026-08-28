using System;
using UnityEngine;

public static class DJTime {
    private static float timeScale = 1.0f;

    public static event Action<float> OnTimeScaleChanged;

    public static float TimeScale {
        get => timeScale;
        set {
            if (Mathf.Approximately(timeScale, value))
                return;
            timeScale = value;

            OnTimeScaleChanged?.Invoke(timeScale);
        }
    }
    public static void SetFixedTime(float fixedDelta) =>
        Time.fixedDeltaTime = fixedDelta;

    public static float DeltaTime => Time.deltaTime * timeScale;
    public static float FixedDeltaTime => Time.fixedDeltaTime * timeScale;
    public static float UnscaledDeltaTime => Time.unscaledDeltaTime;
}
