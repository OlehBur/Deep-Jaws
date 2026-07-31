using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicManager : MonoBehaviour {
    [Header("Graphics Settings")]
    [SerializeField] Light mainLight;
    [SerializeField] Volume mainVolume;
    [SerializeField] UniversalRenderPipelineAsset urpAsset;

    [Header("UI elements")]
    [SerializeField] Toggle bloomTog;
    [SerializeField] Toggle vignetteTog;
    [SerializeField] Toggle shadowCastTog;
    [SerializeField] Slider shadowDistSl;
    [SerializeField] Slider shadowFixSl;
    [SerializeField] Slider gameResolutSl;
    [SerializeField] TMP_Dropdown shadowResolution;

    //private const string BLOOM_KEY = "bloom_active";
    //private const string VIGNETTE_KEY = "vignette_active";
    //private const string SHADOWCAST_KEY = "shadow_active";
    //private const string SHADOWDIST_KEY = "shadow_distance";
    //private const string SHADOWFIX_KEY = "shadow_bias";
    //private const string GAMERESOLUTION_KEY = "resolution_scale";
    //private const string SHADOWRESOLUTION_KEY = "shadow_resolution";


    void Start() {
        bloomTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Bloom bloom))
                bloom.active = value;
            //Debug.LogWarning("Volume bloom trigger");
        });
        vignetteTog.onValueChanged.AddListener(value => {
            if (mainVolume.sharedProfile.TryGet(out Vignette vignette))
                vignette.active = value;
        });
        shadowCastTog.onValueChanged.AddListener(value => {
            mainLight.shadows = value ? LightShadows.Hard : LightShadows.None;
        });
        shadowDistSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDistance = value * GameStaticInfo.SETTINGS_SHADOW_DIST_MULTIPLIER;
        });
        shadowFixSl.onValueChanged.AddListener(value => {
            urpAsset.shadowDepthBias = value * GameStaticInfo.SETTINGS_SHADOW_FIX_MULTIPLIER;
        });
        gameResolutSl.onValueChanged.AddListener(value => {
            urpAsset.renderScale = value;
        });
        shadowResolution.onValueChanged.AddListener(value => {
            urpAsset.mainLightShadowmapResolution = value switch {
                0 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_0,
                1 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_1,
                2 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_2,
                3 => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_3,
                _ => GameStaticInfo.SETTINGS_SHADOWMAP_RESOLUTION_0,
            };
        });

        LoadGraphicsData();
    }

    public void ResetSettingsToDefault() {
        bloomTog.isOn = true;
        vignetteTog.isOn = true;
        shadowCastTog.isOn = true;
        shadowDistSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_DIST;
        shadowFixSl.value = GameStaticInfo.SETTINGS_DEFAULT_SHADOW_FIX;
        gameResolutSl.value = GameStaticInfo.SETTINGS_DEFAYLT_GAME_RESOLUTION;
        shadowResolution.value = GameStaticInfo.SETTINGS_DEFAULT_GAME_RESOLUTION_INDEX;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void LoadGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        bloomTog.isOn = settingsData.bloom;//(PlayerPrefs.GetInt(BLOOM_KEY, 1) == 1) ? true : false;
        bloomTog.onValueChanged.Invoke(bloomTog.isOn);

        vignetteTog.isOn = settingsData.vignette;//(PlayerPrefs.GetInt(VIGNETTE_KEY, 1) == 1) ? true : false;
        vignetteTog.onValueChanged.Invoke(vignetteTog.isOn);

        shadowCastTog.isOn = settingsData.shadow_casting;// (PlayerPrefs.GetInt(SHADOWCAST_KEY, 1) == 1) ? true : false;
        shadowCastTog.onValueChanged.Invoke(shadowCastTog.isOn);

        shadowDistSl.value = settingsData.shadow_distance;//PlayerPrefs.GetFloat(SHADOWDIST_KEY, DEFAULT_SHADOW_DIST);
        shadowFixSl.value = settingsData.shadow_fixer;// PlayerPrefs.GetFloat(SHADOWFIX_KEY, DEFAULT_SHADOW_FIX);
        gameResolutSl.value = settingsData.game_resolution;//PlayerPrefs.GetFloat(GAMERESOLUTION_KEY, DEFAYLT_GAME_RESOLUTION);
        shadowResolution.value = settingsData.shadow_resolution;//PlayerPrefs.GetInt(SHADOWRESOLUTION_KEY, DEFAULT_GAME_RESOLUTION_INDEX);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveGraphicsData() {
        SettingsData settingsData = ProgressSaver.IsSettingsDataExist ?
            ProgressSaver.LoadSettingsData() : new SettingsData();

        settingsData.bloom = bloomTog.isOn;
        settingsData.vignette = vignetteTog.isOn;
        settingsData.shadow_casting = shadowCastTog.isOn;
        settingsData.shadow_distance = shadowDistSl.value;
        settingsData.shadow_fixer = shadowFixSl.value;
        settingsData.game_resolution = gameResolutSl.value;
        settingsData.shadow_resolution = shadowResolution.value;

        ProgressSaver.SaveData(settingsData);
        //PlayerPrefs.SetInt(BLOOM_KEY, bloomTog.isOn ? 1 : 0);
        //PlayerPrefs.SetInt(VIGNETTE_KEY, vignetteTog.isOn ? 1 : 0);
        //PlayerPrefs.SetInt(SHADOWCAST_KEY, shadowCastTog.isOn ? 1 : 0);
        //PlayerPrefs.SetFloat(SHADOWDIST_KEY, shadowDistSl.value);
        //PlayerPrefs.SetFloat(SHADOWFIX_KEY, shadowFixSl.value);
        //PlayerPrefs.SetFloat(GAMERESOLUTION_KEY, gameResolutSl.value);
        //PlayerPrefs.SetInt(SHADOWRESOLUTION_KEY, shadowResolution.value);
    }

    //save gr data on exit
    //private void OnApplicationQuit() {
    //    SaveGraphicsData();
    //}
    private void OnDisable() {
        SaveGraphicsData();
    }
}
