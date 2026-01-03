using cw.MauiExtensions.Services.Configuration;
#if ANDROID
using cw.MauiExtensions.Services.Platforms.Services;
using Microsoft.Maui.LifecycleEvents;
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
        public static MauiAppBuilder UseMauiExtensionsServices(
            this MauiAppBuilder builder,
            Action<MauiExtensionsConfiguration>? configure = null)
        {
            var config = new MauiExtensionsConfiguration();
            configure?.Invoke(config);
            MauiExtensionsConfiguration.Instance = config;

#if ANDROID
            if (!MauiExtensionsConfiguration.Instance.UseCommunityToolkitMaui)
            {
                // Create the DialogFragmentService of cw.MauiExtensions.Services as a singleton for Android to handle status
                // bar and navigation bar colors with modal pages for any API starting with API 26.
                builder.Services.AddSingleton<IDialogFragmentService, DialogFragmentService>();
            }
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddAndroid(android => android
                    .OnCreate((activity, bundle) =>
                    {
                        if (!MauiExtensionsConfiguration.Instance.UseCommunityToolkitMaui)
                        {
                            // Register FragmentLifecycleCallbacks provided by the above DialogFragmentService to
                            // handle dialog fragments.
                            if (activity is not AndroidX.AppCompat.App.AppCompatActivity componentActivity)
                            {
                                Trace.WriteLine($"Unable to Modify Android StatusBar On ModalPage: Activity {activity.LocalClassName} must be an {nameof(AndroidX.AppCompat.App.AppCompatActivity)}");
                                return;
                            }
                            if (componentActivity.GetFragmentManager() is not AndroidX.Fragment.App.FragmentManager fragmentManager)
                            {
                                Trace.WriteLine($"Unable to Modify Android StatusBar On ModalPage: Unable to retrieve fragment manager from {nameof(AndroidX.AppCompat.App.AppCompatActivity)}");
                                return;
                            }
                            var dialogFragmentService = IPlatformApplication.Current?.Services.GetRequiredService<IDialogFragmentService>()
                                ?? throw new InvalidOperationException($"Unable to retrieve {nameof(IDialogFragmentService)}");
                            fragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleManager(dialogFragmentService), false);
                        }
                    })
                    .OnResume((activity) =>
                    {
                        // Set system bars color when the activity starts and resumes
                        SystemBarsService.SetSystemBarsColor(activity);
                    })); 
            });
#endif

            return builder;
        }
    }
}
