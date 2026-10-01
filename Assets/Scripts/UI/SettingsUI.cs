using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {
    [field: SerializeField] public Toggle BloomTog { get; private set; }
    [field: SerializeField] public Toggle BlurTog { get; private set; }
    [field: SerializeField] public Toggle VignetteTog { get; private set; }
    [field: SerializeField] public Toggle ShadowCastTog { get; private set; }
    [field: SerializeField] public Toggle AutoHideUI_tog { get; private set; }
    [field: SerializeField] public Slider ShadowDistSl { get; private set; }
    [field: SerializeField] public Slider ShadowFixSl { get; private set; }
    [field: SerializeField] public Slider GameResolutSl { get; private set; }
    [field: SerializeField] public TMP_Dropdown ShadowResolution { get; private set; }
    [field: SerializeField] public TMP_Dropdown LanguageDropdown { get; private set; }
    [field: SerializeField] public TMP_Dropdown FramerateDropdown { get; private set; }

    public static bool IsAutoHideUI { get; private set; } = true;
    public static event Action OnHudHideChanged;

    private int previousLocaleIndex = 0;

    public enum Framerate {
        Max,
        F30,
        F60,
        F120
    }

    // Automatic application of settings when starting the game
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoApplySettingsOnStartup() {
        //apply FPS and VSync synchronously (instantly, without waiting)
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
                      ? ProgressSaver.LoadSettingsData()
                      : new SettingsData();

        IsAutoHideUI = savedSettings.autoUI;

        QualitySettings.vSyncCount = 0;
        int maxFps = Screen.currentResolution.refreshRateRatio.value > 0
            ? (int)Screen.currentResolution.refreshRateRatio.value
            : 60;

        Application.targetFrameRate = savedSettings.framerate switch {
            0 => maxFps,
            1 => 30,
            2 => 60,
            3 => 120,
            _ => 30
        };

        // upload the localization through a safe background runner
        GameObject runner = new GameObject("[LocalizationInitializer]");
        DontDestroyOnLoad(runner);
        runner.AddComponent<LocalizationStartupRunner>();
    }

    private void OnEnable() {
        RefreshUIValues();
    }

    private void RefreshUIValues() {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        IsAutoHideUI = savedSettings.autoUI;

        if (AutoHideUI_tog != null) {
            AutoHideUI_tog.onValueChanged.RemoveAllListeners();
            AutoHideUI_tog.isOn = IsAutoHideUI;
            AutoHideUI_tog.onValueChanged.AddListener(OnAutoHideUIToggle);
        }

        if (LanguageDropdown != null) {
            previousLocaleIndex = savedSettings.language;
            LanguageDropdown.onValueChanged.RemoveAllListeners();
            LanguageDropdown.value = previousLocaleIndex;
            LanguageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (FramerateDropdown != null) {
            InitFramerateOptions();
            FramerateDropdown.onValueChanged.RemoveAllListeners();
            FramerateDropdown.SetValueWithoutNotify(savedSettings.framerate);
            FramerateDropdown.value = savedSettings.framerate;
            FramerateDropdown.onValueChanged.AddListener(OnFramerateChanged);
        }
    }

    private void InitFramerateOptions() {
        if (FramerateDropdown != null && FramerateDropdown.options.Count == 0) {
            List<string> options = new List<string> { "Max", "30 Hz", "60 Hz", "120 Hz" };
            FramerateDropdown.ClearOptions();
            FramerateDropdown.AddOptions(options);
        }
    }

    private void OnLanguageChanged(int index) {
        if (index >= 0 && index < LocalizationSettings.AvailableLocales.Locales.Count) {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }

        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        savedSettings.language = index;
        ProgressSaver.SaveData(savedSettings);

#if UNITY_ANDROID && !UNITY_EDITOR
        if (previousLocaleIndex != index)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_dont_understand);
#endif
        previousLocaleIndex = index;
    }

    private void OnFramerateChanged(int index) {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        savedSettings.framerate = index;
        ProgressSaver.SaveData(savedSettings);

        int maxFps = Screen.currentResolution.refreshRateRatio.value > 0
            ? (int)Screen.currentResolution.refreshRateRatio.value
            : 60;

        Application.targetFrameRate = index switch {
            0 => maxFps,
            1 => 30,
            2 => 60,
            3 => 120,
            _ => 30
        };
    }

    private void OnAutoHideUIToggle(bool toggle) {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        IsAutoHideUI = toggle;
        savedSettings.autoUI = IsAutoHideUI;
        OnHudHideChanged?.Invoke();

        ProgressSaver.SaveData(savedSettings);
    }
}

// helper class for safe language loading without frame freezes
public class LocalizationStartupRunner : MonoBehaviour {
    private IEnumerator Start() {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist
            ? ProgressSaver.LoadSettingsData()
            : new SettingsData();

        // wait for Unity Localization in frame mode (yield return instead of await)
        yield return LocalizationSettings.InitializationOperation;

        int localeIndex = savedSettings.language;
        if (localeIndex >= 0 && localeIndex < LocalizationSettings.AvailableLocales.Locales.Count) {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
            Debug.Log($"[LocalizationStartup] Successfully loaded locale index: {localeIndex}");
        }

        // self-destruct so as not to litter the memory
        Destroy(gameObject);
    }
}