namespace cw.MauiExtensions.Services.Helpers
{
    /// <summary>
    /// Defines modal page presentation modes for Android status bar handling.
    /// </summary>
    public enum ModalPageMode
    {
        /// <summary>
        /// Full-screen modal page. Status bar color matches the page background.
        /// </summary>
        FullScreen,

        /// <summary>
        /// Overlay modal page (popup/dialog). Status bar color uses semi-transparent overlay color.
        /// </summary>
        Overlay
    }

    /// <summary>
    /// Attached properties for configuring modal page behavior, particularly for Android status bar handling.
    /// </summary>
    public static class ModalPageProperties
    {
        /// <summary>
        /// Attached property that defines how a modal page should be presented.
        /// This affects the Android status bar color handling.
        /// </summary>
        public static readonly BindableProperty ModeProperty = BindableProperty.CreateAttached("Mode",
                                                                                               typeof(ModalPageMode),
                                                                                               typeof(ModalPageProperties),
                                                                                               ModalPageMode.FullScreen);

        /// <summary>
        /// Gets the modal page mode for the specified page.
        /// </summary>
        public static ModalPageMode GetMode(BindableObject view)
        {
            return (ModalPageMode)view.GetValue(ModeProperty);
        }

        /// <summary>
        /// Sets the modal page mode for the specified page.
        /// </summary>
        public static void SetMode(BindableObject view, ModalPageMode value)
        {
            view.SetValue(ModeProperty, value);
        }
    }
}
