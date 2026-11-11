using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.Audio;


public class RootLifetimeScope : LifetimeScopeBase {

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioClipsInfo clipsInfo;
    [SerializeField] SanctuaryUIInfo sanctuaryUIInfo;
    [SerializeField] FishLocalizationInfo fishLocalizedInfo;
    [SerializeField] UILocalizationInfo uiLocalizationInfo;
    [SerializeField] FishAllInfo fishInfoManager;
    [SerializeField] FishSpriteInfo fishSpriteInfo;
    [SerializeField] FishSkinInfo fishSkinInfo;


    //protected override void Awake() {
    //    base.Awake();
    //    Debug.Log($"Root Awake {GetInstanceID()}");
    //}

    protected override void Configure(IContainerBuilder builder) {
        InitAssets(builder);
        InitServices(builder);
        InitMultipleScripts(builder);
        InitSingleScripts(builder);
    }

    public override void InitAssets(IContainerBuilder builder) {
        builder.RegisterInstance(clipsInfo);
        builder.RegisterInstance(sanctuaryUIInfo);
        builder.RegisterInstance(fishLocalizedInfo);
        builder.RegisterInstance(uiLocalizationInfo);
        builder.RegisterInstance(fishSpriteInfo);
        builder.RegisterInstance(fishSkinInfo);
        builder.RegisterInstance(fishInfoManager);
        builder.RegisterInstance(audioMixer);
    }

    public override void InitServices(IContainerBuilder builder) {
        builder.Register<AudioSettingsLoader>(Lifetime.Singleton).As<IAudioSettingsLoader>();

        builder.RegisterComponent(GetComponentInChildren<GPG_Manager>(true)).As<IGPGManager>();
        builder.RegisterComponent(GetComponentInChildren<SanctuariesManager>(true)).As<ISanctuariesManager>();
        builder.RegisterComponent(GetComponentInChildren<AlertWindowsManager>(true)).As<IAlertWindowsManager>();
        builder.RegisterComponent(GetComponentInChildren<SettingsWnd>(true)).As<ISettingsController>();
        builder.RegisterComponent(GetComponentInChildren<SceneLoader>(true)).As<ISceneLoader>();
        builder.RegisterComponent(GetComponentInChildren<GameAdsManager>(true)).As<IAdsManager>();
        builder.RegisterComponent(GetComponentInChildren<AudioPlayManager>(true)).As<IAudioPlayService>();
        builder.RegisterComponent(GetComponentInChildren<GamePurchaseManager>(true)).As<IGamePurchaseManager>();
        builder.RegisterComponent(GetComponentInChildren<LicenseValidator>(true)).As<ILicenseValidator>();
        builder.RegisterComponent(GetComponentInChildren<GraphicManager>(true)).As<IGraphicManager>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {

    }

    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<LicenseValidator>(builder);
        SingleInject<DemoFinWnd>(builder);
    }

}
