using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Core;
using Microsoft.Maui.LifecycleEvents;
#if IOS
using MapKit;
#endif
#if WINDOWS
using Microsoft.UI;
#endif
#if ANDROID
using cw.MauiExtensions.Services.Platforms.Services;
using Microsoft.Maui.Platform;
using System.Diagnostics;
#endif

namespace cw.MauiExtensions.Services.Extensions
{
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Configures the cw.MauiExtensions.Services library with custom styles and settings.
        /// </summary>
        /// <param name="builder">The MauiAppBuilder instance.</param>
        /// <param name="configure">Action to configure library options.</param>
        /// <returns>The MauiAppBuilder for chaining.</returns>
        /// <example>
        /// <code>
        /// builder.UseMauiExtensionsServices(options =>
        /// {
        ///     options.PopupBorderStyle = (Style)Application.Current.Resources["CustomPopupBorder"];
        ///     options.TextOnlyButtonStyle = (Style)Application.Current.Resources["CustomTextButton"];
        /// });
        /// </code>
        /// </example>
        public static MauiAppBuilder UseMauiExtensionsServices(this MauiAppBuilder builder,
                                                               Action<MauiExtensionsConfiguration>? configure = null)
        {
            var config = new MauiExtensionsConfiguration();
            configure?.Invoke(config);
            MauiExtensionsConfiguration.Instance = config;

            // Android-only services and lifecycle events
#if ANDROID
            if (MauiExtensionsConfiguration.Instance.UseSmartSystemBarColoringWithModals)
            {
                // Create the DialogFragmentService of cw.MauiExtensions.Services as a singleton for Android to handle status
                // bar and navigation bar colors with modal pages for any API starting with API 26.
                builder.Services.AddSingleton<IDialogFragmentService, DialogFragmentService>();
            }
#endif
            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android
                    .OnCreate((activity, bundle) =>
                    {
                        if (MauiExtensionsConfiguration.Instance.UseSmartSystemBarColoringWithModals)
                        {
                            // Register FragmentLifecycleCallbacks provided by the above DialogFragmentService to
                            // handle dialog fragments.
                            if (activity is not AndroidX.AppCompat.App.AppCompatActivity componentActivity)
                            {
                                Trace.WriteLine($"Unable to modify Android StatusBar On ModalPage: Activity {activity.LocalClassName} must be an {nameof(AndroidX.AppCompat.App.AppCompatActivity)}");
                                return;
                            }
                            if (componentActivity.GetFragmentManager() is not AndroidX.Fragment.App.FragmentManager fragmentManager)
                            {
                                Trace.WriteLine($"Unable to modify Android StatusBar On ModalPage: Unable to retrieve fragment manager from {nameof(AndroidX.AppCompat.App.AppCompatActivity)}");
                                return;
                            }
                            var dialogFragmentService = IPlatformApplication.Current?.Services.GetRequiredService<IDialogFragmentService>()
                                ?? throw new InvalidOperationException($"Unable to retrieve {nameof(IDialogFragmentService)}");
                            fragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleManager(dialogFragmentService), false);
                        }
                    })
                    .OnResume((activity) =>
                    {
                        // Register theme-change service
                        ThemeService.Instance.Run();
                        if (MauiExtensionsConfiguration.Instance.UseSmartSystemBarColoring)
                        {
                            // Set system bars color when the activity starts and resumes
                            SystemBarsService.SetSystemBarsColor(activity);
                        }
                    }));
#endif
#if IOS
                events.AddiOS(ios =>
                {
                    ios.FinishedLaunching((app, options) =>
                    {
                        ThemeService.Instance.Run();
                        return true;
                    });
                });
#endif
#if WINDOWS
                events.AddWindows(windows =>
                {
                    windows.OnLaunched((app, args) =>
                    {
                        ThemeService.Instance.Run();
                    });
                    windows.OnWindowCreated(window =>
                    {
                        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
                        if (mauiWindow != null)
                        {
                            // Get the appWindow for the current window
                            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                            var windowId = Win32Interop.GetWindowIdFromWindow(handle);
                            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                            if (appWindow != null)
                            {
                            }
                        }
                    });
                });
#endif
            });
            return builder;
        }
    }
}
