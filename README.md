# cw.MauiExtensions.Services

A reusable .NET MAUI library providing page presentation services (standalone pages, pages with navigation stack, popups, alert dialogs),
ViewModel lifecycle management, and customizable system bar (status bar and navigation bar) handling for Android, Windows.
### Features

This library provides:
- **PagePresentationService**: Can create any type of start page, push and pop non-modal pages, and modal pages
- **ViewModel Lifecycle Management**: Automatic lifecycle event notifications and disposal
- **ContentDialog**: Overlay-style modal dialogs (popups) with semi-transparent backgrounds as the base for your own custom dialogs
- **AlertDialog**: A standard alert/confirmation dialog with title, message, and buttons
- **Smart System Bar Handling**: Automatically configures status and navigation bar colors
- **Page Removal Events**: Subscribe to page removal notifications via events (or `WeakReferenceMessenger` via a centralized listener in app.cs) 

---
# The PagePresentationService

The purpose of the `PagePresentationService` is to be a replacement for AppShell and get rid of some questionable features of `AppShell`, in particular:
- URL-based routing which uses web-style navigation where apps are typically state-driven.
- Shell hides too much behavior behind implicit magic: page instantiation, parameter injection, ..
- Dependency Injection is awkward and fragile with Shell.
- Shell enforces UI structure too early.
- Shell doesn't scale well.

`PagePresentationService` therefore offers the following 6 methods:

### Method 1: `OpenMainPage(Type viewType, object? viewModel)`

Creates and returns a new main page instance of the specified type, optionally initialized with the provided
view model. The page is not created in a NavigationPage and so doesn't support the MAUI navigation stack.
The page being instantiated is expected to have a constructor with a viewModel object as parameter when the method is invoked with
a non-null viewModel. The page then typically binds the viewmodel parameter to its BindingContext.
If the page creates the viewModel itself, then the page constructor must be parameterless and the viewModel parameter in the method
call must be set to null.

Example:
```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    var page = PagePresentationService.Instance.OpenMainPage(typeof(Views.DemoTabbedPage), null);
    return new Window(page);
}
```

The method is typically invoked at startup of the app in `App.CreateWindow` which then creates a new Window(page).
If the method is called later at a moment where `Application.Current.Windows` already has a page assigned, then the assigned page
will be replace by the newly created page.

### Method 2: `OpenMainNavigationPage(Type viewType, object? viewModel)`

Creates and returns a new MAUI `NavigationPage`, or replaces a already assigned one, and assigns the page defined by the specified
view type as root of the navigation stack.
The page being instantiated is expected to have a constructor with a viewModel object as parameter when the method is invoked with
a non-null viewModel. The page then typically binds the viewmodel parameter to its BindingContext.
If the page creates the viewModel itself, then the page constructor must be parameterless and the viewModel parameter in the method
call must be set to null.

Example:
```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    var page = PagePresentationService.Instance.OpenMainNavigationPage(typeof(Views.HomePage), new ViewModels.HomeViewModel());
    return new Window(page);
}
```

Also here, the method is usually called at startup of the app in App.CreateWindow which then creates a new Window(page).
If the method is called later at a moment where Application.Current.Windows already has a page assigned, then the assigned page
will be replace by the newly created NavigationPage.


### Method 3: `PushPageAsync(Type viewType, object? viewModel, int pagesToPopCount = 0)`

Opens a new page of the specified type by pushing the page on the MAUI navigation stack. 
The page being instantiated is expected to have a constructor with a viewModel object as parameter when the method is invoked with
a non-null viewModel. The page then typically binds the viewmodel parameter to its BindingContext. If the page creates the viewModel
itself, then the page constructor must be parameterless and the viewModel parameter in the method call must be set to null.

Example:
```csharp
public partial class HomeViewModel : ObservableObject, IPageLifecycleAware
{
    [RelayCommand]
    async Task OpenNonModalPage()
    {
        // Navigate to a non-modal page
        await PagePresentationService.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel(pageNumber: 1));
    }
    ...
}
```

### Method 4: `PopPageAsync(int nrofPagesToPop = 1)`

Removes one or more pages from the top of the navigation stack.

Example:
```csharp
[RelayCommand]
async Task Save()
{
    // Inform listeners
    WeakReferenceMessenger.Default.Send(new LocationConfiguredMessage(new LocationInfoEvent(_isLocA, StorageLoc)));

    await PagePresentationService.Instance.PopPageAsync();
}
```


### Method 5: `OpenModalPageAsync(Page modalPage)`

Displays the specified page as a modal dialog on top of the current page. The modal page can be
instructed to open in FullScreen or Overlay mode.
**Note**: The library also comes with a `ContentDialog` popup page class from which you can derive your own popup. It
expects a `ContentBorder*` style to be available in your app to style the view border in which your content resides.

Example - Full screen modal:

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:cw.MauiExtensions.Services.Demo.ViewModels"
             x:DataType="vm:ModalViewModel"
             x:Class="MauiExtensions.Demo.Views.ModalPage"
             Title="Modal page"
             Background="{AppThemeBinding Light={StaticResource PageBackground}, Dark={StaticResource PageBackgroundDark}}">
    <VerticalStackLayout Padding="18,48,18,0"
                         Spacing="25">
    <Button Text="X"
            Command="{Binding CloseModalCommand}"/>
    <Label Text="This is a modal page. Tap X to close."
           FontSize="Medium" />
  </VerticalStackLayout>
</ContentPage>
```

```csharp
// Full-screen modal page - no need to set mode, FullScreen is default
public partial class ModalPage : ContentPage
{
    public ModalPage()
    {
        InitializeComponent();
        
        // ModalPageMode.FullScreen is the default - no action needed
    }
}

[RelayCommand]
async Task OpenModalPage()
{
    // Navigate to modal page on the stack
    await PagePresentationService.Instance.OpenModalPageAsync(new ModalPage(new ModalViewModel()));
}
```

Example - Overlay modal:

```csharp
public partial class MyPopupDialog : ContentPage
{
    public MyPopupDialog()
    {
        InitializeComponent();
        
        // Only needed for custom overlay dialogs
        ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
    }
}

[RelayCommand]
async Task OpenPopupDialog()
{
    // Navigate to modal page on the stack
    await PagePresentationService.Instance.OpenModalPageAsync(new MyPopupDialog());
}
```

**Note**: Overlay mode can be used for instance to popup a message on the screen. If the popup however serves
as a means for the user to perform a task (like entering a password, picking a date, ..) you will have to
open the popup and provide some mechanism (typically a `TaskCompletionSource`) to wait for the user closing the
page and returning the result.</br>
The library therefore comes with a `ContentDialog` class that provides all of this.

### Method 6: `CloseModalPageAsync()`

Closes the topmost modal page if one is present on the application's main window.

Example:

```csharp
[RelayCommand]
async Task CloseModal()
{
    // Close the modal page
    await PagePresentationService.Instance.CloseModalPageAsync();
}
```

---
# AlertDialog

The library comes with a `AlertDialog` class that allows you to easily show alert popups that only need a title, a description and 1 or 2
buttons. The constructor is: </br>
`public AlertDialog(string title, string text, string primaryBttnText, string? secondaryBttnText)`
- Parameter title is a string that is presented at the top of the popup
- Parameter text defines the text that shows below the title.
- Parameter primaryBttnText defines the text of the 1st button in the dialog. You can assign any meaning to it.
- Parameter secondaryBttnText defines the text of an optional 2nd button in the dialog.

The popup is opened by invoking the ShowAsync() method on the created AlertDialog. The method returns a `ContentDialogResult` which is
an enum value that can be: None, Primary, or Secondary. No,e is returned when the dialog is closed by tapping outside of the popup.

Example:
```csharp
[RelayCommand]
async Task ShowAlert()
{
    var alertDialog = new AlertDialog("Alert", "This is an alert dialog", "OK", "Cancel");
    var result = await alertDialog.ShowAsync();
    if (result == ContentDialogResult.Primary)
    {
        // User clicked OK
    }
    else
    {
        // User clicked Cancel or closed the dialog
    }
}
```

You can style 2 things in the `AlertDialog`:
- The style of the content border. Configure `ResourceKeys.AlertDialogBorderStyle` for this. 
- The style of the 2 buttons. Configure `ResourceKeys.AlertDialogButtonStyle` for this. 

---
# Easy popups using `ContentDialog`

Example:
```csharp
using cw.MauiExtensions.Services.Views;

var dialog = new ContentDialog
{
    ContentView = new MyCustomView()
};

var result = await dialog.ShowAsync();
// Handle result: ContentDialogResult.None, Primary, or Secondary
```

See the [ContentDialog](src/cw.MauiExtensions.Services/docs/ContentDialog.md) documentation for more details.

---
# System Bars coloring

The latest UI recommendations for mobile apps call for a UI where all page background colors extend to the system bars, i.e the device's
status bar and system navigation bar (when avalable). Out of the box MAUI does not provide this functionality, so this library implements it for you.

## Possible display modes

This library supports a number of ways to color the system bars:

### Pages using a navigation bar

Real edge to edge display is not possible when a page is using a navigation bar, because the navigation bar itself occupies space at the top of the screen.
You can however ensure consistent background colors across all system bars and pages in your app by assigning the same background color to all relevant elements.
To achieve this the library provides configuration options to set the SystemBarsBackgroundColor, PageBackgroundColor, and NavigationBarBackgroundColor resource keys
for both light and dark mode. The default resource keys are: "SystemBarsBackground" and "SystemBarsBackgroundDark", "PageBackground" and "PageBackgroundDark",
"NavigationBarBackground" and "NavigationBarBackgroundDark".
For all of your pages make then also sure to set the page background color to the same PageBackgroundColor resource key. Like in:

```csharp
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="cw.MauiExtensions.Services.Demo.Views.NonModalPage"
             Title="NonModalPage"
             Background="{AppThemeBinding Light={StaticResource PageBackground}, Dark={StaticResource PageBackgroundDark}}">
    <VerticalStackLayout Padding="18">
        <Label Text="This is a non-modal page on the navigation stack."
               VerticalOptions="Center" />
    </VerticalStackLayout>
</ContentPage>
```

Of course there is still the possibility to assign different colors for the system bars, navigation bar and page backgrounds.


### Pages not using a navigation bar

When a page does not use a navigation bar (easily accomplished by using PagePresentationService.OpenMainPage()) the
tint of the system bar icons is automatically calculated based on the brightness of the SystemBarsBackgroundColor.
- When `DrawUnderSystemBars` is set to false (the default) the library will explicitly set the background of the bars to SystemBarsBackgroundColor. 
- When `DrawUnderSystemBars` is set to true the library will extend the page's background color to the system bar's area.


### Modal Pages (full screen and overlay)

Coloring of the bars with modal pages is done as follows:

- When `DrawUnderSystemBars` is disabled (the default) the color of the system bar depends on whether the page is showing in full screen or overlay mode.
  - In overlay mode the library will blend the page's background color, i.e `ResourceKeys.PageBackgroundColor` or
`ResourceKeys.PageBackgroundDarkColor`, with the color of `ResourceKeys.ContentDialogBackgroundOverlayColor` or
 `ResourceKeys.ContentDialogBackgroundOverlayDarkColor` and use the resulting color for the system bars.
  - In full screen mode the library will use the color of `ResourceKeys.SystemBarsBackgroundColor` or
`ResourceKeys.SystemBarsBackgroundDarkColor` for the system bars.

- When `DrawUnderSystemBars` is enabled the library will extend the page's background color to the system bar's area.
If the page's background color is opaque so will be the bar color; if the color is transparant so will be the bar background color.
The tint of the system bar icons is automatically calculated based on the brightness of `ResourceKeys.SystemBarsBackgroundColor` or
`ResourceKeys.SystemBarsBackgroundDarkColor`.

The tint of the system bar icons is not explicitly set. It is assumed that the OS takes care of that.

## Configuration options for coloring system bars

```csharp
.UseMauiExtensionsServices(options =>
{
    options.DrawUnderSystemBars = true; // Enable edge-to-edge
    options.AppHasNavigationBar = true; // Library uses configured resource keys for system bar colors
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
})
```


## Things to know:

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

---
# ViewModel Lifecycle Management

When using pages and popups, it is important to manage the lifecycle of the associated ViewModels properly. MAUI doesn't provide
built-in support for ViewModel lifecycle events, which can lead to memory leaks and unexpected behavior.

cw.MauiExtensions.Services provides automatic ViewModel lifecycle management through two opt-in interfaces:

## IPageLifecycleAware

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
- Track page views for analytics
- Save state when navigating away

## IAutoDisposableOnViewClosed

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
- Unsubscribe from event publishers
- Dispose timers and HTTP clients
- Cancel ongoing async operations
- Release unmanaged resources
- Prevent memory leaks


## Lifecycle Event Flow

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

---
# Page Removal Notifications

The library alse provides a PageRemoved event in PagePresentationService to get a notification when a page is removed. It is triggered in
the following scenarios:
- A page is popped from the navigation stack.
- A modal page is closed.

Note: before the event is raised and when the page has a IAutoDisposableOnPageClosed viewmodel assigned to its BindingContext, the viewmodel's
Dispose method (if implemented) will be called.

## WeakReferenceMessenger
Any object in your app can subscribe to the PageRemoved event to get notified when pages are removed.
If you prefer however a more loosely coupled approach, you can of course also use the CommunityToolkit.Mvvm WeakReferenceMessenger
to broadcast the event to interested objects and/or services.
Do this by subscribing to the PageRemoved event in a centralized location (e.g. App.xaml.cs) and broadcasting the event via the messenger.
Like so:

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

An object; e.g. a ViewModel can then subscribe to these events like so:

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

---
# Installation

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

3. Make sure that the following resources are defined:

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

### Additional required Colors in the ContentDialogBorder style when AlertDialog is used

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

That's it! you can now use the PagePresentationService and automatically manage ViewModel lifecycles for all pages.

---
# ViewModel Lifecycle Examples

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

---
# Configuration

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
                
                // Enable/disable smart system bar coloring for modals, popups
                options.UseSmartSystemBarColoringWithModals = true; // Default: true
                
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

The icon tint calculation uses the relative luminance formula: `0.299 * R + 0.587 * G + 0.114 * B`. If the result is > 0.5, dark icons are used; otherwise, light icons are used.

---
# Best Practices

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

---
# Troubleshooting

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

---
# License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 [Jos Huybrighs]

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
