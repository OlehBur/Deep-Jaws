using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;



public interface IGraphicManager {
    Color MainLightColor { get; set; }
    Volume Volume { get; }

    void SaveGraphicsData();
    void LoadGraphicsData();
    void ResetSettingsToDefault();
    void ResetLightColor();
}


public class GraphicManager : MonoBehaviour, IGraphicManager {
    [Header("Graphics Settings")]
    [SerializeField] Light MainLight;
    [SerializeField] Volume mainVolume;
    [SerializeField] UniversalRenderPipelineAsset urpAsset;

    //[Header("UI elements")]
    //[SerializeField] SettingsUI settingsUI;
    [Inject] private readonly ISettingsController _settingsUI;

    private Color _defaultLightColor;

    public Color MainLightColor {
        get => MainLight.color;
        set => MainLight.color = value;
    }
    public Volume Volume => mainVolume;


    void Start() {
        _defaultLightColor = MainLight.color;

        _settingsUI.BloomTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Bloom bloom))
                bloom.active = value;
            SaveGraphicsData();
            //Debug.LogWarning("Volume bloom trigger");
        });
        _settingsUI.BlurTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out DepthOfField blur))
                blur.active = value;
            SaveGraphicsData();
        });
        _settingsUI.VignetteTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Vignette vignette))
                vignette.active = value;
            SaveGraphicsData();
        });
        _settingsUI.ShadowCastTog.onValueChanged.AddListener(value => {
            MainLight.shadows = value ? LightShadows.Hard : LightShadows.None;
            SaveGraphicsData();
        });
        _settingsUI.ShadowDistSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDistance = value * GameStaticInfo.SETTINGS_SHADOW_DIST_MULTIPLIER;
            SaveGraphicsData();
        });
        _settingsUI.ShadowFixSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDepthBias = value * GameStaticInfo.SETTINGS_SHADOW_FIX_MULTIPLIER;
            SaveGraphicsData();
        });
        _settingsUI.GameResolutSl.onValueChanged.AddListener(value => {
            urpAsset.renderScale = value;
            SaveGraphicsData();
        });
        _settingsUI.ShadowResolution.onValueChanged.AddListener(value => {
            urpAsset.mainLightShadowmapResolution = value switch {
                0 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_0,
                1 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_1,
                2 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_2,
                3 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_3,
                _ => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_0,
            };
            SaveGraphicsData();
        });

        LoadGraphicsData();
    }

    public void ResetSettingsToDefault() {
        _settingsUI.BloomTog.isOn = true;
        _settingsUI.BlurTog.isOn = true;
        _settingsUI.VignetteTog.isOn = true;
        _settingsUI.ShadowCastTog.isOn = true;
        _settingsUI.ShadowDistSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_DIST;
        _settingsUI.ShadowFixSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_FIX;
        _settingsUI.GameResolutSl.value = GameStaticInfo.SETTINGS_DEFAYLT_GAME_RESOLUTION;
        _settingsUI.ShadowResolution.value = GameStaticInfo.SETTINGS_DEFAULT_GAME_RESOLUTION_INDEX;
        _settingsUI.FramerateDropdown.value = (int)SettingsWnd.Framerate.Max;
        _settingsUI.AutoHideUI_tog.isOn = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        _settingsUI.BloomTog.isOn = settingsData.bloom;//(PlayerPrefs.GetInt(BLOOM_KEY, 1) == 1) ? true : false;
        _settingsUI.BloomTog.onValueChanged.Invoke(_settingsUI.BloomTog.isOn);

        _settingsUI.BlurTog.isOn = settingsData.blur;
        _settingsUI.BlurTog.onValueChanged.Invoke(_settingsUI.BlurTog.isOn);
        _settingsUI.VignetteTog.isOn = settingsData.vignette;//(PlayerPrefs.GetInt(VIGNETTE_KEY, 1) == 1) ? true : false;
        _settingsUI.VignetteTog.onValueChanged.Invoke(_settingsUI.VignetteTog.isOn);

        _settingsUI.ShadowCastTog.isOn = settingsData.shadow_casting;// (PlayerPrefs.GetInt(SHADOWCAST_KEY, 1) == 1) ? true : false;
        _settingsUI.ShadowCastTog.onValueChanged.Invoke(_settingsUI.ShadowCastTog.isOn);

        _settingsUI.ShadowDistSl.value = settingsData.shadow_distance;//PlayerPrefs.GetFloat(SHADOWDIST_KEY, DEFAULT_SHADOW_DIST);
        _settingsUI.ShadowFixSl.value = settingsData.shadow_fixer;// PlayerPrefs.GetFloat(SHADOWFIX_KEY, DEFAULT_SHADOW_FIX);
        _settingsUI.GameResolutSl.value = settingsData.game_resolution;//PlayerPrefs.GetFloat(GAMERESOLUTION_KEY, DEFAYLT_GAME_RESOLUTION);
        _settingsUI.ShadowResolution.value = settingsData.shadow_resolution;//PlayerPrefs.GetInt(SHADOWRESOLUTION_KEY, DEFAULT_GAME_RESOLUTION_INDEX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        settingsData.bloom = _settingsUI.BloomTog.isOn;
        settingsData.blur = _settingsUI.BlurTog.isOn;
        settingsData.vignette = _settingsUI.VignetteTog.isOn;
        settingsData.shadow_casting = _settingsUI.ShadowCastTog.isOn;
        settingsData.shadow_distance = _settingsUI.ShadowDistSl.value;
        settingsData.shadow_fixer = _settingsUI.ShadowFixSl.value;
        settingsData.game_resolution = _settingsUI.GameResolutSl.value;
        settingsData.shadow_resolution = _settingsUI.ShadowResolution.value;

        ProgressSaver.SaveData(settingsData);
    }

    public void ResetLightColor() {
        MainLight.color = _defaultLightColor;
    }

    //save gr data on exit
    //private void OnApplicationQuit() {
    //    SaveGraphicsData();
    //}
    private void OnDisable() {
        SaveGraphicsData();
    }
}
