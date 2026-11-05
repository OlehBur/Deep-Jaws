using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.Audio;


public class RootLifetimeScope : LifetimeScopeBase {
    public static RootLifetimeScope Instance { get; private set; }

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioClipsInfo clipsInfo;
    [SerializeField] SanctuaryUIInfo sanctuaryUIInfo;
    [SerializeField] FishLocalizedInfo fishLocalizedInfo;
    [SerializeField] FishAllInfo fishInfoManager;
    [SerializeField] FishSpriteInfo fishSpriteInfo;



    protected override void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // for VContainer init container
        base.Awake();
    }

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
        builder.RegisterInstance(fishSpriteInfo);
        builder.RegisterInstance(fishInfoManager);
        builder.RegisterInstance(audioMixer);
    }

    public override void InitServices(IContainerBuilder builder) {
        builder.RegisterComponent(GetComponentInChildren<SanctuariesManager>(true)).As<ISanctuariesManager>();
        builder.RegisterComponent(GetComponentInChildren<AlertWindowsManager>(true)).As<IAlertWindowsManager>();
        builder.RegisterComponent(GetComponentInChildren<SettingsUI>(true)).As<ISettingsManager>();
        builder.RegisterComponent(GetComponentInChildren<SceneLoader>(true)).As<ISceneLoader>();
        builder.RegisterComponent(GetComponentInChildren<GameAdsManager>(true)).As<IAdsManager>();
        builder.RegisterComponent(GetComponentInChildren<AudioPlayManager>(true)).As<IAudioPlayService>();
        builder.RegisterComponent(GetComponentInChildren<GamePurchaseManager>(true)).As<IGamePurchaseManager>();
        builder.RegisterComponent(GetComponentInChildren<LicenseValidator>(true)).As<ILicenseValidator>();
        builder.RegisterComponent(GetComponentInChildren<GraphicManager>(true)).As<IGraphicManager>();

        //builder.RegisterComponentInHierarchy<SanctuariesManager>().As<ISanctuariesManager>();
        //builder.RegisterComponentInHierarchy<AlertWindowsManager>().As<IAlertWindowsManager>();
        //builder.RegisterComponentInHierarchy<SettingsUI>().As<ISettingsManager>();
        //builder.RegisterComponentInHierarchy<SceneLoader>().As<ISceneLoader>();
        //builder.RegisterComponentInHierarchy<GameAdsManager>().As<IAdsManager>();
        //builder.RegisterComponentInHierarchy<AudioPlayManager>().As<IAudioPlayService>();
        //builder.RegisterComponentInHierarchy<GamePurchaseManager>().As<IGamePurchaseManager>();
        //builder.RegisterComponentInHierarchy<GraphicManager>().As<IGraphicManager>();
        //builder.RegisterComponentInHierarchy<LicenseValidator>().As<ILicenseValidator>();

        //builder.RegisterComponent(alertWindowsManager).As<IAlertWindowsManager>();
        //builder.RegisterComponent(settingsManager).As<ISettingsManager>();
        //builder.RegisterComponent(sceneLoader).As<ISceneLoader>();
        //builder.RegisterComponent(gameAdsManager).As<IAdsManager>();
        //builder.RegisterComponent(audioPlayManager).As<IAudioPlayService>();
        //builder.RegisterComponent(gamePurchaseManager).As<IGamePurchaseManager>();
        //builder.RegisterComponent(graphicManager).As<IGraphicManager>();
        //builder.RegisterComponent(sanctuariesManager).As<ISanctuariesManager>();
        //builder.RegisterComponent(licenseValidator).As<ILicenseValidator>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {

    }

    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<LicenseValidator>(builder);

    }

}
