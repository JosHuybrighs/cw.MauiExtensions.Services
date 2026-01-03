using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Helpers;
using Microsoft.Maui.Platform;

namespace cw.MauiExtensions.Services.Platforms.Services
{
    public class SystemBarsService
    {
        public static void SetSystemBarsColor(Activity activity)
        {
            if (!MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling &&
                !MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
            {
                return;
            }

            // This sets the system bars (status bar and navigation bar) background colors at startup of the app and
            // when the app resumes from background, based on the current theme and configured resource colors.
            // NOTE: To also have the correct colors with modal pages and popup dialogs, a Dialog Fragment Service is
            //       also needed. See .
            if (Microsoft.Maui.Controls.Application.Current?.Resources != null)
            {
                var window = activity.Window;
                bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
                var mauiSystemBarColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor
                                                                            : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                                                                  darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));

                var systemBarColor = mauiSystemBarColor.ToPlatform();

                // 3 things to do:

                // 1. Set the StatusBar background color. If this is no done then android will set the status bar to
                //    colorPrimaryDark for API < 35 and colorPrimary for API 35+.

                if (MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling)
                {
                    // Set StatusBar color
                    if (OperatingSystem.IsAndroidVersionAtLeast(35))
                    {
                        // API 35+ does not allow changing status bar color directly, so we add an overlay view
                        var decorGroup = (ViewGroup)window.DecorView;
                        const string statusBarOverlayTag = "StatusBarOverlay";
                        var statusBarOverlay = decorGroup.FindViewWithTag(statusBarOverlayTag);

                        if (statusBarOverlay is null)
                        {
                            var statusBarHeight = activity.Resources?.GetIdentifier("status_bar_height", "dimen", "android") ?? 0;
                            var statusBarPixelSize = statusBarHeight > 0 ? activity.Resources?.GetDimensionPixelSize(statusBarHeight) ?? 0 : 0;

                            statusBarOverlay = new Android.Views.View(activity)
                            {
                                Tag = statusBarOverlayTag,
                                LayoutParameters = new FrameLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, statusBarPixelSize + 3)
                                {
                                    Gravity = GravityFlags.Top
                                }
                            };

                            decorGroup.AddView(statusBarOverlay);
                            statusBarOverlay.BringToFront();
                        }
                        statusBarOverlay.SetBackgroundColor(systemBarColor);
                    }
                    else
                    {
                        // API < 35 can set status bar color directly
                        window.SetStatusBarColor(systemBarColor);
                    }
                }

                // 2. Set NavigationBar color
                if (MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
                {
                    if (!OperatingSystem.IsAndroidVersionAtLeast(35))
                    {
                        window.SetNavigationBarColor(systemBarColor);
                    }
                }

                // 3. Set icon colors based on background brightness. If we don't do this the icons will
                //    always be white, which may not be visible on light backgrounds.
                SetSystemBarsIconsAppearance(activity.Window);
            }
        }

        public static void SetSystemBarsIconsAppearance(Android.Views.Window window)
        {
            bool useDarkIcons = Microsoft.Maui.Controls.Application.Current.RequestedTheme != AppTheme.Dark;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                // Android 11+ (API 30+)
                var insetsController = WindowCompat.GetInsetsController(window, window.DecorView);
                if (insetsController != null)
                {
                    // Determine if we should use light navigation bar icons
                    if (MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling)
                        insetsController.AppearanceLightStatusBars = useDarkIcons;
                    if (MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
                        insetsController.AppearanceLightNavigationBars = useDarkIcons;
                }
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Android 8.0+ (API 26+)
                var decorView = window.DecorView;
                var uiFlags = decorView.SystemUiFlags;
                if (useDarkIcons)
                {
                    // Light background, use dark icons
                    if (MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling)
                        uiFlags |= SystemUiFlags.LightStatusBar;
                    if (MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
                        uiFlags |= SystemUiFlags.LightNavigationBar;
                }
                else
                {
                    // Dark background, use light icons
                    if (MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling)
                        uiFlags &= ~SystemUiFlags.LightStatusBar;
                    if (MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
                        uiFlags &= ~SystemUiFlags.LightNavigationBar;
                }
                decorView.SystemUiFlags = uiFlags;
            }

        }

    }
}
