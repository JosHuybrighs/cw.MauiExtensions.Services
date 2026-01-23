using CommunityToolkit.Maui;
using cw.MauiExtensions.Services.Extensions;
using Microsoft.Extensions.Logging;

namespace cw.MauiExtensions.Services.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                   .UseMauiCommunityToolkit()

                   .UseMauiExtensionsServices(options =>
                    {
                        // Set style resource keys to override the default expected keys in styles.xaml
                        //options.ResourceKeys.AlertDialogBorderStyle = "ContentDialogBorder";
                        //options.ResourceKeys.AlertDialogButtonStyle = "AlertButton";
                        // Set color resource keys to override the default expected keys in colors.xaml
                        //options.ResourceKeys.SystemBarsBackgroundColor = "SystemBarsBackground";
                        //options.ResourceKeys.SystemBarsBackgroundDarkColor = "SystemBarsBackgroundDark";
                        //options.ResourceKeys.NavigationBarBackgroundColor = "NavigationBarBackground";
                        //options.ResourceKeys.ToolbarBackgroundColor = "ToolbarBackground";
                        //options.ResourceKeys.ToolbarTextColor = "ToolbarText";
                        //options.ResourceKeys.ContentDialogBackgroundOverlayColor = "ContentDialogBackgroundOverlay";
                        //options.ResourceKeys.ContentDialogBackgroundOverlayDarkColor = "ContentDialogBackgroundOverlayDark";
                        //options.UseSmartSystemBarColoring = false;
                        //options.UseSmartSystemBarColoringWithModals = false;
                        options.EdgeToEdgeForRootPages = false;
                        //options.EdgeToEdgeForModalPages = false;
                        //options.EdgeToEdgeStartContentBelowBar = false;
                    })

                   .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
