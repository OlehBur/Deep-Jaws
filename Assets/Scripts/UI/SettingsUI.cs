using System;
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
    private Framerate currentFramerateIndex;
    private enum Framerate {
        Max,
        F30,
        F60,
        F120
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //static void ClearStatics() {
    //    IsAutoHideUI = true;
    //}

    private void Awake() {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();
        IsAutoHideUI = savedSettings.autoUI;
    }

    private void Start() {
        SettingsData savedSettings = ProgressSaver.LoadSettingsData();
        if (FramerateDropdown != null) {//if any settings field setted            
            LanguageInit(savedSettings);
            FramerateInit(savedSettings);
        }

        //toggle init
        AutoHideUI_tog.onValueChanged.AddListener(OnAutoHideUIToggle);
        AutoHideUI_tog.isOn = IsAutoHideUI;
        AutoHideUI_tog.onValueChanged.Invoke(AutoHideUI_tog.isOn);
    }


    async void LanguageInit(SettingsData savedSettings) {
        await LocalizationSettings.InitializationOperation.Task; // wait readiness

        previousLocaleIndex = savedSettings == null ?
            (int)GameStaticInfo.LocalizationID.English : savedSettings.language;
        //PlayerPrefs.GetInt(LOCALE_PREF_KEY, (int)GameStaticInfo.LocalizationID.English);

        if (previousLocaleIndex < 0 || previousLocaleIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
            previousLocaleIndex = 0;

        if (LocalizationSettings.AvailableLocales.Locales.Count > previousLocaleIndex)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[previousLocaleIndex];

        LanguageDropdown.value = previousLocaleIndex;
        LanguageDropdown.onValueChanged.RemoveAllListeners();
        LanguageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    void FramerateInit(SettingsData savedSettings) {
        List<string> fpsListing = new();
        QualitySettings.vSyncCount = 0;
        int screenMaxFramerate = GameManager.ScreenMaxFramerate;
        //framerateDropdown.ClearOptions();
        //Debug.LogWarning("DROPDOWN ALWAYS CLEANING UP");

        //Load and set Def val
        currentFramerateIndex = savedSettings == null ? Framerate.Max : (Framerate)savedSettings.framerate;
        //currentFramerateIndex = (Framerate)PlayerPrefs.GetInt(FPS_KEY, 0);
        Application.targetFrameRate = currentFramerateIndex switch {
            Framerate.Max => screenMaxFramerate,
            Framerate.F30 => GameStaticInfo.FRAMERATE_30,
            Framerate.F60 => GameStaticInfo.FRAMERATE_60,
            Framerate.F120 => GameStaticInfo.FRAMERATE_120,
            _ => GameStaticInfo.FRAMERATE_30
        };

        //init dropdown
        if (FramerateDropdown.options.Count == 0) {
            fpsListing.Add(/*settingsMaxFpsLocalized.GetLocalizedString() +*/ $"{screenMaxFramerate} Hz");//max
            //Debug.LogWarning(screenMaxFramerate);
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_30)
                fpsListing.Add(GameStaticInfo.FRAMERATE_30.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_60)
                fpsListing.Add(GameStaticInfo.FRAMERATE_60.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_120)
                fpsListing.Add(GameStaticInfo.FRAMERATE_120.ToString() + " Hz");

            FramerateDropdown.AddOptions(fpsListing);
        }

        FramerateDropdown.SetValueWithoutNotify((int)currentFramerateIndex);
        FramerateDropdown.onValueChanged.RemoveAllListeners();
        FramerateDropdown.onValueChanged.AddListener(OnFramerateChanged);
    }

    private void OnLanguageChanged(int index) {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();
        savedSettings.language = index;

        ProgressSaver.SaveData(savedSettings);
        //PlayerPrefs.SetInt(LOCALE_PREF_KEY, index); // Save the selected locale index

        if (previousLocaleIndex != index)
            GPG_Manager.OnTriggerAchievement(GPGSIds.achievement_i_dont_understand);

        previousLocaleIndex = index;
    }

    private void OnFramerateChanged(int index) {
        currentFramerateIndex = (Framerate)index;
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();
        savedSettings.framerate = index;

        ProgressSaver.SaveData(savedSettings);
        //PlayerPrefs.SetInt(FPS_KEY, index);

        Application.targetFrameRate = currentFramerateIndex switch {
            Framerate.Max => GameManager.ScreenMaxFramerate,
            Framerate.F30 => 30,
            Framerate.F60 => 60,
            Framerate.F120 => 120,
            _ => 30
        };
    }

    void OnAutoHideUIToggle(bool toggle) {
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        IsAutoHideUI = toggle;
        savedSettings.autoUI = IsAutoHideUI;
        OnHudHideChanged?.Invoke();

        ProgressSaver.SaveData(savedSettings);
    }
}
