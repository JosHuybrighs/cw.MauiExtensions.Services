namespace cw.MauiExtensions.Services.Interfaces
{
    /// <summary>
    /// Interface for ViewModels that need to be notified of page lifecycle events.
    /// Implement this interface to receive notifications when the page appears or disappears.
    /// </summary>
    public interface IPageLifecycleAware
    {
        /// <summary>
        /// Called when the page is navigated to and is about to appear.
        /// This is triggered by the Page.Appearing event.
        /// </summary>
        void OnNavigatedTo();

        /// <summary>
        /// Called when the page is navigated away from and is about to disappear.
        /// This is triggered by the Page.Disappearing event.
        /// </summary>
        void OnNavigatedFrom();
    }
}
