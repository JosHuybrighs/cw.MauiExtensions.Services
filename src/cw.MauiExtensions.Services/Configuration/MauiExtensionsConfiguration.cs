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
        /// Gets or sets the background color in light mode used for the Maui NavigationBar (back button bar).
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarBackground".</remarks>
        public string NavigationBarBackgroundColor { get; set; } = "NavigationBarBackground";

        /// <summary>
        /// Gets or sets the background color in dark mode used for the Maui NavigationBar (back button bar).
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarBackgroundDark".</remarks>
        public string NavigationBarBackgroundDarkColor { get; set; } = "NavigationBarBackgroundDark";

        /// <summary>
        /// Gets or sets the text color in light mode used for the Maui NavigationBar (back button bar).
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarText".</remarks>
        public string NavigationBarTextColor { get; set; } = "NavigationBarText";

        /// <summary>
        /// Gets or sets the text color in dark mode used for the Maui NavigationBar (back button bar).
        /// </summary>
        /// <remarks>The default expected key is "NavigationBarTextDark".</remarks>
        public string NavigationBarTextDarkColor { get; set; } = "NavigationBarTextDark";
    }


    public class MauiExtensionsConfiguration
    {
        //private Func<Style>? _alertDialogBorderStyleProvider;
        //private Func<Style>? _alertDialogButtonStyleProvider;

        //private Style? _alertDialogBorderStyle;
        //private Style? _alertDialogButtonStyle;

        /// <summary>
        /// Property holding all resource keys used by the cw.MauiExtensions.Services library.
        /// </summary>
        public MauiExtensionsResourceKeys ResourceKeys { get; set; } = new();

        public bool DrawUnderSystemBars { get; set; } = true;
        public bool AppHasNavigationBar { get; set; } = true;


        /// <summary>
        /// Gets or sets a value indicating whether to enable smart handling of system bar colors with modal pages.
        /// </summary>
        /// <remarks>The default is true meaning that the library provides full support for setting a correct color and
        /// tint on system bars, like the status and navigation bars, when showing modal pages and Popups.
        /// When set to false the library will NOT be responsible for setting the right colors for system bars. It is
        /// expected that other libraries like CommunityToolkit.Maui takes care of this.</remarks>
        public bool UseSmartSystemBarColoringWithModals { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether smart coloring is applied to system bars based on the current theme
        /// or content.
        /// </summary>
        /// <remarks>When enabled, the system bar colors automatically adjust to improve visibility and
        /// match the application's appearance. Disabling this property may result in less optimal contrast or
        /// integration with the system UI. To overcome this you can use CommunityToolkit.Maui and add
        /// toolkit:StatusBarBehavior to your startup page. This works well for android API 35+ but not for lower versions.</remarks>
        public bool UseSmartSystemBarColoring { get; set; } = true;

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


        internal static MauiExtensionsConfiguration Instance { get; set; } = new();
    }
}
