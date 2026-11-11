using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using VContainer;




public class SettingsWnd : WindowBase, ISettingsController {
    [field: SerializeField] public Toggle BloomTog { get; private set; }
    [field: SerializeField] public Toggle BlurTog { get; private set; }
    [field: SerializeField] public Toggle VignetteTog { get; private set; }
    [field: SerializeField] public Toggle ShadowCastTog { get; private set; }
    [field: SerializeField] public Toggle AutoHideUI_tog { get; private set; }
    [field: SerializeField] public Slider ShadowDistSl { get; private set; }
    [field: SerializeField] public Slider ShadowFixSl { get; private set; }
    [field: SerializeField] public Slider GameResolutSl { get; private set; }
    [field: SerializeField] public Slider masterSlider { get; private set; }// Slider for master volume control
    [field: SerializeField] public Slider musicSlider { get; private set; }// Slider for music volume control
    [field: SerializeField] public Slider sfxSlider { get; private set; }// Slider for SFX volume control
    [field: SerializeField] public TMP_Dropdown ShadowResolution { get; private set; }
    [field: SerializeField] public TMP_Dropdown LanguageDropdown { get; private set; }
    [field: SerializeField] public TMP_Dropdown FramerateDropdown { get; private set; }

    public static bool IsAutoHideUI { get; private set; } = true;
    public static event Action OnHudHideChanged;

    [Inject] private readonly IAudioPlayService _audioPlayService;
    private int _previousLocaleIndex = 0;

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

    private void Awake() {
        RefreshUIValues();
        InitListeners();
    }

    void Start() {
        SettingsData data = ProgressSaver.LoadSettingsData();

        masterSlider.value = data.volume_master;
        musicSlider.value = data.volume_music;
        sfxSlider.value = data.volume_sfx;
    }

    private void OnDisable() {
        SaveAudioData();
    }

    //save audio data in normalize format
    public void SaveAudioData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        settingsData.volume_master = masterSlider.value;
        settingsData.volume_music = musicSlider.value;
        settingsData.volume_sfx = sfxSlider.value;
        ProgressSaver.SaveData(settingsData);
    }


    void InitListeners() {
        masterSlider.onValueChanged.AddListener(value =>
           _audioPlayService.MasterVolume = GameStaticInfo.SliderToDB(value, GameStaticInfo.MAX_MASTER_DB));

        musicSlider.onValueChanged.AddListener(value => {
            _audioPlayService.MusicVolume = GameStaticInfo.SliderToDB(value, GameStaticInfo.MAX_MUSIC_DB);
#if UNITY_ANDROID && !UNITY_EDITOR
            if (value == 0f)
                GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_hey_give_it_back);
#endif
        });

        sfxSlider.onValueChanged.AddListener(value =>
            _audioPlayService.SFXVolume = GameStaticInfo.SliderToDB(value, GameStaticInfo.MAX_SFX_DB));
    }


    void RefreshUIValues() {
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
            _previousLocaleIndex = savedSettings.language;
            LanguageDropdown.onValueChanged.RemoveAllListeners();
            LanguageDropdown.value = _previousLocaleIndex;
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
        _previousLocaleIndex = index;
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

