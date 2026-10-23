using VContainer;
using VContainer.Unity;
using UnityEngine;


public class MainLifetimeScope : LifetimeScope
{
    [SerializeField] FishPrefabsInfo fishPrefabs;


    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(fishPrefabs);

        builder.Register<IPlayerPoolService, PlayerPoolService>(Lifetime.Singleton);
        builder.Register<IEnemyPoolService, EnemyPoolService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<MainmenuManager>();
        builder.RegisterComponentInHierarchy<AudioManager>();
        builder.RegisterComponentInHierarchy<VFXpoolManager>();
    }
}
