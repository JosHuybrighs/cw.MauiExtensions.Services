namespace cw.MauiExtensions.Services.Configuration
{
    public class MauiExtensionsResourceKeys
    {
        /// <summary>
        /// Gets or sets the border style applied to alert dialogs.
        /// </summary>
        /// <remarks>The style determines the visual appearance of the Border of alert dialogs. The default
        /// expected style key is "ContentDialogBorder".</remarks>
        public string AlertDialogBorderStyle { get; set; } = "ContentDialogBorder";

        /// <summary>
        /// Gets or sets the style applied to alert dialog buttons.
        /// </summary>
        /// <remarks>The style determines the visual appearance of buttons within alert dialogs. Default
        /// expected style key is "TextOnlyButton".</remarks>
        public string AlertDialogButtonStyle { get; set; } = "TextOnlyButton";

        /// <summary>
        /// Gets or sets the resource key for the background overlay color used in content dialogs in Light mode.
        /// </summary>
        /// <remarks>Set this property to specify a custom overlay color by providing the corresponding
        /// resource key. The overlay color affects the appearance of the dialog's background when displayed.
        /// The default expected key is "ContentDialogBackgroundOverlay".</remarks>
        public string ContentDialogBackgroundOverlayColor { get; set; } = "ContentDialogBackgroundOverlay";

        /// <summary>
        /// Gets or sets the resource key for the background overlay color used in dark mode for content dialogs.
        /// </summary>
        /// <remarks>Set this property to specify a custom overlay color for content dialogs when the
        /// application is in dark mode. The value should correspond to a valid resource key defined in the
        /// application's resource dictionary.
        /// The default expected key is "ContentDialogBackgroundOverlayDark"</remarks>
        public string ContentDialogBackgroundOverlayDarkColor { get; set; } = "ContentDialogBackgroundOverlayDark";

        /// <summary>
        /// Gets or sets the background color in light mode used for system bars, such as the status bar and navigation bar.
        /// </summary>
        /// <remarks>The default expected key is "SystemBarsBackground".</remarks>
        public string SystemBarsBackgroundColor { get; set; } = "SystemBarsBackground";

        /// <summary>
        /// Gets or sets the background color in dark mode used for system bars, such as the status bar and navigation bar.
        /// </summary>
        /// <remarks>The default expected key is "SystemBarsBackgroundDark".</remarks>
        public string SystemBarsBackgroundDarkColor { get; set; } = "SystemBarsBackgroundDark";

        /// <summary>
        /// Gets or sets the background color in light mode for pages on which a ContentDialog is opened.
        /// </summary>
        /// <remarks>The color is used for creating the opaque background color of the system bars in light mode when a modal page is
        /// opened in 'Overlay Mode'. The color is calculated by blending PageBackgroundColor and ContentDialogBackgroundOverlayColor.
        /// The default expected key is "PageBackground".</remarks>
        public string PageBackgroundColor { get; set; } = "PageBackground";

        /// <summary>
        /// Gets or sets the background color in dark mode for pages on which a ContentDialog is opened.
        /// </summary>
        /// <remarks>The color is used for creating the opaque background color of the system bars in dark mode when a modal page is
        /// opened in 'Overlay Mode'. The color is calculated by blending PageBackgroundDarkColor and ContentDialogBackgroundOverlayDarkColor.
        /// The default expected key is "PageBackgroundDark".</remarks>
        public string PageBackgroundDarkColor { get; set; } = "PageBackgroundDark";

        /// <summary>
        /// Gets or sets the background color in light mode used for the Maui NavigationBar.
        /// </summary>
        /// <remarks>The default expected key is "MauiNavigationBarBackgroundColor".</remarks>
        public string MauiNavigationBarBackgroundColor { get; set; } = "PageBackground";

        /// <summary>
        /// Gets or sets the background color in dark mode used for the Maui NavigationBar.
        /// </summary>
        /// <remarks>The default expected key is "PageBackgroundDark".</remarks>
        public string MauiNavigationBarBackgroundDarkColor { get; set; } = "PageBackgroundDark";

        /// <summary>
        /// Gets or sets the background color in light mode used for text in Maui NavigationBar.
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarText".</remarks>
        public string MauiNavigationBarTextColor { get; set; } = "NavigationBarText";

        /// <summary>
        /// Gets or sets the background color in dark mode used for text in Maui NavigationBar.
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarTextDark".</remarks>
        public string MauiNavigationBarTextDarkColor { get; set; } = "NavigationBarTextDark";
    }


    public class MauiExtensionsConfiguration
    {
        private Func<Style>? _alertDialogBorderStyleProvider;
        private Func<Style>? _alertDialogButtonStyleProvider;

        private Style? _alertDialogBorderStyle;
        private Style? _alertDialogButtonStyle;

        /// <summary>
        /// Property holding all resource keys used by the cw.MauiExtensions.Services library.
        /// </summary>
        public MauiExtensionsResourceKeys ResourceKeys { get; set; } = new();

        /*
        /// <summary>
        /// Gets or sets the default style for AlertDialogs.
        /// If not set, the library will use its built-in default style.
        /// </summary>
        public Style? AlertDialogBorderStyle
        {
            get => _alertDialogBorderStyle ?? _alertDialogBorderStyleProvider?.Invoke();
            set => _alertDialogBorderStyle = value;
        }

        /// <summary>
        /// Gets or sets the default style for the buttons in AlertDialog.
        /// If not set, the library will use its built-in default style.
        /// </summary>
        public Style? AlertDialogButtonStyle
        {
            get => _alertDialogButtonStyle ?? _alertDialogButtonStyleProvider?.Invoke();
            set => _alertDialogButtonStyle = value;
        }

        /// <summary>
        /// Gets or sets the background overlay color for ContentDialog (used when the dialog is displayed).
        /// If not set, the library will automatically derive the color from the underlying page background.
        /// </summary>
        public Color? ContentDialogBackgroundOverlayColor
        {
            get => _contentDialogBackgroundOverlayColor ?? _contentDialogBackgroundOverlayColorProvider?.Invoke();
            set => _contentDialogBackgroundOverlayColor = value;
        }
        */

        /// <summary>
        /// Gets or sets a value indicating whether to enable integration with CommunityToolkit.Maui features.
        /// </summary>
        /// <remarks>The default is false meaning that the library provides full support for modal pages and Popups.
        /// When set to true the library will NOT be responsible for setting the right colors for system bars, like
        /// the status and navigation bars. It is expected that the CommunityToolkit.Maui library takes care of this.</remarks>
        public bool UseCommunityToolkitMaui { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the library's status bar styling is applied.
        /// </summary>
        /// <remarks>Set this property to <see langword="true"/> to allow the library to set the status bar color using
        /// the configured keys <see cref="MauiExtensionsResourceKeys"/>.
        /// Set to <see langword="false"/> to not apply any styling within the library.</remarks>
        public bool UseSystemStatusBarStyling { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the library's navigation bar styling is applied.
        /// </summary>
        /// <remarks>Set this property to <see langword="true"/> to allow the library to set the navigation bar color using
        /// the configured keys <see cref="MauiExtensionsResourceKeys"/>.
        /// Set to <see langword="false"/> to not apply any styling within the library.</remarks>
        public bool UseSystemNavigationBarStyling { get; set; } = true;

        /*
        /// <summary>
        /// Sets a provider function that will be called to get the AlertDialogBorderStyle when needed.
        /// This allows you to access Application.Current.Resources after the app is initialized.
        /// </summary>
        public void SetAlertDialogBorderStyleProvider(Func<Style> provider)
        {
            _alertDialogBorderStyleProvider = provider;
        }

        /// <summary>
        /// Sets a provider function that will be called to get the AlertDialogButtonStyle when needed.
        /// This allows you to access Application.Current.Resources after the app is initialized.
        /// </summary>
        public void SetAlertDialogButtonStyleProvider(Func<Style> provider)
        {
            _alertDialogButtonStyleProvider = provider;
        }

        /// <summary>
        /// Sets a provider function that will be called to get the ContentDialogBackgroundOverlayColor when needed.
        /// This allows you to access Application.Current.Resources after the app is initialized.
        /// </summary>
        public void SetContentDialogBackgroundOverlayColorProvider(Func<Color> provider)
        {
            _contentDialogBackgroundOverlayColorProvider = provider;
        }
        */

        /// <summary>
        /// 
        /// </summary>
        internal static MauiExtensionsConfiguration Instance { get; set; } = new();
    }
}
