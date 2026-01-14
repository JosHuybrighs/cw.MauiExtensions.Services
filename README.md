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
- **Page Removal Events**: Subscribe to page removal notifications via events 

---
# The PagePresentationService

The purpose of the `PagePresentationService` is to be a replacement for AppShell and get rid of some questionable features of `AppShell`, in particular:
- URL-based routing which uses web-style navigation where apps are typically state-driven.
- Shell hides too much behavior behind implicit magic: page instantiation, parameter injection, ..
- Dependency Injection is awkward and fragile with Shell.
- Shell enforces UI structure too early.
- Shell doesn't scale well.

`PagePresentationService` offers the following methods for opening pages:

### Method 1: `OpenMainPage(Type viewType, object? viewModel)`

Creates and returns a new main page instance of the specified type, optionally initialized with the provided
view model. With this method the page is not created in a `NavigationPage` and so doesn't support the MAUI navigation stack.
- When the method is invoked with a non-null viewModel then the page being instantiated is expected to have
a constructor with a viewModel object as parameter. The page then typically binds the viewmodel parameter to
its BindingContext.
- When viewModel is null then the page constructor must be parameterless. You can then choose to work with or
without a view model in the page code-behind.

Example:
```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    var page = PagePresentationService.Instance.OpenMainPage(typeof(Views.DemoTabbedPage), null);
    return new Window(page);
}
```

The method is typically invoked at startup of the app in `App.CreateWindow` which then creates a new Window where
the page will be the "canvas" with the UI.
If the method is called later at a moment where `Application.Current.Windows` already has a page assigned, then the assigned page
will be replace by the newly created page.

### Method 2: `OpenMainNavigationPage(Type viewType, object? viewModel)`

Creates and returns a new MAUI `NavigationPage`, or replaces a already assigned one, and assigns the page defined by the specified
view type as root of the navigation stack.
- When the method is invoked with a non-null viewModel then the page being instantiated is expected to have
a constructor with a viewModel object as parameter. The page then typically binds the viewmodel parameter to
its BindingContext.
- When viewModel is null then the page constructor must be parameterless. You can then choose to work with or
without a view model in the page code-behind.

Use this method if you want to create an app that uses a navigation stack for navigating between pages.


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

Opens a new page of the specified type by pushing the page on the MAUI navigation stack. This method requires
a `NavigationPage` to be already assigned to the current window. See method 2.
- When the method is invoked with a non-null viewModel then the page being instantiated is expected to have
a constructor with a viewModel object as parameter. The page then typically binds the viewmodel parameter to
its BindingContext.
- When viewModel is null then the page constructor must be parameterless. You can then choose to work with or
without a view model in the page code-behind.

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

Displays the specified page as a modal dialog on top of the current page. The method can be called from any
open page and is put on a modal page's stack supporting multiple modal pages.</br>
The modal page can be instructed to open in FullScreen or Overlay mode.
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

Closes the topmost modal page if one is present on the application's main window. The system will bring back
a possible previous modal page or the underlying non-modal page.

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

The library comes with a `AlertDialog` class for simple alert/confirmation scenarios where the popup
only needs a title, a description and 1 or 2 buttons.</br>
The constructor is:
`public AlertDialog(string title, string text, string primaryBttnText, string? secondaryBttnText)`
- Parameter title is a string that is presented at the top of the popup
- Parameter text defines the text that shows below the title.
- Parameter primaryBttnText defines the text of the 1st button in the dialog. You can assign any meaning to it.
- Parameter secondaryBttnText defines the text of an optional 2nd button in the dialog.

The popup is opened by invoking the `ShowAsync()` method on the created `AlertDialog`. The method returns a
`ContentDialogResult` which is an enum value that can be: None, Primary, or Secondary. None is returned when
the dialog is closed by tapping outside of the popup.

Example:
```csharp
[RelayCommand]
async Task ShowAlert()
{
    var alert = new AlertDialog(title: "Confirm Delete",
                                text: "Are you sure you want to delete this item? This action cannot be undone.",
                                primaryBttnText: "Delete",
                                secondaryBttnText: "Cancel");

    var result = await alert.ShowAsync();

    if (result == ContentDialogResult.Primary)
    {
        await DeleteItemAsync();
    }
)
```

You can style 2 things in the `AlertDialog`:
- The style of the content border. Configure `ResourceKeys.AlertDialogBorderStyle` for this. 
- The style of the 2 buttons. Configure `ResourceKeys.AlertDialogButtonStyle` for this. 

---
# Easy popups using `ContentDialog`

The library comes with a `ContentDialog` class that you can use as a base class for your own custom popup dialogs.
It supports: type-safe results, full XAML support, automatic overlay styling, and lifecycle management.

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

See the [ContentDialog](src/cw.MauiExtensions.Services/docs/ContentDialog.md) documentation for a detailed description.

---
# System bars and navigation bar coloring on Android

The latest UI recommendations for mobile apps call for a UI where all page background colors extend to the device's status
bar, the maui navigation bar (when available) and the system navigation bar (when avalable).</br>
Out of the box MAUI does not provide this functionality on all platforms. Especially on Android support for
configuring the system bars across all Android versions is limited.

On Android, the library deals with this in 2 phases of the app lifecycle:
1. When the app starts up or resumes from sleep the library configures the system bars once. The system
bars stay as set when a pages replaces the current or is pushed on the stack.
2. When a modal page is opened the library will always configure the system bars because modal pages use a
different dialog fragment.

## 1. App starts up or resumes from sleep

The way how the library sets the color and tint of the system bars depends on whether the app uses a navigation stack or not.

### 1.1 App opens a page on the MAUI navigation stack
Pre-condition: `AppHasNavigationBar = true` (the default) and `UseSmartSystemBarColoring = true` (the default).</br>
Note: if `UseSmartSystemBarColoring` is set to false the library will not set any colors on the system bars.

**Edge to edge** - Real edge to edge display is not possible on Android when a page is the root page of the navigation stack
or is pushed on the stack. The reason for this is that there is a navigation bar which occupies space at the top of the screen.
The option `EnableEdgeToEdge` is therefore ignored. You can however mimic edge to edge by assigning the same background color
to all relevant elements. Do that on the following 2 places:
- When the option `UseSmartSystemBarColoring` is set to true (the default) the library will color the Android system bars
with the color referenced by `SystemBarsBackgroundColor` (light mode) and `SystemBarsBackgroundDarkColor` (dark mode). When the
option is set to false then the library will not set the background color.
- When you create/open the main page using `PagePresentationService.OpenMainNavigationPage` then both the background color and the
text color of the MAUI navigation bar is set to the colors referenced by `NavigationBarBackgroundColor` or `NavigationBarBackgroundDarkColor`
and `NavigationBarTextColor` or `NavigationBarTextDarkColor`.

You must specify resource keys (i.e. in `Colors.xaml`) for all of the above color definitions:
The default expected resource keys, which you can change if you want, are:
- "SystemBarsBackground" and "SystemBarsBackgroundDark",
- "NavigationBarBackground" and "NavigationBarBackgroundDark".
- "NavigationBarText" and "NavigationBarTextDark".

For all of your pages make then also sure to set the page background color to the same color. It is advised to define for this
the resource keys "PageBackground" and "PageBackgroundDark" because those are also expected to be defined when you open modal pages.

A typical page definition would look like this:

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

**Different colors** - Of course there is still the possibility to assign different colors for the system bars, navigation bar and page
backgrounds.

### 1.2 App opens a page without using the MAUI navigation stack
Set `AppHasNavigationBar`to false.

At startup or resume of the app the library will color the Android system bars depending on the setting of `EnableEdgeToEdge`.
- When `EnableEdgeToEdge` is set to true (the default) the library will extend the page's background color to the system bar's area
and set the tint of the system bar icons based on the brightness of the color referenced by `SystemBarsBackgroundColor` or
`SystemBarsBackgroundDarkColor`.
- When `EnableEdgeToEdge` is set to false the library will explicitly create a overlay color on the status bar and set it to the
color referenced by `SystemBarsBackgroundColor` or `SystemBarsBackgroundDarkColor`. The tint of the system bar icons is
calculated based on the brightness of overlay color.
 
## 2. Modal Pages (full screen and overlay)

Pre-condition: `UseSmartSystemBarColoringWithModals` is set to true (the default).</br>
Note: if `UseSmartSystemBarColoringWithModals` is set to false the library will not configure the system bars when a modal page
is opened. The MAUI framework will then take care of it which will be OK for Android API 35+ but not as expected for lower versions.

- When `EnableEdgeToEdge` is set to true (the default) the library will extend the page's background color to the system bar's area.
If the page's background color is opaque so will be the bar color; if the color is transparant so will be the bar background color.</br>
The tint of the system bar icons is defined by the setting of `UseDarkSystemBarIconsWithModalPages` or
`UseDarkSystemBarIconsWithModalPagesDark`.

- When `EnableEdgeToEdge` is disabled the color of the system bar depends on whether the page is showing in full screen or overlay mode.
  - In overlay mode the library will blend the color configured in `ResourceKeys.PageBackgroundColor` or
`ResourceKeys.PageBackgroundDarkColor`, with the color of `ResourceKeys.ContentDialogBackgroundOverlayColor` or
 `ResourceKeys.ContentDialogBackgroundOverlayDarkColor` and use the resulting color for the system bars.
  - In full screen mode the library will use the color of `ResourceKeys.SystemBarsBackgroundColor` or
`ResourceKeys.SystemBarsBackgroundDarkColor` for the system bars.
  - The tint of the system bar icons is not explicitly set. It is assumed that the OS takes care of that.


# System bars and navigation bar coloring on iOS

## 1. Non-modal pages

### 1.1 App uses the MAUI navigation stack
On iOS the MAUI navigation bar is drawn as an overlay on top of the system bar with a vertical offset, so true edge-to-edge display
is possible by assigning the same background color to the navigation bar and page background.</br>
A page definition in xaml is typically the same as shown above for Android.

### 1.1 App doesn't uses the MAUI navigation stack
On iOS 13+ the color of the status bar is transparant and the background color of the page extends to the bar. Depending on the
value of `IgnoreSafeArea` (typically set as a property of the root container of a page) the page content starts below or at the
top of the bar. So it is possible to achieve true edge-to-edge display.

## 2. Modal pages (full screen and overlay)
On iOS the background color configured for the modal page automatically extends to the system bars. If the background
color is opaque then so will be the color of the system bars; if the color is transparant then the system bar will
also become dimmed.


# System bars and navigation bar coloring on Windows

## 1. Non-modal pages

### 1.1 App uses the MAUI navigation stack

On Windows the MAUI navigation bar is drawn below the title bar, so true edge-to-edge display is not possible. However,
you can achieve a similar effect by adding a TitleBar control in the window of your app and styling the control to match the
background color with the navigation bar and page background.</br>
It is done as follows:

Create the TitleBar control. Example:

The TitleBar control is ignored when the app is running on Android or iOS.

### 1.2 App doesn't uses the MAUI navigation stack

still todo...

## 2. Modal pages (full screen and overlay)
On Windows the modal page will allways start below the title bar. If the background color is semi-transparant then the
previous page will shine through be will be dimmed. The title stays untouched.


# Configuration options for coloring system bars

```csharp
.UseMauiExtensionsServices(options =>
{
    options.UseSmartSystemBarColoringWithModals = true;
    options.EnableEdgeToEdge = true;
    options.AppHasNavigationBar = true;
    options.UseDarkSystemBarIconsWithModalPages = true;
    options.UseDarkSystemBarIconsWithModalPagesDark = false;
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


# Things to know in case of Android:

1. **Status Bar Color**: The background color of the Android Status Bar can be configured via resource keys. The tint of the icons
is automatically calculated based on the background color brightness (dark icons for light backgrounds, light icons for dark backgrounds).

2. **Navigation Bar Color** (Android bottom bar):
   - API < 35: Color can be controlled via resource keys
   - API 35+: Color automatically matches the page background

3. **MAUI NavigationBar**: Has its own background color (configurable via resource keys).

4. **Modal Pages and Dialogs**: The library includes a `DialogFragmentService` that correctly handles system bar colors for:
   - Modal pages (full-screen and overlay modes)
   - Popup dialogs
   - Works correctly across all Android API levels (26+)

5. **CommunityToolkit.Maui Comparison**: While CommunityToolkit.Maui offers StatusBarBehavior, it doesn't correctly handle modal
pages and popups across all Android versions. This library's `DialogFragmentService` provides proper support for all scenarios.

6. **Modal Behavior by API Level**:
   - API 35+: System bars automatically match page background; status bar icons tint is managed by the library.
   - API < 35: System bars maintain configured colors; `DialogFragmentService` ensures consistency.

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

The library alse provides a `PageRemoved` event in `PagePresentationService` to get a notification when a page is removed. It is triggered in
the following scenarios:
- A page is popped from the navigation stack.
- A modal page is closed.

Note: before the event is raised and when the page has a `IAutoDisposableOnPageClosed` viewmodel assigned to its BindingContext, the viewmodel's
`Dispose` method (if implemented) will be called.

## WeakReferenceMessenger
Any object in your app can subscribe to the `PageRemoved` event to get notified when pages are removed.
If you prefer however a more loosely coupled approach, you can of course also use the CommunityToolkit.Mvvm `WeakReferenceMessenger`
to broadcast the event to interested objects and/or services.
Do this by subscribing to the `PageRemoved` event in a centralized location (e.g. App.xaml.cs) and broadcasting the event via the messenger.</br>
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

**Note**: When using Edge-to-Edge mode (`EnableEdgeToEdge = true`), these colors still affect modal dialogs and popups, but the
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
                // Enable/disable smart system bar coloring for modals, popups
                options.UseSmartSystemBarColoringWithModals = true; // Default: true
                options.UseDarkSystemBarIconsWithModalPages = true; // Default: true
                options.UseDarkSystemBarIconsWithModalPagesDark = false; // Default: false
                
                // Configure edge-to-edge display
                options.EnableEdgeToEdge = true; // Default: true
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

## Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ResourceKeys.*` | `string` | Various | Customize the resource key names the library looks for |
| `UseSmartSystemBarColoring` | `bool` | `true` | Enable/disable smart system bar coloring at app startup and resume 0|
| `UseSmartSystemBarColoringWithModals` | `bool` | `true` | Enable/disable Dialog Fragment Service for modal pages and popups |
| `EnableEdgeToEdge` | `bool` | `false` | Enable edge-to-edge display (content draws under status bar) |
| `AppHasNavigationBar` | `bool` | `true` | Set to false if your app does NOT use MAUI NavigationBar |



## Custom Resource Keys

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


---
# Troubleshooting

### MissingResourceException

If you get a `MissingResourceException`, ensure you have defined all required color resources in your `Colors.xaml` or `App.xaml`.
The exception message will tell you which resource key is missing.

### Status Bar Icons Not Visible

On Android, if status bar icons are not visible:
1. Ensure you've called `.UseMauiExtensionsServices()` in your `MauiProgram.cs`
2. Verify your color resources are defined correctly
4. The library automatically calculates icon tint based on background brightness - verify your background colors have sufficient contrast

### Modal Pages Not Using Correct Colors

Ensure you've set the `ModalPageMode` on your modal pages if creating custom overlay modals:

```csharp
ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
```

**Note**: `ContentDialog` and `AlertDialog` set this automatically.

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
