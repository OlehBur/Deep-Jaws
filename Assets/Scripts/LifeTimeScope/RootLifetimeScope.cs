using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.Audio;


public class RootLifetimeScope : LifetimeScopeBase {
    public static RootLifetimeScope Instance { get; private set; }

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioClipsInfo clipsInfo;
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] GameAdsManager gameAdsManager;
    [SerializeField] AudioPlayManager audioPlayManager;
    [SerializeField] GamePurchaseManager gamePurchaseManager;
    [SerializeField] GraphicManager graphicManager;
    [SerializeField] SanctuariesManager sanctuariesManager;


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
        builder.RegisterInstance(audioMixer);
    }

    public override void InitServices(IContainerBuilder builder) {
        builder.RegisterComponent(sceneLoader).As<ISceneLoader>();
        builder.RegisterComponent(gameAdsManager).As<IAdsManager>();
        builder.RegisterComponent(audioPlayManager).As<IAudioPlayService>();
        builder.RegisterComponent(gamePurchaseManager).As<IGamePurchaseManager>();
        builder.RegisterComponent(graphicManager).As<IGraphicManager>();
        builder.RegisterComponent(sanctuariesManager).As<ISanctuariesManager>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
    }

    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<MainMenuNavigation>(builder);
        SingleInject<LicenseValidator>(builder);

    }

}
