using VContainer;
using VContainer.Unity;



public class MenuLifetimeScope : LifetimeScopeBase {

    protected override void Configure(IContainerBuilder builder) {
        InitAssets(builder);
        InitServices(builder);
        InitMultipleScripts(builder);
        InitSingleScripts(builder);
    }

    public override void InitAssets(IContainerBuilder builder) {
    }
    public override void InitServices(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<IMainMenuNavigation>();
        builder.RegisterComponentInHierarchy<IGameModeController>();
        builder.RegisterComponentInHierarchy<ISkinsPreviewController>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
        //MultipleInject<StoreCardView>(builder);
    }

    public override void InitSingleScripts(IContainerBuilder builder) {
        //SingleInject<MainMenuNavigation>(builder);
        //SingleInject<GameHudController>(builder);
        SingleInject<GPGServicesWnd>(builder);
        SingleInject<StoreWnd>(builder);
        SingleInject<SkinsPreviewWnd>(builder);
        SingleInject<PurchaseAccountVisualizer>(builder);
        SingleInject<ShellsBarView>(builder);
    }
}
