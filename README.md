# cw.MauiExtensions.Services

A reusable .NET MAUI library providing navigation services, popups, alert dialogs, ViewModel lifecycle management, and proper Android
system bar handling.

## 1. Android System Bars coloring

The latest UI recommendations for mobile apps call for a UI where all page background colors extend to the Android system bars.
It is called Edge-to-edge and is enabled in MAUI by setting DecorFitsSystemWindows to false for Android. Now, when you want to use
the MAUI NavigationPage (e.g. using AppShell or your own page navigation service) any ContentPage will still start at the bottom of
the Status Bar since MAUI applies a gap instead of overlapping it with the Status Bar.

If you want to have the same background color for the Status Bar, the MAUI Navigation bar, the main page, non-modal pages pushed on 
the stack, modal pages, popup dialogs, and the android system navigation bar at the bottom, some work needs to be done.

### Things to know:
1. The background color of the Android Status Bar is pre-configured. Its color is either colorPrimaryDark (defined in colors.xml of
the values and values-night folders in Platforms/Android/Resources) on API < 35 or colorPrimary for API 35+.
The tint of the icons is per default always white, independent of night or day mode. When you set the background color to
white the icons will not be visible.

2. The background color of the Android Navigation Bar (the bottom bar on modern Android devices) is fixed with
API < 35 (but I think it is defined by Android). With API 35+ the color is the same as the background of the page.

3. The NavigationBar of the MAUI NavigationPage has its own background color.

4. CommunityToolkit.Maui offers a behaviour to configure the Status Bar color and icons but doesn't do this correctly with modal
pages and popups. You need to implement a Dialog Fragment Service to get the colors right.

5. CommunityToolkit.Maui offers nothing to control the color of the bottom android navigation bar (which you might want with API < 35).

6. Modal pages and popups:
- With a Modal page on API 35+ the Status bar and the bottom navigation bar get the background color of the page
but the ContentView starts below the bar. If the page is not a popup page with a transparent background then the
icons in the status bar will not be visible, unless you provide your own Dialog Fragment Service to set the colors and tint.
- On API < 35 the page starts at the bottom of the status bar and ends at the top of the navigation bar. Both system
bars keep the background color and icons tint.

## 2. ViewModel Lifecycle Management

When using pages and popups, it is important to manage the lifecycle of the associated ViewModels properly. MAUI doesn't provide
built-in support for ViewModel lifecycle events, which can lead to memory leaks and unexpected behavior.

cw.MauiExtensions.Services provides automatic ViewModel lifecycle management through two opt-in interfaces:

### IPageLifecycleAware

Receive notifications when your ViewModel's page appears or disappears:

```csharp
using cw.MauiExtensions.Services.Interfaces;

public class MyViewModel : ObservableObject, IPageLifecycleAware
{
    public void OnNavigatedTo()
    {
        // Called when page appears
        // Start timers, refresh data, subscribe to events
    }

    public void OnNavigatedFrom()
    {
        // Called when page disappears
        // Stop timers, pause operations
    }
}
```

**Use cases:**
- Refresh data when page appears
- Start/stop timers based on visibility
- Subscribe/unsubscribe from real-time updates
- Track page views for analytics
- Save state when navigating away

### IAutoDisposableOnViewClosed

Automatically dispose resources when the page is removed from navigation:

```csharp
using cw.MauiExtensions.Services.Interfaces;

public class MyViewModel : ObservableObject, 
    IPageLifecycleAware,
    IAutoDisposableOnViewClosed
{
    private Timer? _refreshTimer;
    private bool _isDisposed;

    public void OnNavigatedTo()
    {
        _refreshTimer = new Timer(5000);
        _refreshTimer.Start();
    }

    public void OnNavigatedFrom()
    {
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
}
```

**Use cases:**
- Clean up event subscriptions
- Dispose timers and HTTP clients
- Cancel ongoing async operations
- Release unmanaged resources
- Prevent memory leaks

### Lifecycle Event Flow

```
Page Created
    ↓
PageNavigationService hooks lifecycle events
    ↓
Page.Appearing → IPageLifecycleAware.OnNavigatedTo()
    ↓
[User interacts with page]
    ↓
Page.Disappearing → IPageLifecycleAware.OnNavigatedFrom()
    ↓
Page Removed/Popped
    ↓
Unhook events → IAutoDisposableOnViewClosed.Dispose()
    ↓
PageRemoved event raised
```

### Important Notes

- **Opt-in**: Only implement the interfaces you need
- **Automatic**: No manual event hooking/unhooking required
- **Memory Safe**: Events are properly unhooked to prevent leaks
- **Works everywhere**: Main pages, content pages, modal pages
- **OnNavigatedTo** fires every time the page appears (including when returning from a modal)
- **Dispose** fires only once when the page is permanently removed

For detailed examples and best practices, see the [Page Lifecycle Management Guide](Docs/PageLifecycleManagement.md).

## Features

This library provides:
- **PageNavigationService**: Create a main page, open and close non-modal and modal pages
- **ViewModel Lifecycle Management**: Automatic lifecycle event notifications and disposal
- **ContentDialog**: Overlay-style modal dialogs with semi-transparent backgrounds
- **AlertDialog**: Standard alert/confirmation dialogs
- **Smart Android Status Bar Handling**: Automatically configures status bar colors for different modal page types
- **Page Removal Events**: Subscribe to page removal notifications via events or WeakReferenceMessenger

## Installation

1. Add a reference to the `cw.MauiExtensions.Services` project in your MAUI application.

2. Register the services in your `MauiProgram.cs`:

```csharp
using cw.MauiExtensions.Services.Extensions;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiExtensionsServices() // Add this line
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
```

That's it! The PageNavigationService will now automatically manage ViewModel lifecycles for all pages.

## ViewModel Lifecycle Examples

### Example 1: Data Refresh on Page Appear

```csharp
public class ProductListViewModel : ObservableObject, IPageLifecycleAware
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
}
```

### Example 2: Timer Management

```csharp
public class LiveDataViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnViewClosed
{
    private System.Timers.Timer? _refreshTimer;
    private bool _isDisposed;

    public void OnNavigatedTo()
    {
        // Start refresh timer when page is visible
        _refreshTimer = new System.Timers.Timer(5000);
        _refreshTimer.Elapsed += OnRefresh;
        _refreshTimer.Start();
    }

    public void OnNavigatedFrom()
    {
        // Stop timer when page is not visible
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

    private void OnRefresh(object? sender, EventArgs e)
    {
        // Refresh data
    }
}
```

### Example 3: Event Subscription Management

```csharp
public class NotificationsViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnViewClosed
{
    private readonly INotificationService _notificationService;
    private bool _isDisposed;

    public NotificationsViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void OnNavigatedTo()
    {
        // Subscribe when page is visible
        _notificationService.NotificationReceived += OnNotificationReceived;
    }

    public void OnNavigatedFrom()
    {
        // Unsubscribe when page is hidden to save resources
        _notificationService.NotificationReceived -= OnNotificationReceived;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        
        // Ensure cleanup
        _notificationService.NotificationReceived -= OnNotificationReceived;
        
        _isDisposed = true;
    }

    private void OnNotificationReceived(object? sender, NotificationEventArgs e)
    {
        // Handle notification
    }
}
```

### Example 4: Cancellation Token for Async Operations

```csharp
public class DataViewModel : ObservableObject, 
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
        // Cancel ongoing operations when navigating away
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
            // Expected when navigation away happens
        }
    }
}
```

## Page Removal Notifications

The library provides multiple ways to be notified when pages are removed:

### Option 1: Event-Based (In App.xaml.cs)

```csharp
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Events;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Subscribe to page removal events
        PageNavigationService.Instance.PageRemoved += OnPageRemoved;
    }

    private void OnPageRemoved(object? sender, PageRemovedEventArgs e)
    {
        Debug.WriteLine($"Page removed: {e.RemovedPage.GetType().Name}");
        
        // Optionally broadcast via WeakReferenceMessenger
        WeakReferenceMessenger.Default.Send(e);
    }
}
```

### Option 2: WeakReferenceMessenger (In ViewModels)

```csharp
using CommunityToolkit.Mvvm.Messaging;
using cw.MauiExtensions.Services.Events;

public class MyViewModel : ObservableObject, IRecipient<PageRemovedEventArgs>
{
    public MyViewModel()
    {
        WeakReferenceMessenger.Default.Register<PageRemovedEventArgs>(this);
    }

    public void Receive(PageRemovedEventArgs message)
    {
        if (message.RemovedPage is Views.DetailPage)
        {
            // React to specific page removal
        }
    }
}
```

## Required Color Resources

Define these colors in your `App.xaml` or `Colors.xaml`:

```xaml
<!-- System bars (Status bar and Navigation bar on Android) -->
<Color x:Key="SystemBarsBackground">#FFFFFF</Color>
<Color x:Key="SystemBarsBackgroundDark">#000000</Color>

<!-- Page backgrounds -->
<Color x:Key="PageBackground">#FFFFFF</Color>
<Color x:Key="PageBackgroundDark">#000000</Color>

<!-- MAUI NavigationBar (the bar with back button and title) -->
<Color x:Key="NavigationBarText">#101010</Color>
<Color x:Key="NavigationBarTextDark">#FFFFFF</Color>

<!-- ContentDialog overlay backgrounds (semi-transparent for popups/dialogs) -->
<Color x:Key="ContentDialogBackgroundOverlay">#4C000000</Color>
<Color x:Key="ContentDialogBackgroundOverlayDark">#80000000</Color>
```

### Additional Colors for Dialog Borders

```xaml
<!-- Dialog border and background colors (optional, for custom styling) -->
<Color x:Key="ContentDialogBorderBackground">#FAFCFE</Color>
<Color x:Key="ContentDialogBorderBackgroundDark">#202020</Color>
<Color x:Key="ContentDialogBorderStroke">#E0E0E0</Color>
<Color x:Key="ContentDialogBorderStrokeDark">#3D3D3D</Color>
```

## Required Styles

Define these styles in your `Styles.xaml`:

```xaml
<!-- Border style for AlertDialog and ContentDialog -->
<Style x:Key="ContentDialogBorder" TargetType="Border">
    <Setter Property="VerticalOptions" Value="Center" />
    <Setter Property="Padding" Value="10,24,10,24"/>
    <Setter Property="StrokeShape" Value="RoundRectangle 15"/>
    <Setter Property="StrokeThickness" Value="1"/>
    <Setter Property="Stroke" Value="{AppThemeBinding Light={StaticResource ContentDialogBorderStroke}, Dark={StaticResource ContentDialogBorderStrokeDark}}" />
    <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource ContentDialogBorderBackground}, Dark={StaticResource ContentDialogBorderBackgroundDark}}" />
</Style>

<!-- Button style for AlertDialog buttons -->
<Style x:Key="TextOnlyButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource PrimaryDark}}" />
    <Setter Property="BorderWidth" Value="0" />
    <Setter Property="Padding" Value="8,4" />
</Style>
```

You can customize these resource keys in configuration if needed (see Configuration section below).

## Modal Page Modes

The library distinguishes between two types of modal pages for Android status bar handling:

1. **FullScreen Mode** (Default): Regular modal pages that cover the entire screen
   - Status bar color matches the page background color (`PageBackground` resource)
   - Use for: Settings pages, detail views, full-screen forms
   - **This is the default mode** - you don't need to set it explicitly

2. **Overlay Mode**: Popup/dialog-style modals with semi-transparent backgrounds
   - Status bar uses blended color calculated from `PageBackground` and `ContentDialogBackgroundOverlay` resources
   - Use for: Dialogs, popups, alerts, content pickers

### Usage Example

```csharp
using cw.MauiExtensions.Services.Helpers;

// Full-screen modal page - no need to set mode, FullScreen is default
public partial class MyModalPage : ContentPage
{
    public MyModalPage()
    {
        InitializeComponent();
        
        // ModalPageMode.FullScreen is the default - no action needed
    }
}

// Overlay modal (dialog/popup) - only set if creating custom overlay modals
public partial class MyCustomOverlayDialog : ContentPage
{
    public MyCustomOverlayDialog()
    {
        InitializeComponent();
        
        // Only needed for custom overlay dialogs
        ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
    }
}
```

**Note**: 
- `ContentDialog` and `AlertDialog` automatically set `ModalPageMode.Overlay` internally - you don't need to set it
- Regular modal pages default to `ModalPageMode.FullScreen` - you don't need to set it
- Only set `ModalPageMode.Overlay` explicitly if you're creating a custom overlay-style modal page

## Configuration

Configure the library in your `MauiProgram.cs` to customize resource keys and behavior:

```csharp
using cw.MauiExtensions.Services.Extensions;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiExtensionsServices(options =>
            {
                // Customize resource key names if you use different names in your app
                options.ResourceKeys.AlertDialogBorderStyle = "ContentDialogBorder";
                options.ResourceKeys.AlertDialogButtonStyle = "TextOnlyButton";
                options.ResourceKeys.SystemBarsBackgroundColor = "SystemBarsBackground";
                options.ResourceKeys.SystemBarsBackgroundDarkColor = "SystemBarsBackgroundDark";
                options.ResourceKeys.PageBackgroundColor = "PageBackground";
                options.ResourceKeys.PageBackgroundDarkColor = "PageBackgroundDark";
                options.ResourceKeys.ContentDialogBackgroundOverlayColor = "ContentDialogBackgroundOverlay";
                options.ResourceKeys.ContentDialogBackgroundOverlayDarkColor = "ContentDialogBackgroundOverlayDark";
                options.ResourceKeys.MauiNavigationBarBackgroundColor = "PageBackground";
                options.ResourceKeys.MauiNavigationBarBackgroundDarkColor = "PageBackgroundDark";
                options.ResourceKeys.MauiNavigationBarTextColor = "NavigationBarText";
                options.ResourceKeys.MauiNavigationBarTextDarkColor = "NavigationBarTextDark";
                
                // Enable/disable system bar styling
                options.UseSystemStatusBarStyling = true;   // Default: true
                options.UseSystemNavigationBarStyling = true; // Default: true
                
                // Use CommunityToolkit.Maui for status bar handling instead (default: false)
                options.UseCommunityToolkitMaui = false;
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ResourceKeys.*` | `string` | Various | Customize the resource key names the library looks for |
| `UseSystemStatusBarStyling` | `bool` | `true` | Enable/disable automatic status bar styling |
| `UseSystemNavigationBarStyling` | `bool` | `true` | Enable/disable automatic navigation bar styling |
| `UseCommunityToolkitMaui` | `bool` | `false` | Use CommunityToolkit.Maui for status bar instead of built-in service |

## Usage

### Alert Dialog

```csharp
using cw.MauiExtensions.Services.Views;

var dialog = new AlertDialog(
    title: "Confirm Action",
    text: "Are you sure you want to continue?",
    primaryBttnText: "Yes",
    secondaryBttnText: "No"
);

bool result = await dialog.ShowAsync();
if (result)
{
    // Primary button was clicked (Yes)
}
else
{
    // Secondary button was clicked (No)
}
```

### Content Dialog

```csharp
using cw.MauiExtensions.Services.Views;

var dialog = new ContentDialog
{
    ContentView = new MyCustomView()
};

var result = await dialog.ShowAsync();
// Handle result: ContentDialogResult.None, Primary, or Secondary
```

### Page Navigation

```csharp
using cw.MauiExtensions.Services.Core;

// Open a main page with NavigationPage
var navigationPage = PageNavigationService.Instance.OpenMainPage(
    typeof(HomePage), 
    new HomeViewModel()
);
Application.Current.MainPage = navigationPage;

// Navigate to a new page (pushes onto navigation stack)
await PageNavigationService.Instance.OpenContentPageAsync(
    typeof(DetailPage), 
    new DetailViewModel()
);

// Navigate with page replacement
await PageNavigationService.Instance.OpenContentPageAsync(
    typeof(SettingsPage), 
    new SettingsViewModel(),
    PageNavigationService.OpenMode.ReplaceCurrent
);

// Close current page
await PageNavigationService.Instance.CloseContentPageAsync();

// Open modal page
await PageNavigationService.Instance.OpenModalPageAsync(myModalPage);

// Close modal page
await PageNavigationService.Instance.CloseModalPageAsync();
```

## Advanced Configuration

### Using with CommunityToolkit.Maui

If you're using CommunityToolkit.Maui for status bar styling, you can disable the built-in DialogFragmentService:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseCommunityToolkitMaui = true; // Disables built-in DialogFragmentService
})
```

**Note**: When `UseCommunityToolkitMaui` is `true`, the library will NOT handle system bar colors for modal pages. You'll need to configure CommunityToolkit.Maui separately.

### Custom Resource Keys

All resource keys can be customized via the `ResourceKeys` property:

```csharp
.UseMauiExtensionsServices(options =>
{
    // Customize all resource keys to match your app's resource dictionary
    options.ResourceKeys.SystemBarsBackgroundColor = "MyStatusBarColor";
    options.ResourceKeys.SystemBarsBackgroundDarkColor = "MyStatusBarColorDark";
    options.ResourceKeys.PageBackgroundColor = "MyPageBg";
    options.ResourceKeys.PageBackgroundDarkColor = "MyPageBgDark";
    // ... etc
})
```

### Disabling Automatic Styling

If you want to handle system bars manually:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseSystemStatusBarStyling = false;
    options.UseSystemNavigationBarStyling = false;
})
```

## How Overlay Mode Works

When a modal page is set to `ModalPageMode.Overlay`, the library:

1. Reads the `PageBackground` color (the underlying page color)
2. Reads the `ContentDialogBackgroundOverlay` color (semi-transparent overlay)
3. Blends these two colors to create an opaque color for the Android system bars
4. Applies this blended color to the status bar and navigation bar

This ensures the system bars match the visual appearance of the overlay without requiring you to manually calculate and define additional color resources.

## Best Practices

### ViewModel Lifecycle
- **Always implement IAutoDisposableOnViewClosed** if your ViewModel subscribes to events or uses timers
- **Use the dispose pattern** to prevent double disposal
- **Keep OnNavigatedFrom() fast** - don't await async operations
- **Use CancellationToken** for long-running operations that should be cancelled on navigation
- **Unsubscribe in both OnNavigatedFrom() and Dispose()** for robustness

### Memory Management
- The library automatically unhooks page events to prevent leaks
- ViewModels implementing `IAutoDisposableOnViewClosed` are disposed when pages are removed
- Always dispose timers, HTTP clients, and other resources in the `Dispose()` method
- Use WeakReferenceMessenger for loose coupling between components

## Documentation

For more detailed information, see:
- [Page Lifecycle Management Guide](Docs/PageLifecycleManagement.md) - Comprehensive guide with examples
- [Page Removal Events](Docs/PageRemovalEvents.md) - Guide for subscribing to page removal notifications

## Troubleshooting

### MissingResourceException

If you get a `MissingResourceException`, ensure you have defined all required color resources in your `Colors.xaml` or `App.xaml`. The exception message will tell you which resource key is missing.

### Status Bar Icons Not Visible

On Android, if status bar icons are not visible (white icons on white background), ensure:
1. You've called `.UseMauiExtensionsServices()` in your `MauiProgram.cs`
2. Your color resources are defined correctly
3. `UseSystemStatusBarStyling` is set to `true` (default)

### Modal Pages Not Using Correct Colors

Ensure you've set the `ModalPageMode` on your modal pages:

```csharp
ModalPageProperties.SetMode(this, ModalPageMode.FullScreen); // or Overlay
```

### OnNavigatedTo/OnNavigatedFrom Not Called

- Ensure your ViewModel implements `IPageLifecycleAware`
- Verify the ViewModel is set as `BindingContext` before the page is pushed
- Check that the page is created through `PageNavigationService`

### Dispose Not Called

- Ensure ViewModel implements `IAutoDisposableOnViewClosed`
- Verify the page is actually being removed (not just hidden)
- Check that `PageNavigationService` is managing the navigation

### Memory Leaks

If you suspect memory leaks:
- Implement `IAutoDisposableOnViewClosed` on all ViewModels
- Always unsubscribe from events in `Dispose()`
- Cancel CancellationTokenSource in `Dispose()`
- Dispose timers, HTTP clients, and other IDisposable resources

### Colors Don't Update After Changing Configuration

The resource keys are read when needed. If you change `ResourceKeys` values after app initialization, you may need to restart the app for changes to take effect.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
