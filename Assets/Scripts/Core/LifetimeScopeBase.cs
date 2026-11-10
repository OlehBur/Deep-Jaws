using UnityEngine;
using VContainer;
using VContainer.Unity;


public abstract class LifetimeScopeBase : LifetimeScope {
    public static void MultipleInject<T>(IContainerBuilder builder) where T : Object {
        T[] allScripts = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        builder.RegisterBuildCallback(container => {
            foreach (T script in allScripts)
                container.Inject(script);
        });
    }

    public static void SingleInject<T>(IContainerBuilder builder) where T : Object {
        T script = FindAnyObjectByType<T>(FindObjectsInactive.Include);

        if (script != null)
            builder.RegisterBuildCallback(container => container.Inject(script));
        //builder.RegisterComponent(script);
    }

    public abstract void InitAssets(IContainerBuilder builder);
    public abstract void InitServices(IContainerBuilder builder);
    public abstract void InitMultipleScripts(IContainerBuilder builder);
    public abstract void InitSingleScripts(IContainerBuilder builder);
}