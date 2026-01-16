using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using cw.MauiExtensions.Services.Configuration;
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
            bool isLightStatusBar = darkTheme ? MauiExtensionsConfiguration.Instance.UseDarkSystemBarIconsWithModalPagesDark
                                              : MauiExtensionsConfiguration.Instance.UseDarkSystemBarIconsWithModalPages;

            if (MauiExtensionsConfiguration.Instance.EdgeToEdgeForModalPages)
            {
                SystemBarsService.EnableEdgeToEdge(dialogWindow, isLightStatusBar, MauiExtensionsConfiguration.Instance.EdgeToEdgeStartContentBelowBar);
            }
            else
            {
                Android.Graphics.Color platformColor = new Android.Graphics.Color(activity.Window.StatusBarColor);

                // Determine which color resource keys to use based on modal mode and theme
                bool isDarkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;

                Microsoft.Maui.Graphics.Color mauiSystemBarColor;
                mauiSystemBarColor = ResourcesHelper.GetColor(
                    isDarkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor
                                : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                    isDarkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));
                platformColor = mauiSystemBarColor.ToPlatform();

                dialogWindow.ClearFlags(WindowManagerFlags.LayoutNoLimits | WindowManagerFlags.DimBehind);
                dialogWindow.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);

                dialogWindow.SetStatusBarColor(platformColor);
                dialogWindow.SetNavigationBarColor(platformColor);

                var decorView = dialogWindow.DecorView;
                var controller = WindowCompat.GetInsetsController(dialogWindow, decorView);
                if (controller != null)
                {
                    controller.AppearanceLightStatusBars = isLightStatusBar;
                    controller.AppearanceLightNavigationBars = isLightStatusBar;
                }
            }
        }
    }
}
