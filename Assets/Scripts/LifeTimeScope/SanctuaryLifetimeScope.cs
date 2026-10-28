using VContainer;
using VContainer.Unity;

public class SanctuaryLifetimeScope : LifetimeScopeBase {
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
    }

    public override void InitMultipleScripts(IContainerBuilder builder) {
    }
    public override void InitSingleScripts(IContainerBuilder builder) {
        SingleInject<SanctuaryVisualizeUI>(builder);
        SingleInject<ShellsBarVIsualize>(builder);
    }
}
