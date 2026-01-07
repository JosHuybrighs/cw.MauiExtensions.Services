# Page Lifecycle Management

This document explains how to use the page lifecycle interfaces in the cw.MauiExtensions.Services library to manage ViewModel lifecycle events.

## Overview

The `PagePresentationService` automatically manages ViewModel lifecycle by hooking into MAUI page events. ViewModels can implement one or both of the following interfaces:

1. **`IPageLifecycleAware`**: Receive notifications when pages appear/disappear
2. **`IAutoDisposableOnViewClosed`**: Automatically dispose resources when page is removed

## Interfaces

### IPageLifecycleAware

```csharp
public interface IPageLifecycleAware
{
    void OnNavigatedTo();    // Called when page appears
    void OnNavigatedFrom();  // Called when page disappears
}
```

**When to use:**
- Start/stop timers based on page visibility
- Subscribe/unsubscribe from events
- Refresh data when page appears
- Pause/resume operations
- Track analytics (page views)

**Event mapping:**
- `OnNavigatedTo()` ? Triggered by `Page.Appearing` event
- `OnNavigatedFrom()` ? Triggered by `Page.Disappearing` event

### IAutoDisposableOnViewClosed

```csharp
public interface IAutoDisposableOnViewClosed : IDisposable
{
}
```

**When to use:**
- Clean up resources when page is permanently removed
- Cancel pending async operations
- Dispose timers, event subscriptions, etc.
- Release unmanaged resources

**Triggered when:**
- Page is popped from navigation stack
- Page is removed programmatically
- Modal page is closed
- App navigates to a new main page

## How It Works

```
Page Created
    ?
PagePresentationService hooks Page.Appearing & Page.Disappearing
    ?
Page.Appearing ? ViewModel.OnNavigatedTo() (if IPageLifecycleAware)
    ?
Page.Disappearing ? ViewModel.OnNavigatedFrom() (if IPageLifecycleAware)
    ?
Page Removed
    ?
Unhook events ? ViewModel.Dispose() (if IAutoDisposableOnViewClosed)
```

## Usage Examples

### Example 1: Basic Lifecycle Awareness

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

public partial class MyViewModel : ObservableObject, IPageLifecycleAware
{
    public void OnNavigatedTo()
    {
        Debug.WriteLine("Page appeared - refresh data");
        // Refresh data, start timers, etc.
    }

    public void OnNavigatedFrom()
    {
        Debug.WriteLine("Page disappeared - pause operations");
        // Stop timers, pause operations, etc.
    }
}
```

### Example 2: With Auto-Disposal

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;

public partial class MyViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnViewClosed
{
    private System.Timers.Timer? _refreshTimer;
    private bool _isDisposed;

    public MyViewModel()
    {
        _refreshTimer = new System.Timers.Timer(5000);
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;
    }

    public void OnNavigatedTo()
    {
        // Start refresh timer when page appears
        _refreshTimer?.Start();
    }

    public void OnNavigatedFrom()
    {
        // Stop refresh timer when page disappears
        _refreshTimer?.Stop();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _refreshTimer = null;

        _isDisposed = true;
    }

    private void OnRefreshTimerElapsed(object? sender, EventArgs e)
    {
        // Refresh data periodically
    }
}
```

### Example 3: Data Refresh on Page Appear

```csharp
public partial class ProductListViewModel : ObservableObject, IPageLifecycleAware
{
    private readonly IProductService _productService;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    public async void OnNavigatedTo()
    {
        // Refresh product list every time page appears
        await LoadProductsAsync();
    }

    public void OnNavigatedFrom()
    {
        // Nothing to do when page disappears
    }

    private async Task LoadProductsAsync()
    {
        // Load fresh data
        var products = await _productService.GetProductsAsync();
        // Update ObservableCollection...
    }
}
```

### Example 4: Cancellation Token Management

```csharp
public partial class DataViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnViewClosed
{
    private CancellationTokenSource? _cts;
    private bool _isDisposed;

    public async void OnNavigatedTo()
    {
        _cts = new CancellationTokenSource();
        await LoadDataAsync(_cts.Token);
    }

    public void OnNavigatedFrom()
    {
        // Cancel ongoing operations when page disappears
        _cts?.Cancel();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _isDisposed = true;
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Long-running operation
            await Task.Delay(5000, cancellationToken);
            // Load data...
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled - this is expected
        }
    }
}
```

### Example 5: Event Subscription Management

```csharp
public partial class LiveDataViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnViewClosed
{
    private readonly IDataService _dataService;
    private bool _isDisposed;

    public LiveDataViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public void OnNavigatedTo()
    {
        // Subscribe to live updates when page is visible
        _dataService.DataUpdated += OnDataUpdated;
    }

    public void OnNavigatedFrom()
    {
        // Unsubscribe when page is not visible to save resources
        _dataService.DataUpdated -= OnDataUpdated;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        // Ensure unsubscribed
        _dataService.DataUpdated -= OnDataUpdated;

        _isDisposed = true;
    }

    private void OnDataUpdated(object? sender, DataEventArgs e)
    {
        // Handle data update
    }
}
```

## Best Practices

### 1. **Always Unsubscribe in Dispose**
Even if you unsubscribe in `OnNavigatedFrom()`, always ensure cleanup in `Dispose()`:

```csharp
public void Dispose()
{
    if (_isDisposed) return;
    
    // Ensure all event handlers are removed
    SomeService.SomeEvent -= OnSomeEvent;
    
    _isDisposed = true;
}
```

### 2. **Use Dispose Pattern**
Implement the dispose pattern to prevent double disposal:

```csharp
private bool _isDisposed;

public void Dispose()
{
    if (_isDisposed) return;
    
    // Cleanup code here
    
    _isDisposed = true;
}
```

### 3. **Null-Check Before Disposing**
Always check for null before disposing resources:

```csharp
public void Dispose()
{
    _timer?.Stop();
    _timer?.Dispose();
    _cts?.Cancel();
    _cts?.Dispose();
}
```

### 4. **Async in OnNavigatedTo**
If you need async operations in `OnNavigatedTo()`, use fire-and-forget pattern carefully:

```csharp
public async void OnNavigatedTo()
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        // Handle error
        Debug.WriteLine($"Error loading data: {ex.Message}");
    }
}
```

### 5. **Don't Block in OnNavigatedFrom**
Keep `OnNavigatedFrom()` fast and non-blocking:

```csharp
public void OnNavigatedFrom()
{
    // Good: Quick synchronous cleanup
    _timer?.Stop();
    
    // Bad: Don't await async operations
    // await SaveStateAsync(); // DON'T DO THIS
    
    // Instead: Fire and forget if needed
    _ = SaveStateAsync();
}
```

## Lifecycle Event Order

### Regular Navigation (Push)
```
1. Page Constructor
2. Page.Appearing ? OnNavigatedTo()
3. Page visible to user
4. User navigates back
5. Page.Disappearing ? OnNavigatedFrom()
6. Page.Popped ? Dispose()
```

### Modal Navigation
```
1. Modal Page Constructor
2. Page.Appearing ? OnNavigatedTo()
3. Modal visible to user
4. User closes modal
5. Page.Disappearing ? OnNavigatedFrom()
6. Modal closed ? Dispose()
```

### Replacing Main Page
```
1. Old pages: OnNavigatedFrom() ? Dispose()
2. New page Constructor
3. New page: OnNavigatedTo()
```

## Common Use Cases

| Use Case | Interface | Method |
|----------|-----------|--------|
| Refresh data on page show | `IPageLifecycleAware` | `OnNavigatedTo()` |
| Start timer when visible | `IPageLifecycleAware` | `OnNavigatedTo()` |
| Stop timer when hidden | `IPageLifecycleAware` | `OnNavigatedFrom()` |
| Dispose timer completely | `IAutoDisposableOnViewClosed` | `Dispose()` |
| Track page views | `IPageLifecycleAware` | `OnNavigatedTo()` |
| Cancel async operations | Both | Both methods |
| Unsubscribe from events | `IAutoDisposableOnViewClosed` | `Dispose()` |
| Save state before hiding | `IPageLifecycleAware` | `OnNavigatedFrom()` |

## Memory Leak Prevention

The `PagePresentationService` automatically:
- ? Unhooks page events when page is removed
- ? Calls `Dispose()` on ViewModels implementing `IAutoDisposableOnViewClosed`
- ? Raises `PageRemoved` event for additional cleanup

You should:
- ? Implement `IAutoDisposableOnViewClosed` for ViewModels that subscribe to events
- ? Always unsubscribe from events in `Dispose()`
- ? Cancel ongoing async operations in `Dispose()`
- ? Dispose timers, HTTP clients, and other resources

## Troubleshooting

### OnNavigatedTo not called
- Ensure ViewModel implements `IPageLifecycleAware`
- Ensure ViewModel is set as `BindingContext` before page is pushed
- Check that page lifecycle events aren't being suppressed

### Dispose not called
- Ensure ViewModel implements `IAutoDisposableOnViewClosed`
- Check that page is actually being removed (not just hidden)
- Verify `PagePresentationService` is managing the navigation

### Multiple OnNavigatedTo calls
- `OnNavigatedTo()` is called every time the page appears
- This includes coming back from a modal or child page
- Use a flag if you only want initialization once:

```csharp
private bool _isInitialized;

public async void OnNavigatedTo()
{
    if (!_isInitialized)
    {
        await InitializeAsync();
        _isInitialized = true;
    }
    else
    {
        await RefreshAsync();
    }
}
```
