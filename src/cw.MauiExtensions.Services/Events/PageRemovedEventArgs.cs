namespace cw.MauiExtensions.Services.Events
{
    /// <summary>
    /// Event arguments for the PageRemoved event.
    /// </summary>
    public class PageRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the page that was removed from the navigation stack.
        /// </summary>
        public Page RemovedPage { get; }

        /// <summary>
        /// Initializes a new instance of the PageRemovedEventArgs class.
        /// </summary>
        /// <param name="removedPage">The page that was removed.</param>
        public PageRemovedEventArgs(Page removedPage)
        {
            RemovedPage = removedPage ?? throw new ArgumentNullException(nameof(removedPage));
        }
    }
}
