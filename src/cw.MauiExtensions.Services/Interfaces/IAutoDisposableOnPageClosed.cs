namespace cw.MauiExtensions.Services.Interfaces
{
    /// <summary>
    /// Marker interface for ViewModels that should be automatically disposed when their associated page is removed from the navigation stack.
    /// Implement this interface along with IDisposable to enable automatic cleanup when the page is popped.
    /// </summary>
    public interface IAutoDisposableOnPageClosed : IDisposable
    {
    }
}
