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
    //[SerializeField] LightShadows shadowType = LightShadows.Hard;
    //[SerializeField] UnityEngine.Rendering.Universal.ShadowResolution shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
    //[SerializeField][Range(0f, 100f)] float ShadowDistance = 50f;
    //[SerializeField][Range(0f, 1f)] float RenderScale = .8f;
    //[SerializeField] bool BloomActive = true;
    //[SerializeField] bool VignetteActive = true;

    [Header("UI elements")]
    [SerializeField] Toggle bloomTog;
    [SerializeField] Toggle vignetteTog;
    [SerializeField] Toggle shadowCastTog;
    [SerializeField] Slider shadowDistSl;
    [SerializeField] Slider shadowFixSl;
    [SerializeField] Slider gameResolutSl;
    [SerializeField] TMP_Dropdown shadowResolution;
    private const float SHADOW_DIST_MULTIPLIER = 100f;
    private const float DEFAULT_SHADOW_DIST = .5f;
    private const float DEFAYLT_GAME_RESOLUTION = .8f;
    private const int DEFAULT_GAME_RESOLUTION_INDEX = 2;
    private const float SHADOW_FIX_MULTIPLIER = 10f;
    private const float DEFAULT_SHADOW_FIX = .1f;

    private const string BLOOM_KEY = "bloom_active";
    private const string VIGNETTE_KEY = "vignette_active";
    private const string SHADOWCAST_KEY = "shadow_active";
    private const string SHADOWDIST_KEY = "shadow_distance";
    private const string SHADOWFIX_KEY = "shadow_bias";
    private const string GAMERESOLUTION_KEY = "resolution_scale";
    private const string SHADOWRESOLUTION_KEY = "shadow_resolution";


    void Start() {
        bloomTog.onValueChanged.AddListener(value =>
        {
            if (mainVolume.profile.TryGet(out Bloom bloom))
                bloom.active = value;
        });
        vignetteTog.onValueChanged.AddListener(value =>
        {
            if (mainVolume.profile.TryGet(out Vignette vignette))
                vignette.active = value;
        });
        shadowCastTog.onValueChanged.AddListener(value =>
        {
            mainLight.shadows = value ? LightShadows.Hard : LightShadows.None;
        });
        shadowDistSl.onValueChanged.AddListener(value =>
        {
            urpAsset.shadowDistance = value * SHADOW_DIST_MULTIPLIER;
        });
        shadowFixSl.onValueChanged.AddListener(value =>
        {
            urpAsset.shadowDepthBias = value * SHADOW_FIX_MULTIPLIER;
        });
        gameResolutSl.onValueChanged.AddListener(value =>
        {
            urpAsset.renderScale = value;
        });
        shadowResolution.onValueChanged.AddListener(value =>
        {
            urpAsset.mainLightShadowmapResolution = value switch {
                0 => 256,
                1 => 512,
                2 => 1024,
                3 => 2048,
                _ => 1024,
            };
        });

        LoadGraphicsData();
    }

    public void ResetSettingsToDefault() {         
        bloomTog.isOn = true;
        vignetteTog.isOn = true;
        shadowCastTog.isOn = true;
        shadowDistSl.value = DEFAULT_SHADOW_DIST;
        shadowFixSl.value = DEFAULT_SHADOW_FIX;
        gameResolutSl.value = DEFAYLT_GAME_RESOLUTION;
        shadowResolution.value = DEFAULT_GAME_RESOLUTION_INDEX;
    }

    void LoadGraphicsData() {
        bloomTog.isOn = (PlayerPrefs.GetInt(BLOOM_KEY, 1) == 1) ? true : false;
        bloomTog.onValueChanged.Invoke(bloomTog.isOn);
        vignetteTog.isOn = (PlayerPrefs.GetInt(VIGNETTE_KEY, 1) == 1) ? true : false;
        vignetteTog.onValueChanged.Invoke(vignetteTog.isOn);
        shadowCastTog.isOn = (PlayerPrefs.GetInt(SHADOWCAST_KEY, 1) == 1) ? true : false;
        shadowCastTog.onValueChanged.Invoke(shadowCastTog.isOn);
        shadowDistSl.value = PlayerPrefs.GetFloat(SHADOWDIST_KEY, DEFAULT_SHADOW_DIST);
        shadowFixSl.value = PlayerPrefs.GetFloat(SHADOWFIX_KEY, DEFAULT_SHADOW_FIX);
        gameResolutSl.value = PlayerPrefs.GetFloat(GAMERESOLUTION_KEY, DEFAYLT_GAME_RESOLUTION);
        shadowResolution.value = PlayerPrefs.GetInt(SHADOWRESOLUTION_KEY, DEFAULT_GAME_RESOLUTION_INDEX);
    }

    public void SaveGraphicsData() {
        PlayerPrefs.SetInt(BLOOM_KEY, bloomTog.isOn ? 1 : 0);
        PlayerPrefs.SetInt(VIGNETTE_KEY, vignetteTog.isOn ? 1 : 0);
        PlayerPrefs.SetInt(SHADOWCAST_KEY, shadowCastTog.isOn ? 1 : 0);
        PlayerPrefs.SetFloat(SHADOWDIST_KEY, shadowDistSl.value);
        PlayerPrefs.SetFloat(SHADOWFIX_KEY, shadowFixSl.value);
        PlayerPrefs.SetFloat(GAMERESOLUTION_KEY, gameResolutSl.value);
        PlayerPrefs.SetInt(SHADOWRESOLUTION_KEY, shadowResolution.value);
    }

    //save gr data on exit
    private void OnApplicationQuit() {
        SaveGraphicsData();
    }
    private void OnDisable() {
        SaveGraphicsData();
    }

    //private void OnValidate() {
    ////UniversalRenderPipelineAsset urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
    ////if (urpAsset != null) {
    ////urpAsset.castS
    //mainLight.shadows = shadowType;
    //urpAsset.mainLightShadowmapResolution = (int)shadowResolution;
    //urpAsset.shadowDistance = ShadowDistance;
    //urpAsset.renderScale = RenderScale;

    //if (mainVolume.profile.TryGet(out Bloom bloom))
    //    bloom.active = BloomActive;
    //if (mainVolume.profile.TryGet(out Vignette vignette))
    //    vignette.active = VignetteActive;

    //Debug.Log("URP Asset modified at runtime");
    ////}
    ////QualitySettings.shadows = shadowQuality;//ShadowQuality.All; //All або Disable, HardOnly, SoftOnly
    ////QualitySettings.shadowDistance = shadowDistance;//100f; // максимальна дистанція тіней
    ////QualitySettings.shadowResolution = shadowResolution;//ShadowResolution.High; //High або Low, Medium, 
    ////Debug.Log("On Validate Shadow settings");
    //}

    // Update is called once per frame

}
