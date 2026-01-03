using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    /// <summary>
    /// Example ViewModel demonstrating the use of IPageLifecycleAware and IAutoDisposableOnViewClosed interfaces.
    /// </summary>
    public partial class ExampleLifecycleViewModel : ObservableObject, IPageLifecycleAware, IAutoDisposableOnPageClosed
    {
        private bool _isDisposed;

        public ExampleLifecycleViewModel()
        {
            Debug.WriteLine("ExampleLifecycleViewModel: Constructor called");
        }

        /// <summary>
        /// Called when the page is navigated to and about to appear.
        /// Use this to:
        /// - Start timers
        /// - Subscribe to events
        /// - Refresh data
        /// - Start animations
        /// </summary>
        public void OnNavigatedTo()
        {
            Debug.WriteLine("ExampleLifecycleViewModel: OnNavigatedTo - Page is appearing");
            
            // Example: Start a timer or refresh data
            // _refreshTimer?.Start();
            // await LoadDataAsync();
        }

        /// <summary>
        /// Called when the page is navigated away from and about to disappear.
        /// Use this to:
        /// - Stop timers
        /// - Unsubscribe from events (that should pause when page is not visible)
        /// - Pause animations
        /// - Save state
        /// </summary>
        public void OnNavigatedFrom()
        {
            Debug.WriteLine("ExampleLifecycleViewModel: OnNavigatedFrom - Page is disappearing");
            
            // Example: Stop timers or pause operations
            // _refreshTimer?.Stop();
        }

        /// <summary>
        /// Called when the page is removed from the navigation stack.
        /// Use this to:
        /// - Clean up resources
        /// - Unsubscribe from all events
        /// - Dispose timers
        /// - Cancel pending tasks
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            Debug.WriteLine("ExampleLifecycleViewModel: Dispose - Cleaning up resources");
            
            // Clean up resources here
            // _refreshTimer?.Dispose();
            // _cancellationTokenSource?.Cancel();
            // _cancellationTokenSource?.Dispose();
            
            _isDisposed = true;
        }
    }
}
