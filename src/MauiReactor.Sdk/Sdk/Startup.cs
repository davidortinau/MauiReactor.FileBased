using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using UIKit;
#if DEBUG && MAUI_REACTOR_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace Microsoft.Maui.FileBased;

internal static class Program
{
    public static void Main(string[] args)
        => UIApplication.Main(args, null, typeof(AppDelegate));
}

[Register("MauiReactorFileAppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiReactorApp<global::App>();
#if DEBUG && MAUI_REACTOR_DEVFLOW
        builder.AddMauiDevFlowAgent();
#endif
        return builder.Build();
    }
}

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
