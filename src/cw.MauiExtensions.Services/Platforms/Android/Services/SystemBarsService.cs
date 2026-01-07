using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace cw.MauiExtensions.Services.Platforms.Services
{
    public static class AndroidWindowExtensions
    {
        /// <summary>
        /// Gets the current window associated with the specified activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The current window.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the activity window is null.</exception>
        public static Android.Views.Window GetCurrentWindow(this Activity activity)
        {
            var window = activity.Window ?? throw new InvalidOperationException($"{nameof(activity.Window)} cannot be null");
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            return window;
        }
    }

    /*
    public static class MauiActivityExtensions
    {
        public static Android.Widget.Toolbar? GetToolbar(this Microsoft.Maui.Controls.Page page)
        {
            var handler = page.Handler as Microsoft.Maui.Handlers.PageHandler;
            if (handler == null)
                return null;

            var platformView = handler.PlatformView;
            return platformView?.FindViewById<Android.Widget.Toolbar>(platformView.Context.Resources.GetIdentifier("toolbar", "id", platformView.Context.PackageName));
        }
    }
    */

    public class SystemBarsService
    {
        //static Activity Activity => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? throw new InvalidOperationException("Android Activity can't be null.");


        public static void SetSystemBarsColor(Activity activity)
        {
            if (!MauiExtensionsConfiguration.Instance.UseSystemStatusBarStyling &&
                !MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
            {
                return;
            }

            var window = activity.Window ?? throw new InvalidOperationException($"{nameof(activity.Window)} cannot be null");

            bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
            var mauiSystemBarColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor
                                                                        : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                                                              darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));

            bool isLightStatusBar = ShouldUseDarkIcons(mauiSystemBarColor);

            if (MauiExtensionsConfiguration.Instance.DrawUnderSystemBars &&
                !MauiExtensionsConfiguration.Instance.AppHasNavigationBar)
            {
                EnableEdgeToEdge(window, isLightStatusBar);
                return;
            }

            // This sets the system bars (status bar and navigation bar) background colors at startup of the app and
            // when the app resumes from background, based on the current theme and configured resource colors.
            // NOTE: To also have the correct colors with modal pages and popup dialogs, DialogFragmentService is
            //       also instantiated.

            var systemBarColor = mauiSystemBarColor.ToPlatform();

            if (OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                // The following is taken over from StatusBar.android.cs in CommunityToolkit.Maui.
                // The navigation bar color musn't (actually can't) be changed on API 35+ since the
                // page background color overlaps with the system navigation bar area.

                const string statusBarOverlayTag = "StatusBarOverlay";

                var decorGroup = (ViewGroup)window.DecorView;
                var statusBarOverlay = decorGroup.FindViewWithTag(statusBarOverlayTag);

                if (statusBarOverlay is null)
                {
                var statusBarHeight = activity.Resources?.GetIdentifier("status_bar_height", "dimen", "android") ?? 0;
                var statusBarPixelSize = statusBarHeight > 0 ? activity.Resources?.GetDimensionPixelSize(statusBarHeight) ?? 0 : 0;

                statusBarOverlay = new(activity)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, statusBarPixelSize + 3)
                        {
                            Gravity = GravityFlags.Top
                        }
                    };

                    decorGroup.AddView(statusBarOverlay);
                    statusBarOverlay.SetZ(0);
                }

                // Set StatusBar color
                statusBarOverlay.SetBackgroundColor(systemBarColor);
            }
            else
            {
                // Set StatusBar color
                activity.Window.SetStatusBarColor(systemBarColor);

                // Set NavigationBar color
                if (MauiExtensionsConfiguration.Instance.UseSystemNavigationBarStyling)
                {
                    if (!OperatingSystem.IsAndroidVersionAtLeast(35))
                    {
                        activity.Window.SetNavigationBarColor(systemBarColor);
                    }
                }
            }


            bool isColorTransparent = systemBarColor == Android.Graphics.Color.Transparent;
            if (isColorTransparent)
            {
                activity.Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                activity.Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
            }
            else
            {
                activity.Window.ClearFlags(WindowManagerFlags.LayoutNoLimits);
                activity.Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);
            }
            WindowCompat.SetDecorFitsSystemWindows(window, !isColorTransparent);

            // Set light or dark status bar icons based on background color brightness
            var decorView = window.DecorView;
            var controller = WindowCompat.GetInsetsController(window, decorView);
            if (controller != null)
            {
                controller.AppearanceLightStatusBars = isLightStatusBar;
            }
        }

        public static bool ShouldUseDarkIcons(Color c)
        {
            double lum = 0.299 * c.Red + 0.587 * c.Green + 0.114 * c.Blue;
            return lum > 0.5;
        }


        /*
        private static void ApplyToolbarPadding(ViewGroup viewGroup, int statusBarHeight)
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);

                // Handle both toolbar types
                if (child is Android.Widget.Toolbar androidToolbar)
                {
                    androidToolbar.SetPadding(
                        androidToolbar.PaddingLeft,
                        statusBarHeight,
                        androidToolbar.PaddingRight,
                        androidToolbar.PaddingBottom);
                    return;
                }

                if (child is AndroidX.AppCompat.Widget.Toolbar appCompatToolbar)
                {
                    appCompatToolbar.SetPadding(
                        appCompatToolbar.PaddingLeft,
                        statusBarHeight,
                        appCompatToolbar.PaddingRight,
                        appCompatToolbar.PaddingBottom);
                    return;
                }

                // Recursively search child view groups
                if (child is ViewGroup childGroup)
                {
                    ApplyToolbarPadding(childGroup, statusBarHeight);
                }
            }
        }
        */

        public class EdgeToEdgeInsetsListener : Java.Lang.Object, Android.Views.View.IOnApplyWindowInsetsListener
        {
            int _topOffset;

            public EdgeToEdgeInsetsListener(int topOffset)
            {
                _topOffset = topOffset;
            }

            public WindowInsets OnApplyWindowInsets(Android.Views.View v, WindowInsets insets)
            {
                //int statusBarHeight = insets.SystemWindowInsetTop;
                //statusBarHeight = 0;

                /*
                // Find and adjust the toolbar instead of padding the entire view
                if (v is ViewGroup viewGroup)
                {
                    ApplyToolbarPadding(viewGroup, 56);
                }

                // Don't pad the entire view - let content draw edge-to-edge
                //return insets;
                //// Hoogte van de status bar
                ////statusBarHeight = 18;
                */
                // Padding toepassen zodat content niet onder status bar valt
                v.SetPadding(v.PaddingLeft,
                             _topOffset,
                             v.PaddingRight,
                             v.PaddingBottom);

                // Consume insets zodat MAUI niet onder de status bar stopt
                return insets.ConsumeSystemWindowInsets();
                //return insets;
            }
        }


        public static void EnableEdgeToEdge(Activity activity, bool isLightStatusBar)
        {
            if (activity?.Window == null)
                return;

            var window = activity.Window;

            EnableEdgeToEdge(window, isLightStatusBar);
        }


        public static void EnableEdgeToEdge(Android.Views.Window window, bool isLightStatusBar)
        {
            // Edge-to-edge
            WindowCompat.SetDecorFitsSystemWindows(window, false);

            //window.SetStatusBarColor(Android.Graphics.Color.Transparent);

            window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.AddFlags(WindowManagerFlags.LayoutNoLimits);

            var decorView = window.DecorView;
            // Set the OnApplyWindowInsetsListener to position the view at the top of the screen,
            // i.e. under the status bar.
            decorView.SetOnApplyWindowInsetsListener(new EdgeToEdgeInsetsListener(topOffset: 0));

            var controller = WindowCompat.GetInsetsController(window, decorView);
            if (controller != null)
            {
                controller.AppearanceLightStatusBars = isLightStatusBar;
            }
        }

    }
}
