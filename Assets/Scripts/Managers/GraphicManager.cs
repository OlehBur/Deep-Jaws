using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;



public interface IGraphicManager {
    public void SaveGraphicsData();
    public void LoadGraphicsData();
    public void ResetSettingsToDefault();
}


public class GraphicManager : MonoBehaviour, IGraphicManager {
    [Header("Graphics Settings")]
    [SerializeField] Light mainLight;
    [SerializeField] Volume mainVolume;
    [SerializeField] UniversalRenderPipelineAsset urpAsset;

    //[Header("UI elements")]
    //[SerializeField] SettingsUI settingsUI;
    [Inject] private readonly ISettingsManager settingsUI;


    void Start() {
        settingsUI.BloomTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Bloom bloom))
                bloom.active = value;
            SaveGraphicsData();
            //Debug.LogWarning("Volume bloom trigger");
        });
        settingsUI.BlurTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out DepthOfField blur))
                blur.active = value;
            SaveGraphicsData();
        });
        settingsUI.VignetteTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Vignette vignette))
                vignette.active = value;
            SaveGraphicsData();
        });
        settingsUI.ShadowCastTog.onValueChanged.AddListener(value => {
            mainLight.shadows = value ? LightShadows.Hard : LightShadows.None;
            SaveGraphicsData();
        });
        settingsUI.ShadowDistSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDistance = value * GameStaticInfo.SETTINGS_SHADOW_DIST_MULTIPLIER;
            SaveGraphicsData();
        });
        settingsUI.ShadowFixSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDepthBias = value * GameStaticInfo.SETTINGS_SHADOW_FIX_MULTIPLIER;
            SaveGraphicsData();
        });
        settingsUI.GameResolutSl.onValueChanged.AddListener(value => {
            urpAsset.renderScale = value;
            SaveGraphicsData();
        });
        settingsUI.ShadowResolution.onValueChanged.AddListener(value => {
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
        settingsUI.BloomTog.isOn = true;
        settingsUI.BlurTog.isOn = true;
        settingsUI.VignetteTog.isOn = true;
        settingsUI.ShadowCastTog.isOn = true;
        settingsUI.ShadowDistSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_DIST;
        settingsUI.ShadowFixSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_FIX;
        settingsUI.GameResolutSl.value = GameStaticInfo.SETTINGS_DEFAYLT_GAME_RESOLUTION;
        settingsUI.ShadowResolution.value = GameStaticInfo.SETTINGS_DEFAULT_GAME_RESOLUTION_INDEX;
        settingsUI.FramerateDropdown.value = (int)SettingsUI.Framerate.Max;
        settingsUI.AutoHideUI_tog.isOn = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        settingsUI.BloomTog.isOn = settingsData.bloom;//(PlayerPrefs.GetInt(BLOOM_KEY, 1) == 1) ? true : false;
        settingsUI.BloomTog.onValueChanged.Invoke(settingsUI.BloomTog.isOn);

        settingsUI.BlurTog.isOn = settingsData.blur;
        settingsUI.BlurTog.onValueChanged.Invoke(settingsUI.BlurTog.isOn);
        settingsUI.VignetteTog.isOn = settingsData.vignette;//(PlayerPrefs.GetInt(VIGNETTE_KEY, 1) == 1) ? true : false;
        settingsUI.VignetteTog.onValueChanged.Invoke(settingsUI.VignetteTog.isOn);

        settingsUI.ShadowCastTog.isOn = settingsData.shadow_casting;// (PlayerPrefs.GetInt(SHADOWCAST_KEY, 1) == 1) ? true : false;
        settingsUI.ShadowCastTog.onValueChanged.Invoke(settingsUI.ShadowCastTog.isOn);

        settingsUI.ShadowDistSl.value = settingsData.shadow_distance;//PlayerPrefs.GetFloat(SHADOWDIST_KEY, DEFAULT_SHADOW_DIST);
        settingsUI.ShadowFixSl.value = settingsData.shadow_fixer;// PlayerPrefs.GetFloat(SHADOWFIX_KEY, DEFAULT_SHADOW_FIX);
        settingsUI.GameResolutSl.value = settingsData.game_resolution;//PlayerPrefs.GetFloat(GAMERESOLUTION_KEY, DEFAYLT_GAME_RESOLUTION);
        settingsUI.ShadowResolution.value = settingsData.shadow_resolution;//PlayerPrefs.GetInt(SHADOWRESOLUTION_KEY, DEFAULT_GAME_RESOLUTION_INDEX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        settingsData.bloom = settingsUI.BloomTog.isOn;
        settingsData.blur = settingsUI.BlurTog.isOn;
        settingsData.vignette = settingsUI.VignetteTog.isOn;
        settingsData.shadow_casting = settingsUI.ShadowCastTog.isOn;
        settingsData.shadow_distance = settingsUI.ShadowDistSl.value;
        settingsData.shadow_fixer = settingsUI.ShadowFixSl.value;
        settingsData.game_resolution = settingsUI.GameResolutSl.value;
        settingsData.shadow_resolution = settingsUI.ShadowResolution.value;

        ProgressSaver.SaveData(settingsData);
    }

    //save gr data on exit
    //private void OnApplicationQuit() {
    //    SaveGraphicsData();
    //}
    private void OnDisable() {
        SaveGraphicsData();
    }
}
