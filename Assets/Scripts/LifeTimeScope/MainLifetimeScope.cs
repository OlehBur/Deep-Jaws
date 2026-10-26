using VContainer;
using VContainer.Unity;
using UnityEngine;


public class MainLifetimeScope : LifetimeScope {
    [SerializeField] FishPrefabsInfo fishPrefabs;
    [SerializeField] VFX_PrefabsInfo vfxPrefabsInfo;
    [SerializeField] FishSkinInfo fishSkinInfo;


    protected override void Configure(IContainerBuilder builder) {
        InitInjectParams(builder);
        InitServices(builder);

        InjectMultipleScripts(builder);
        InjectSingleScripts(builder);

        //builder.RegisterComponentInHierarchy<HydraController>();

        //builder.RegisterComponentInHierarchy<ZoneController>(); //implemented in auto inject (component config on scene)
    }

    void InitInjectParams(IContainerBuilder builder) {
        //inject params
        builder.RegisterInstance(fishPrefabs);
        builder.RegisterInstance(vfxPrefabsInfo);
        builder.RegisterInstance(fishSkinInfo);
    }

    void InitServices(IContainerBuilder builder) {
        //services
        //builder.Register<IGameTip, GameTipsService>(Lifetime.Singleton);
        builder.Register<IVFXPoolService, VFXPoolService>(Lifetime.Singleton);
        builder.Register<IPlayerFishPoolService, PlayerPoolService>(Lifetime.Singleton);
        builder.Register<IEnemyFishPoolService, EnemyPoolService>(Lifetime.Singleton);
        builder.Register<IPermamentFishPoolService, PermamentPoolService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<IGameTip>();
        //builder.RegisterComponentInHierarchy<IMainmenuManager/*MainmenuManager*/>();
        builder.RegisterComponentInHierarchy<IAudioManager/*AudioManager*/>();
        builder.RegisterComponentInHierarchy<ISpawnOrchestrator/*SpawnOrchestrator*/>();
        //builder.RegisterComponentInHierarchy<VFXpoolService>();
    }

    void InjectMultipleScripts(IContainerBuilder builder) {
        //scripts to inject
        MultipleInject<HydraController>(builder);
        MultipleInject<HealerFruitController>(builder);
        MultipleInject<CaviarController>(builder);
        MultipleInject<EchoaquaController>(builder);
        MultipleInject<JellyfishController>(builder);
        MultipleInject<SlowdownPearlController>(builder);
        MultipleInject<SonicPulseControl>(builder);
        MultipleInject<ZoneController>(builder);
    }

    void InjectSingleScripts(IContainerBuilder builder) {
        SingInject<NoEscapeAchieveTrig>(builder);
        SingInject<PlayerLeaderboardScore>(builder);
        SingInject<CameraFollower>(builder);
        SingInject<SoundtrackController>(builder);
        SingInject<DeadArea>(builder);
        SingInject<FishHeartbeatsManager>(builder);
        SingInject<GameManager>(builder);
        SingInject<MainmenuManager>(builder);
        SingInject<PlayerUIController>(builder);
    }

    void MultipleInject<T>(IContainerBuilder builder) where T : Object {
        T[] allScripts = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        builder.RegisterBuildCallback(container => {
            foreach (T script in allScripts)
                container.Inject(script);
        });
    }

    void SingInject<T>(IContainerBuilder builder) where T : Object {
        T script = FindAnyObjectByType<T>();

        if (script != null)
            builder.RegisterBuildCallback(container =>
                container.Inject(script));
    }
}
