using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


public class SettingsUI : MonoBehaviour {
    [SerializeField] Toggle autoHideUI_tog;
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] TMP_Dropdown framerateDropdown;

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
        SettingsData savedSettings = ProgressSaver.IsSettingsDataExist?
            ProgressSaver.LoadSettingsData(): new SettingsData();
        IsAutoHideUI = savedSettings.autoUI;
    }

    private void Start() {
        SettingsData savedSettings = ProgressSaver.LoadSettingsData();
        if (framerateDropdown != null) {//if any settings field setted            
            LanguageInit(savedSettings);
            FramerateInit(savedSettings);
        }

        autoHideUI_tog.onValueChanged.AddListener(OnAutoHideUIToggle);
        autoHideUI_tog.isOn = savedSettings == null ?
            true : savedSettings.autoUI;
        autoHideUI_tog.onValueChanged.Invoke(autoHideUI_tog.isOn);
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

        languageDropdown.value = previousLocaleIndex;
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
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
        if (framerateDropdown.options.Count == 0) {
            fpsListing.Add(/*settingsMaxFpsLocalized.GetLocalizedString() +*/ $"{screenMaxFramerate} Hz");//max
            //Debug.LogWarning(screenMaxFramerate);
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_30)
                fpsListing.Add(GameStaticInfo.FRAMERATE_30.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_60)
                fpsListing.Add(GameStaticInfo.FRAMERATE_60.ToString() + " Hz");
            if (screenMaxFramerate >= GameStaticInfo.FRAMERATE_120)
                fpsListing.Add(GameStaticInfo.FRAMERATE_120.ToString() + " Hz");

            framerateDropdown.AddOptions(fpsListing);
        }

        framerateDropdown.SetValueWithoutNotify((int)currentFramerateIndex);
        framerateDropdown.onValueChanged.RemoveAllListeners();
        framerateDropdown.onValueChanged.AddListener(OnFramerateChanged);
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
