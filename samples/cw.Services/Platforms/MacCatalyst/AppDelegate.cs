using Foundation;
using cw.MauiExtensions.Services.Demo;

namespace MauiExtensions.Demo
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
