using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Helpers;
using Microsoft.UI;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace cw.MauiExtensions.Services.Platforms.Windows
{
    public class WindowsTitleBarService
    {
        private static Microsoft.UI.Xaml.Window? _currentWindow;

        /// <summary>
        /// Registers the WinUI window for later use. Call this during app initialization.
        /// </summary>
        public static void RegisterWindow(Microsoft.UI.Xaml.Window window)
        {
            _currentWindow = window;
        }

        public static void ConfigureTitleBar()
        {
            if (_currentWindow != null)
            {
                ConfigureTitleBar(_currentWindow);
            }
        }

        public static void ConfigureTitleBar(Microsoft.UI.Xaml.Window window)
        {
            // Get the AppWindow for the current window
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow == null)
                return;

            // Determine if we're in dark mode
            bool isDarkMode = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark;

            // Get colors from resources
            var backgroundColor = ResourcesHelper.GetColor(
                isDarkMode ? MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundDarkColor 
                           : MauiExtensionsConfiguration.Instance.ResourceKeys.SystemBarsBackgroundColor,
                isDarkMode ? MauiColor.FromRgb(0, 0, 0) : MauiColor.FromRgb(255, 255, 255));

            var foregroundColor = ResourcesHelper.GetColor(
                isDarkMode ? MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarTextDarkColor 
                           : MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarTextColor,
                isDarkMode ? MauiColor.FromRgb(255, 255, 255) : MauiColor.FromRgb(16, 16, 16));

            // Configure title bar
            if (appWindow.TitleBar != null)
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                
                // Convert MAUI colors to Windows colors
                var bgColor = ToWindowsColor(backgroundColor);
                var fgColor = ToWindowsColor(foregroundColor);

                appWindow.TitleBar.ButtonBackgroundColor = bgColor;
                appWindow.TitleBar.ButtonForegroundColor = fgColor;
                appWindow.TitleBar.BackgroundColor = bgColor;
                appWindow.TitleBar.ForegroundColor = fgColor;
                
                // Inactive colors (slightly dimmed)
                appWindow.TitleBar.InactiveBackgroundColor = bgColor;
                appWindow.TitleBar.InactiveForegroundColor = ToWindowsColor(
                    isDarkMode ? MauiColor.FromRgb(160, 160, 160) : MauiColor.FromRgb(96, 96, 96));
                
                // Hover colors
                appWindow.TitleBar.ButtonHoverBackgroundColor = ToWindowsColor(
                    isDarkMode ? MauiColor.FromRgb(32, 32, 32) : MauiColor.FromRgb(240, 240, 240));
                appWindow.TitleBar.ButtonHoverForegroundColor = fgColor;
                
                // Pressed colors
                appWindow.TitleBar.ButtonPressedBackgroundColor = ToWindowsColor(
                    isDarkMode ? MauiColor.FromRgb(48, 48, 48) : MauiColor.FromRgb(230, 230, 230));
                appWindow.TitleBar.ButtonPressedForegroundColor = fgColor;
            }

            // Fix MAUI NavigationPage bar on Windows by styling the native control.
            // Because ExtendsContentIntoTitleBar is set to true, the navigation bar is a part
            // of the windows title bar area
            ApplyNavigationBarColors(backgroundColor, foregroundColor);
        }

        private static void ApplyNavigationBarColors(MauiColor backgroundColor, MauiColor foregroundColor)
        {
            // The NavigationView might not be created immediately, so we'll try with a delay
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
            {
                // Try a few times with delays to catch the NavigationView when it's created
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    if (_currentWindow?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
                    {
                        var navigationView = FindNavigationView(rootElement);
                        if (navigationView != null)
                        {
                            var bgBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(ToWindowsColor(backgroundColor));
                            var fgBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(ToWindowsColor(foregroundColor));
                            
                            navigationView.Background = bgBrush;
                            navigationView.Foreground = fgBrush;
                            return; // Success, exit
                        }
                    }
                    
                    // Wait before next attempt
                    await Task.Delay(100);
                }
            });
        }

        private static Microsoft.UI.Xaml.Controls.NavigationView? FindNavigationView(Microsoft.UI.Xaml.DependencyObject parent)
        {
            if (parent is Microsoft.UI.Xaml.Controls.NavigationView navView)
            {
                return navView;
            }

            int childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                var result = FindNavigationView(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static global::Windows.UI.Color ToWindowsColor(MauiColor color)
        {
            return global::Windows.UI.Color.FromArgb(
                (byte)(color.Alpha * 255),
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255));
        }
    }
}
