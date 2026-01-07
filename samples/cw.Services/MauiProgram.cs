using CommunityToolkit.Maui;
using cw.MauiExtensions.Services.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace cw.MauiExtensions.Services.Demo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
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
                    //options.UseSystemStatusBarStyling = false;
                    //options.UseSystemNavigationBarStyling = false;
                    //options.UseSmartSystemBarColoringWithModals = false;
                })
                /*
                .ConfigureMauiHandlers(handlers =>
                {
                #if ANDROID
                    handlers.AddHandler(typeof(ContentPage), typeof(PageHandler));

                    PageHandler.Mapper.AppendToMapping("EdgeToEdge", (handler, view) =>
                    {
                        // De native view van een Page op Android is een Microsoft.Maui.Platform.ContentViewGroup
                        var androidView = handler.PlatformView;

                        // Zorg dat deze view niet stopt bij de randen van de systeembalken
                        androidView.SetFitsSystemWindows(false);

                        // De parent van de view moet ook weten dat we edge-to-edge gaan
                        if (androidView.Parent is Android.Views.ViewGroup parent)
                        {
                            parent.SetFitsSystemWindows(false);
                        }
                    });
                #endif
                })
                */
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
