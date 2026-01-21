using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;


namespace cw.MauiExtensions.Services.Platforms.Services
{
    public partial class DialogFragmentService : IDialogFragmentService
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentAttached(FragmentManager fm, Fragment f, Context context)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentDestroyed(FragmentManager fm, Fragment f)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentDetached(FragmentManager fm, Fragment f)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentPaused(FragmentManager fm, Fragment f)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentPreAttached(FragmentManager fm, Fragment f, Context context)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentPreCreated(FragmentManager fm, Fragment f, Bundle? savedInstanceState)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentResumed(FragmentManager fm, Fragment f)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentSaveInstanceState(FragmentManager fm, Fragment f, Bundle outState)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentStarted(FragmentManager fm, Fragment f)
        {
            if (!TryConvertToDialogFragment(f, out var dialogFragment) || Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is not AppCompatActivity activity)
            {
                return;
            }
            SetSystemBarsColor(dialogFragment, activity);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentStopped(FragmentManager fm, Fragment f)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentViewCreated(FragmentManager fm, Fragment f, Android.Views.View v, Bundle? savedInstanceState)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnFragmentViewDestroyed(FragmentManager fm, Fragment f)
        {
        }

        static bool TryConvertToDialogFragment(Fragment fragment, [NotNullWhen(true)] out DialogFragment? dialogFragment)
        {
            dialogFragment = null;

            if (fragment is not DialogFragment dialog)
            {
                return false;
            }

            dialogFragment = dialog;
            return true;
        }

        static void SetSystemBarsColor(DialogFragment dialogFragment, AppCompatActivity activity)
        {
            if (activity.Window is null)
            {
                return;
            }

            if (dialogFragment.Dialog?.Window is not Android.Views.Window dialogWindow)
            {
                throw new InvalidOperationException("Dialog window cannot be null");
            }

            bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
            //bool isLightStatusBar = darkTheme ? MauiExtensionsConfiguration.Instance.UseDarkSystemBarIconsWithModalPagesDark
            //                                  : MauiExtensionsConfiguration.Instance.UseDarkSystemBarIconsWithModalPages;

            if (OperatingSystem.IsAndroidVersionAtLeast(32) &&
                MauiExtensionsConfiguration.Instance.EdgeToEdgeForModalPages)
            {
                SystemBarsService.EnableEdgeToEdge(dialogWindow,
                                                   SystemBarsService.EdgeEdgeBarsTintMode.LetAndroidDecide,
                                                   MauiExtensionsConfiguration.Instance.EdgeToEdgeStartContentBelowBar);
            }
            else
            {
                // Determine the platform color of the system bars based on modal mode (fullscreen or overlay),
                // theme (dark or light), and configured resource keys.
                // Get the MAUI page associated with this dialog fragment to check its modal mode.
                var mauiPage = GetMauiPageFromFragment(dialogFragment);
                var modalMode = mauiPage != null
                    ? ModalPageProperties.GetMode(mauiPage)
                    : ModalPageMode.FullScreen;

                Android.Graphics.Color platformColor = new Android.Graphics.Color(activity.Window.StatusBarColor);

                // Determine which color resource keys to use based on modal mode and theme
                Microsoft.Maui.Graphics.Color mauiSystemBarColor;

                if (modalMode == ModalPageMode.Overlay)
                {
                    // For overlay modals (popups/dialogs), calculate the modal-specific overlay color
                    var mauiOverlayColor = ResourcesHelper.GetColor(
                        darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayDarkColor
                                  : MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayColor,
                        darkTheme ? Color.FromRgba(0, 0, 0, 0.5) : Color.FromRgba(0, 0, 0, 0.55));
                    var mauiPageColor = ViewPresenter.Instance.GetBackgroundColorOfLastNonModalPage();
                    mauiSystemBarColor = Blend(mauiOverlayColor, mauiPageColor);
                }
                else
                {
                    // For full-screen modals, use the configured color for system bars
                    mauiSystemBarColor = ResourcesHelper.GetColor(
                        darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor
                                  : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                        darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));
                }

                platformColor = mauiSystemBarColor.ToPlatform();

                dialogWindow.ClearFlags(WindowManagerFlags.LayoutNoLimits | WindowManagerFlags.DimBehind |
                                        WindowManagerFlags.TranslucentStatus | WindowManagerFlags.TranslucentNavigation);
                dialogWindow.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);

                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    dialogWindow.NavigationBarContrastEnforced = false;
                    dialogWindow.StatusBarContrastEnforced = false;
                }
                dialogWindow.SetStatusBarColor(platformColor);
                dialogWindow.SetNavigationBarColor(platformColor);

                // Let Android decide on tint color
                //var decorView = dialogWindow.DecorView;
                //var controller = WindowCompat.GetInsetsController(dialogWindow, decorView);
                //if (controller != null)
                //{
                //    controller.AppearanceLightStatusBars = isLightStatusBar;
                //    controller.AppearanceLightNavigationBars = isLightStatusBar;
                //}
            }
        }

        /// <summary>
        /// Attempts to retrieve the MAUI Page associated with a DialogFragment.
        /// </summary>
        static Page? GetMauiPageFromFragment(DialogFragment dialogFragment)
        {
            try
            {
                // Try to get the page from the modal stack
                // When a modal page is shown, it's pushed onto the Navigation.ModalStack
                if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
                {
                    var navigation = Microsoft.Maui.Controls.Application.Current.Windows[0].Page?.Navigation;
                    if (navigation?.ModalStack?.Count > 0)
                    {
                        // Return the topmost modal page
                        return navigation.ModalStack[navigation.ModalStack.Count - 1];
                    }
                }
            }
            catch
            {
                // If we can't retrieve the page, return null and fall back to default behavior
            }

            return null;
        }

        static Color Blend(Color overlay, Color baseColor)
        {
            float a = overlay.Alpha;

            return new Color(
                overlay.Red * a + baseColor.Red * (1 - a),
                overlay.Green * a + baseColor.Green * (1 - a),
                overlay.Blue * a + baseColor.Blue * (1 - a),
                1f);
        }
    }
}
