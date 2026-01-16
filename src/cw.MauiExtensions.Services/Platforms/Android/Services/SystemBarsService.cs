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
            var window = activity.Window ?? throw new InvalidOperationException($"{nameof(activity.Window)} cannot be null");

            bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
            var mauiSystemBarColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor
                                                                        : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                                                              darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));

            bool isLightStatusBar = ShouldUseDarkIcons(mauiSystemBarColor);

            var systemBarColor = mauiSystemBarColor.ToPlatform();
            //window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(systemBarColor));

            if (MauiExtensionsConfiguration.Instance.EdgeToEdgeForRootPages)
            {
                // Set system bars by extending the page content into the system bars area (edge-to-edge)
                EnableEdgeToEdge(window, isLightStatusBar, MauiExtensionsConfiguration.Instance.EdgeToEdgeStartContentBelowBar);
                return;
            }

            // Page content is not extended into system bars area, and so we must set the system bars colors 
            // according to the configured SystemBarsBackgroundColor or SystemBarsBackgroundDarkColor resource color.
            if (OperatingSystem.IsAndroidVersionAtLeast(35))
            {
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

                const string navBarOverlayTag = "NavBarOverlay";

                var navBarOverlay = decorGroup.FindViewWithTag(navBarOverlayTag);

                if (navBarOverlay == null)
                {
                    var navBarHeightId = activity.Resources?.GetIdentifier(
                        "navigation_bar_height", "dimen", "android") ?? 0;

                    var navBarHeight = navBarHeightId > 0
                        ? activity.Resources?.GetDimensionPixelSize(navBarHeightId) ?? 0
                        : 0;

                    navBarOverlay = new Android.Views.View(activity)
                    {
                        LayoutParameters = new FrameLayout.LayoutParams(
                            ViewGroup.LayoutParams.MatchParent,
                            navBarHeight)
                        {
                            Gravity = GravityFlags.Bottom
                        }
                    };

                    //navBarOverlay.SetTag(navBarOverlayTag);
                    decorGroup.AddView(navBarOverlay);
                    navBarOverlay.SetZ(0);
                }

                navBarOverlay.SetBackgroundColor(systemBarColor);

                //ApplyBottomNavBackground(decorGroup, systemBarColor);
                window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(systemBarColor));
            }
            else
            {
                // Set StatusBar color
                activity.Window.SetStatusBarColor(systemBarColor);
                // Set NavigationBar color
                activity.Window.SetNavigationBarColor(systemBarColor);
            }

            //bool isColorTransparent = systemBarColor == Android.Graphics.Color.Transparent;
            //if (isColorTransparent)
            //{
            //    activity.Window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            //    activity.Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
            //}
            //else
            //{
            //    activity.Window.ClearFlags(WindowManagerFlags.LayoutNoLimits);
            //    activity.Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);
            //}
            //WindowCompat.SetDecorFitsSystemWindows(window, !isColorTransparent);

            activity.Window.ClearFlags(WindowManagerFlags.LayoutNoLimits);
            activity.Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);
            WindowCompat.SetDecorFitsSystemWindows(window, true);

            // Set light or dark status bar icons based on background color brightness
            var decorView = window.DecorView;
            var controller = WindowCompat.GetInsetsController(window, decorView);
            if (controller != null)
            {
                controller.AppearanceLightStatusBars = isLightStatusBar;
                controller.AppearanceLightNavigationBars = isLightStatusBar;
            }
        }

        public static bool ShouldUseDarkIcons(Color c)
        {
            double lum = 0.299 * c.Red + 0.587 * c.Green + 0.114 * c.Blue;
            return lum > 0.5;
        }

        static void ApplyBottomNavBackground(ViewGroup parent, Android.Graphics.Color color)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);

                if (child is Google.Android.Material.BottomNavigation.BottomNavigationView bnv)
                {
                    bnv.SetBackgroundColor(color);
                    return;
                }

                if (child is ViewGroup vg)
                {
                    ApplyBottomNavBackground(vg, color);
                }
            }
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

        public class ResetEdgeToEdgeInsetsListener : Java.Lang.Object, Android.Views.View.IOnApplyWindowInsetsListener
        {
            public WindowInsets OnApplyWindowInsets(Android.Views.View v, WindowInsets insets)
            {
                int offset = 0;
                /*
                // Use WindowInsetsCompat for backward compatibility
                var compatInsets = WindowInsetsCompat.ToWindowInsetsCompat(insets);
                // Get the height of the status bars (includes the notch/display cutout)
                offset = compatInsets.GetInsets(WindowInsetsCompat.Type.StatusBars()).Top;
                */
                v.SetPadding(v.PaddingLeft,
                             offset,
                             v.PaddingRight,
                             v.PaddingBottom);
                return insets.ConsumeSystemWindowInsets();
            }
        }


        public static void EnableEdgeToEdge(Activity activity, bool isLightStatusBar)
        {
            if (activity?.Window == null)
                return;

            var window = activity.Window;

            EnableEdgeToEdge(window, isLightStatusBar, MauiExtensionsConfiguration.Instance.EdgeToEdgeStartContentBelowBar);
        }


        public static void EnableEdgeToEdge(Android.Views.Window window, bool isLightStatusBar, bool addStatusBarOffset)
        {
            // Edge-to-edge
            WindowCompat.SetDecorFitsSystemWindows(window, false);

            window.ClearFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.AddFlags(WindowManagerFlags.LayoutNoLimits);
            //window.SetStatusBarColor(Android.Graphics.Color.Transparent);
            //window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
            //window.ClearFlags(WindowManagerFlags.LayoutNoLimits);

            var decorView = window.DecorView;
            // If the next is not called then the page content starts under the status bar.
            if (!addStatusBarOffset)
            {
                decorView.SetOnApplyWindowInsetsListener(new ResetEdgeToEdgeInsetsListener());
            }

            var controller = WindowCompat.GetInsetsController(window, decorView);
            if (controller != null)
            {
                controller.AppearanceLightStatusBars = isLightStatusBar;
                controller.AppearanceLightNavigationBars = isLightStatusBar;
            }
        }

    }
}
