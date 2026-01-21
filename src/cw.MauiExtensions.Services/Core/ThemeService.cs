using cw.MauiExtensions.Services.Configuration;
#if ANDROID
using cw.MauiExtensions.Services.Platforms.Services;
#endif


namespace cw.MauiExtensions.Services.Core
{
    public class ThemeService
    {
        private static volatile ThemeService? sInstance;

        public static ThemeService Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new ThemeService();
                }
                return sInstance;
            }
        }

        public void Run()
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            }

        }
        void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            ViewPresenter.Instance.UpdateNavigationBarColors();
#if ANDROID
            // Update status bars
            if (MauiExtensionsConfiguration.Instance.UseSmartSystemBarColoring)
            {
                var activity = Platform.CurrentActivity;
                if (activity != null)
                {
                    SystemBarsService.SetSystemBarsColor(activity);
                }
            }
#endif
        }

    }
}
