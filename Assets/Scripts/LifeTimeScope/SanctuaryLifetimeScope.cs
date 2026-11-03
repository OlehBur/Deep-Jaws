using VContainer;
using VContainer.Unity;
//using VContainer.Unity;



public class SanctuaryLifetimeScope : LifetimeScopeBase {
    protected override void Configure(IContainerBuilder builder) {
        InitAssets(builder);
        InitServices(builder);
        InitMultipleScripts(builder);
        InitSingleScripts(builder);
    }

    public override void InitAssets(IContainerBuilder builder) {
    }
    public override void InitServices(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<SanctuaryCameraChangerService>().As<ISanctuaryCameraChanger>();
        builder.RegisterComponentInHierarchy<FishAlmanacVisualizeUI>().As<IFishAlmanacVisualizeUI>();
        builder.RegisterComponentInHierarchy<SanctuaryBiomesNavigation>().As<ISanctuaryBiomesNavigation>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
    }
    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<BiomesVisualize>(builder);
        SingleInject<ResidentButtonsView>(builder);
        SingleInject<SanctuaryBiomesNavigation>(builder);
        SingleInject<SanctuaryVisualizeUI>(builder);
        SingleInject<ShellsBarVIsualize>(builder);
        SingleInject<FishAlmanacVisualizeUI>(builder);
    }
}
