using VContainer;
using VContainer.Unity;
using UnityEngine;



public class GameLifetimeScope : LifetimeScopeBase {
    [SerializeField] FishPrefabsInfo fishPrefabs;
    [SerializeField] VFX_PrefabsInfo vfxPrefabsInfo;
    [SerializeField] FishSkinInfo fishSkinInfo;


    protected override void Configure(IContainerBuilder builder) {
        InitAssets(builder);
        InitServices(builder);

        InitMultipleScripts(builder);
        InitSingleScripts(builder);

        //builder.RegisterComponentInHierarchy<HydraController>();

        //builder.RegisterComponentInHierarchy<ZoneController>(); //implemented in auto inject (component config on scene)
    }

    public override void InitAssets(IContainerBuilder builder) {
        //inject params
        builder.RegisterInstance(fishPrefabs);
        builder.RegisterInstance(vfxPrefabsInfo);
        builder.RegisterInstance(fishSkinInfo);
    }

    public override void InitServices(IContainerBuilder builder) {
        //services
        //builder.Register<IGameTip, GameTipsService>(Lifetime.Singleton);
        builder.Register<IVFXPoolService, VFXPoolService>(Lifetime.Singleton);
        builder.Register<IPlayerFishPoolService, PlayerPoolService>(Lifetime.Singleton);
        builder.Register<IEnemyFishPoolService, EnemyPoolService>(Lifetime.Singleton);
        builder.Register<IPermamentFishPoolService, PermamentPoolService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<IGameTip>();
        //builder.RegisterComponentInHierarchy<IMainmenuManager/*MainmenuManager*/>();
        //builder.RegisterComponentInHierarchy<IAudioPlayService/*AudioManager*/>();
        builder.RegisterComponentInHierarchy<ISpawnOrchestrator/*SpawnOrchestrator*/>();
        //builder.RegisterComponentInHierarchy<VFXpoolService>();
        builder.RegisterComponentInHierarchy<IGameHudController>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
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

    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<NoEscapeAchieveTrig>(builder);
        SingleInject<PlayerLeaderboardScore>(builder);
        SingleInject<CameraFollower>(builder);
        SingleInject<SoundtrackController>(builder);
        SingleInject<DeadArea>(builder);
        SingleInject<FishHeartbeatsManager>(builder);
        SingleInject<GameManager>(builder);
        SingleInject<MainmenuManager>(builder);
        SingleInject<PlayerUIController>(builder);
        SingleInject<GameResultWnd>(builder);
    }


}
