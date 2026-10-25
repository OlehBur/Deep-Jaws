using VContainer;
using VContainer.Unity;
using UnityEngine;


public class MainLifetimeScope : LifetimeScope {
    [SerializeField] FishPrefabsInfo fishPrefabs;
    [SerializeField] VFX_PrefabsInfo vfxPrefabsInfo;
    

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterInstance(fishPrefabs);
        builder.RegisterInstance(vfxPrefabsInfo);

        builder.Register<IVFXPoolService, VFXPoolService>(Lifetime.Singleton);
        builder.Register<IPlayerFishPoolService, PlayerPoolService>(Lifetime.Singleton);
        builder.Register<IEnemyFishPoolService, EnemyPoolService>(Lifetime.Singleton);

        //builder.RegisterComponentInHierarchy<IMainmenuManager/*MainmenuManager*/>();
        builder.RegisterComponentInHierarchy<IAudioManager/*AudioManager*/>();
        builder.RegisterComponentInHierarchy<ISpawnOrchestrator/*SpawnOrchestrator*/>();
        //builder.RegisterComponentInHierarchy<VFXpoolService>();
    }
}
