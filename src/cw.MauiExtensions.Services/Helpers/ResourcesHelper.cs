using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Exceptions;
using Microsoft.Maui.Controls.Shapes;

namespace cw.MauiExtensions.Services.Helpers
{
    internal class ResourcesHelper
    {
        private static Style? _alertDialogBorderStyle;
        private static Style? _alertDialogButtonStyle;

        /// <summary>
        /// Gets the AlertDialog border style, using the configured style if available, otherwise the default.
        /// </summary>
        public static Style GetAlertDialogBorderStyle()
        {
            // Return configured style if available
            if (_alertDialogBorderStyle == null)
            {
                if (Application.Current != null &&
                    Application.Current.Resources.TryGetValue(MauiExtensionsConfiguration.Instance.ResourceKeys.AlertDialogBorderStyle, out var val))
                {
                    _alertDialogBorderStyle = (Style)val;
                }
                else
                {
                    // Create default style
                    _alertDialogBorderStyle = CreateDefaultAlertDialogBorderStyle();
                }
            }
            return _alertDialogBorderStyle;
        }

        /// <summary>
        /// Gets the AlertDialog border style, using the configured style if available, otherwise the default.
        /// </summary>
        public static Style GetAlertDialogButtonStyle()
        {
            // Return configured style if available
            if (_alertDialogButtonStyle == null)
            {
                if (Application.Current != null &&
                    Application.Current.Resources.TryGetValue(MauiExtensionsConfiguration.Instance.ResourceKeys.AlertDialogButtonStyle, out var val))
                {
                    _alertDialogButtonStyle = (Style)val;
                }
                else
                {
                    // Create default style
                    _alertDialogButtonStyle = CreateDefaultAlertDialogButtonStyle();
                }
            }
            return _alertDialogButtonStyle;
        }


        /// <summary>
        /// Retrieves a color resource associated with the specified key, or returns a fallback color if the resource is
        /// not found.
        /// </summary>
        /// <remarks>This method searches the application's current resource dictionary for a color
        /// resource matching the specified key. If the key does not exist or is not associated with a color, the
        /// provided fallback color is returned.</remarks>
        /// <param name="key">The key of the color resource to retrieve. Cannot be null.</param>
        /// <param name="fallback">The color to return if the resource with the specified key is not found.</param>
        /// <returns>The color resource associated with the specified key, or the fallback color if the resource is not found.</returns>
        public static Color GetColor(string key, Color fallback)
        {
            if (Application.Current != null &&
                Application.Current.Resources.TryGetValue(key, out var val))
            {
                return (Color)val;
            }
            return fallback;
        }



        /// <summary>
        /// Derives an overlay color from a background color by making it semi-transparent.
        /// For very light backgrounds, darkens them; for very dark backgrounds, keeps them dark.
        /// </summary>
        private static Color DeriveOverlayColor(Color backgroundColor)
        {
            // Calculate luminance to determine if background is light or dark
            float luminance = (0.299f * backgroundColor.Red +
                             0.587f * backgroundColor.Green +
                             0.114f * backgroundColor.Blue);

            // For light backgrounds (luminance > 0.5), create a darker overlay
            // For dark backgrounds, create a semi-transparent version
            if (luminance > 0.5f)
            {
                // Light background - use darker overlay
                return Color.FromRgba(
                    backgroundColor.Red * 0.3f,
                    backgroundColor.Green * 0.3f,
                    backgroundColor.Blue * 0.3f,
                    0.6f  // 60% opacity
                );
            }
            else
            {
                // Dark background - use semi-transparent version
                return Color.FromRgba(
                    backgroundColor.Red,
                    backgroundColor.Green,
                    backgroundColor.Blue,
                    0.5f  // 50% opacity
                );
            }
        }

        private static Style CreateDefaultAlertDialogBorderStyle()
        {
            var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;

            var style = new Style(typeof(Border))
            {
                Setters =
                {
                    new Setter { Property = Border.VerticalOptionsProperty, Value = LayoutOptions.Center },
                    new Setter { Property = Border.PaddingProperty, Value = new Thickness(10, 24, 10, 24) },
                    new Setter { Property = Border.StrokeShapeProperty, Value = new RoundRectangle { CornerRadius = 15 } },
                    new Setter { Property = Border.StrokeThicknessProperty, Value = 1 },
                    new Setter { Property = Border.StrokeProperty, Value = isDarkMode ? Color.FromArgb("#404040") : Color.FromArgb("#E0E0E0") },
                    new Setter { Property = Border.BackgroundColorProperty, Value = isDarkMode ? Color.FromArgb("#1E1E1E") : Colors.White }
                }
            };

            return style;
        }

        private static Style CreateDefaultAlertDialogButtonStyle()
        {
            var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;

            var style = new Style(typeof(Button))
            {
                Setters =
                {
                    new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Transparent },
                    new Setter { Property = Button.TextColorProperty, Value = isDarkMode ? Color.FromArgb("#AC99EA") : Color.FromArgb("#512BD4") },
                    new Setter { Property = Button.BorderWidthProperty, Value = 0 },
                    new Setter { Property = Button.PaddingProperty, Value = new Thickness(8, 4) }
                }
            };

            return style;
        }



        public static Color GetColorResource(string lightKey, string darkKey)
        {
            if (Application.Current != null)
            {
                string key = Application.Current.RequestedTheme == AppTheme.Dark ? darkKey : lightKey;

                if (Application.Current.Resources.TryGetValue(key, out var color) && color is Color c)
                {
                    return c;
                }
                throw new MissingResourceException(key);
            }
            throw new InvalidOperationException("Application.Current is null.");
        }
    }
}
