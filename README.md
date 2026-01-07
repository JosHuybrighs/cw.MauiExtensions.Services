# cw.MauiExtensions.Services

A reusable .NET MAUI library providing navigation services, popups, alert dialogs, ViewModel lifecycle management, and proper Android
system bar handling.
### Features

This library provides:
- **PagePresentationService**: Can create any type of start page, push and pop non-modal pages, and modal pages
- **ViewModel Lifecycle Management**: Automatic lifecycle event notifications and disposal
- **ContentDialog**: Overlay-style modal dialogs (popups) with semi-transparent backgrounds
- **AlertDialog**: Standard alert/confirmation dialogs
- **Smart Android System Bar Handling**: Automatically configures status and navigation bar colors
- **Page Removal Events**: Subscribe to page removal notifications via events or WeakReferenceMessenger


## 1. Android System Bars coloring

The latest UI recommendations for mobile apps call for a UI where all page background colors extend to the Android system bars.
It is called Edge-to-edge and is enabled in MAUI by setting `DecorFitsSystemWindows` to false for Android. 

### Two Display Modes

This library supports two distinct display modes for Android:

#### Standard Mode (Default)
When you use the MAUI NavigationPage (e.g. using AppShell or the PagePresentationService), any ContentPage will start at the bottom of
the Status Bar since MAUI applies a gap instead of overlapping it with the Status Bar. The library ensures consistent background colors
across:
- Android Status Bar
- MAUI Navigation bar (with back button and title)
- Main page
- Non-modal pages pushed on the stack
- Modal pages
- Popup dialogs
- Android system navigation bar at the bottom

#### Edge-to-Edge Mode
When `DrawUnderSystemBars` is enabled and your app has no MAUI NavigationBar (`AppHasNavigationBar = false`), the library enables
true edge-to-edge display where:
- Page content draws under the status bar
- Status bar becomes transparent
- You control spacing via padding or margins
- Content extends to screen edges

To enable edge-to-edge mode:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.DrawUnderSystemBars = true;  // Enable edge-to-edge
    options.AppHasNavigationBar = false; // Required: app must not use MAUI NavigationBar
})
```

**Important**: Edge-to-edge mode only works when your app does NOT use a MAUI NavigationBar. If you're using `NavigationPage`,
`Shell`, or `PagePresentationService` with navigation features, use standard mode instead.

### Things to know:

1. **Status Bar Color**: The background color of the Android Status Bar can be configured via resource keys. The tint of the icons
is automatically calculated based on the background color brightness (dark icons for light backgrounds, light icons for dark backgrounds).

2. **Navigation Bar Color** (Android bottom bar):
   - API < 35: Color can be controlled via resource keys
   - API 35+: Color automatically matches the page background

3. **MAUI NavigationBar**: Has its own background color (configurable via resource keys).

4. **Modal Pages and Dialogs**: The library includes a Dialog Fragment Service that correctly handles system bar colors for:
   - Modal pages (full-screen and overlay modes)
   - Popup dialogs
   - Works correctly across all Android API levels (26+)

5. **CommunityToolkit.Maui Comparison**: While CommunityToolkit.Maui offers StatusBarBehavior, it doesn't correctly handle modal
pages and popups across all Android versions. This library's Dialog Fragment Service provides proper support for all scenarios.

6. **Modal Behavior by API Level**:
   - API 35+: System bars automatically match page background; status bar icons tint is managed by the library
   - API < 35: System bars maintain configured colors; Dialog Fragment Service ensures consistency

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
PagePresentationService hooks lifecycle events
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


## 3. Installation

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

That's it! you can now use the PagePresentationService and automatically manage ViewModel lifecycles for all pages.

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
        PagePresentationService.Instance.PageRemoved += OnPageRemoved;
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
<Color x:Key="NavigationBarBackground">#FFFFFF</Color>
<Color x:Key="NavigationBarBackgroundDark">#000000</Color>
<Color x:Key="NavigationBarText">#101010</Color>
<Color x:Key="NavigationBarTextDark">#FFFFFF</Color>

<!-- ContentDialog overlay backgrounds (semi-transparent for popups/dialogs) -->
<Color x:Key="ContentDialogBackgroundOverlay">#4C000000</Color>
<Color x:Key="ContentDialogBackgroundOverlayDark">#80000000</Color>
```

**Note**: When using Edge-to-Edge mode (`DrawUnderSystemBars = true`), these colors still affect modal dialogs and popups, but the
main page content will draw under the transparent status bar.
```

### Additional required Colors in the ContentDialogBorder style when AlertDialog is used

```xaml
<!-- Dialog border and background colors (optional, for custom styling) -->
<Color x:Key="ContentDialogBorderBackground">#FAFCFE</Color>
<Color x:Key="ContentDialogBorderBackgroundDark">#202020</Color>
<Color x:Key="ContentDialogBorderStroke">#E0E0E0</Color>
<Color x:Key="ContentDialogBorderStrokeDark">#3D3D3D</Color>

## Required Styles

Define these styles in your `Styles.xaml`:

```xaml
<!-- Border style for AlertDialog (but can also be used for your version of a ContentDialog) -->
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

## Modal Page Modes

The library distinguishes between two types of modal pages for Android status bar handling:

1. **FullScreen Mode** (Default): Regular modal pages that cover the entire screen
- Status bar color matches the page background color (`SystemBarsBackground` resource)
- Status bar icons tint is automatically calculated based on background brightness
- Use for: Settings pages, detail views, full-screen forms
- **This is the default mode** - you don't need to set it explicitly

2. **Overlay Mode**: Popup/dialog-style modals with semi-transparent backgrounds
- Status bar uses blended color calculated from `PageBackground` and `ContentDialogBackgroundOverlay` resources
- Status bar icons tint is automatically calculated based on the blended color brightness
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
- `ContentDialog` and `AlertDialog` automatically set `ModalPageMode.Overlay` internally - you don't need to set it.
- Regular modal pages default to `ModalPageMode.FullScreen` - you don't need to set it.
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
                options.ResourceKeys.NavigationBarBackgroundColor = "PageBackground";
                options.ResourceKeys.NavigationBarBackgroundDarkColor = "PageBackgroundDark";
                options.ResourceKeys.NavigationBarTextColor = "NavigationBarText";
                options.ResourceKeys.NavigationBarTextDarkColor = "NavigationBarTextDark";
                options.ResourceKeys.ContentDialogBackgroundOverlayColor = "ContentDialogBackgroundOverlay";
                options.ResourceKeys.ContentDialogBackgroundOverlayDarkColor = "ContentDialogBackgroundOverlayDark";
                
                // Enable/disable system bar styling
                options.UseSmartSystemBarColoring = true; // Default: true
                options.UseSystemStatusBarStyling = true;   // Default: true
                options.UseSystemNavigationBarStyling = true; // Default: true
                
                // Enable/disable smart system bar coloring for modals
                options.UseSmartSystemBarColoringWithModals = true; // Default: true
                
                // Edge-to-edge display mode (only works when AppHasNavigationBar = false)
                options.DrawUnderSystemBars = false; // Default: false
                options.AppHasNavigationBar = true;  // Default: true - set to false for edge-to-edge
                
                // Configure edge-to-edge display
                options.DrawUnderSystemBars = false; // Default: false
                options.AppHasNavigationBar = true;  // Default: true
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
| `UseSmartSystemBarColoring` | `bool` | `true` | Enable/disable smart system bar coloring at app startup and resume |
| `UseSystemStatusBarStyling` | `bool` | `true` | Enable/disable automatic status bar color and icon styling |
| `UseSystemNavigationBarStyling` | `bool` | `true` | Enable/disable automatic navigation bar (bottom bar) color styling |
| `UseSmartSystemBarColoringWithModals` | `bool` | `true` | Enable/disable Dialog Fragment Service for modal pages and popups |
| `DrawUnderSystemBars` | `bool` | `false` | Enable edge-to-edge mode (requires `AppHasNavigationBar = false`) |
| `AppHasNavigationBar` | `bool` | `true` | Set to `false` if your app doesn't use MAUI NavigationBar (required for edge-to-edge) |
| `DrawUnderSystemBars` | `bool` | `false` | Enable edge-to-edge display (content draws under status bar) |
| `AppHasNavigationBar` | `bool` | `true` | Set to false if your app does NOT use MAUI NavigationBar |

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

### Opening and closing pages

```csharp
using cw.MauiExtensions.Services.Core;

// Open the root page of a NavigationPage supporting a navigation bar
// e.g. in App.xaml.cs
protected override Window CreateWindow(IActivationState? activationState)
{
    var page = PagePresentationService.Instance.OpenMainNavigationPage(typeof(Views.HomePage), new ViewModels.HomeViewModel());
    return new Window(page);
}

// Navigate to a new page (pushes onto navigation stack)
await PagePresentationService.Instance.PushPageAsync(
    typeof(DetailPage), 
    new DetailViewModel()
);

// Navigate with page replacement
await PagePresentationService.Instance.PushPageAsync(
    typeof(SettingsPage), 
    new SettingsViewModel(),
    PagePresentationService.OpenMode.ReplaceCurrent
);

// Close the current page on the navigation stack
await PagePresentationService.Instance.PopPageAsync();

// Open a modal page
await PagePresentationService.Instance.OpenModalPageAsync(myModalPage);

// Close modal page
await PagePresentationService.Instance.CloseModalPageAsync();
```

## Advanced Configuration

### Using with CommunityToolkit.Maui

If you're using CommunityToolkit.Maui for status bar styling at startup of the app, you can disable `UseSmartSystemBarColoring`:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseSmartSystemBarColoring = false; // Disables built-in setting of system bar colors at startup
})
```

**Note**: When `UseSmartSystemBarColoring` is `false`, the library will NOT configure the system bars at startup or on app resume.
You'll need to use another library (e.g., CommunityToolkit.Maui). However, CommunityToolkit.Maui's `StatusBarBehavior` works well
with Android API 35+ but may have issues with lower API levels.

If you're using CommunityToolkit.Maui for status bar styling of modal pages (e.g., Popups), you can disable `UseSmartSystemBarColoringWithModals`:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseSmartSystemBarColoringWithModals = false; // Disables built-in DialogFragmentService
})
```

**Note**: When `UseSmartSystemBarColoringWithModals` is `false`, the library will NOT handle system bar colors for modal pages and dialogs.
CommunityToolkit.Maui's Dialog Fragment Service typically works well with Android API 35+ but may not handle overlay-style modals correctly
across all Android versions. This library's implementation provides consistent behavior across all supported API levels (26+).

### Edge-to-Edge Mode

To enable true edge-to-edge display where content draws under the status bar:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.DrawUnderSystemBars = true;  // Enable edge-to-edge
    options.AppHasNavigationBar = false; // REQUIRED: Must not use MAUI NavigationBar
})
```

**Requirements for Edge-to-Edge**:
- `DrawUnderSystemBars` must be `true`
- `AppHasNavigationBar` must be `false`
- Your app cannot use `NavigationPage`, `Shell` navigation, or `PagePresentationService` navigation features
- You must manually manage content padding to avoid overlap with the status bar

**What happens in edge-to-edge mode**:
- Status bar becomes transparent
- Page content draws from the top edge (under status bar)
- A listener applies padding to prevent content from being hidden under the status bar
- You can customize the top offset via `EdgeToEdgeInsetsListener` constructor

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

If you want to handle the 2 system bars manually:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseSystemStatusBarStyling = false;
    options.UseSystemNavigationBarStyling = false;
})
```

## How Overlay Mode Works when UseSmartSystemBarColoringWithModals is true

When a modal page is set to `ModalPageMode.Overlay`, the library:

1. Reads the `PageBackground` color (the underlying page color)
2. Reads the `ContentDialogBackgroundOverlay` color (semi-transparent overlay)
3. Blends these two colors to create an opaque color for the Android system bars
4. Calculates appropriate icon tint (light or dark) based on the blended color brightness
5. Applies this blended color and icon tint to the status bar and navigation bar

This ensures the system bars match the visual appearance of the overlay without requiring you to manually calculate and define additional color resources.

The icon tint calculation uses the relative luminance formula: `0.299 * R + 0.587 * G + 0.114 * B`. If the result is > 0.5, dark icons are used; otherwise, light icons are used.

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

### System Bar Styling
- Use **Standard Mode** (default) for apps with MAUI NavigationBar
- Use **Edge-to-Edge Mode** only for apps without MAUI NavigationBar that need full-screen content
- Define all required color resources to avoid `MissingResourceException`
- Test on both light and dark themes to ensure icon visibility
- Test modal pages and dialogs to ensure correct color blending

## Documentation

For more detailed information, see:
- [Page Lifecycle Management Guide](Docs/PageLifecycleManagement.md) - Comprehensive guide with examples
- [Page Removal Events](Docs/PageRemovalEvents.md) - Guide for subscribing to page removal notifications

## Troubleshooting

### MissingResourceException

If you get a `MissingResourceException`, ensure you have defined all required color resources in your `Colors.xaml` or `App.xaml`.
The exception message will tell you which resource key is missing.

### Status Bar Icons Not Visible

On Android, if status bar icons are not visible:
1. Ensure you've called `.UseMauiExtensionsServices()` in your `MauiProgram.cs`
2. Verify your color resources are defined correctly
3. Check that `UseSystemStatusBarStyling` is set to `true` (default)
4. The library automatically calculates icon tint based on background brightness - verify your background colors have sufficient contrast

### Modal Pages Not Using Correct Colors

Ensure you've set the `ModalPageMode` on your modal pages if creating custom overlay modals:

```csharp
ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
```

**Note**: `ContentDialog` and `AlertDialog` set this automatically.

### Edge-to-Edge Not Working

If edge-to-edge mode isn't activating:
1. Verify both `DrawUnderSystemBars = true` AND `AppHasNavigationBar = false`
2. Ensure you're not using `NavigationPage`, `Shell`, or navigation features of `PagePresentationService`
3. Check that you're not calling any navigation methods that create a NavigationBar

### OnNavigatedTo/OnNavigatedFrom Not Called

- Ensure your ViewModel implements `IPageLifecycleAware`
- Verify the ViewModel is set as `BindingContext` before the page is pushed
- Check that the page is created through `PagePresentationService`

### Dispose Not Called

- Ensure ViewModel implements `IAutoDisposableOnViewClosed`
- Verify the page is actually being removed (not just hidden)
- Check that `PagePresentationService` is managing the navigation

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

Copyright (c) 2025 [Jos Huybrighs]

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
