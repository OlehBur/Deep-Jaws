using VContainer;
using VContainer.Unity;



public class MenuLifetimeScope : LifetimeScopeBase {


    protected override void Configure(IContainerBuilder builder) {
        DontDestroyOnLoad(gameObject);

        InitAssets(builder);
        InitServices(builder);
        InitMultipleScripts(builder);
        InitSingleScripts(builder);


    }

    public override void InitAssets(IContainerBuilder builder) {
    }
    public override void InitServices(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<IGameHudController>();

        builder.RegisterComponentInHierarchy<MainMenuNavigation>().As<IMainMenuNavigation>();
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
    }
    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<GameHudController>(builder);
        SingleInject<PurchaseUIvisualizer>(builder);
        SingleInject<ShellsBarVIsualize>(builder);
    }
}
