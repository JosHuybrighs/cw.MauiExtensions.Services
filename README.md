# cw.MauiExtensions.Services

A reusable .NET MAUI library providing page presentation services (standalone pages, pages with navigation stack, popups, alert dialogs),
ViewModel lifecycle management, and customizable system bar (status bar and navigation bar) handling for Android, Windows.
### Features

This library offers:
- **ViewPresenter**: You can create any type of page: a start page with or without a navigation stack, a page to be
pushed on the stack, a page replacing the start page, and modal pages/popups. 
- **ViewModel Lifecycle Management**: Hook your viewmodels to lifecycle events and disposal requests.
- **ContentDialog**: A overlay-style modal dialog (popup) with semi-transparent backgrounds as the base for your own custom dialogs
- **AlertDialog**: A simple alert/confirmation dialog with only a title, a message, and up to 2 buttons.
- **Smart System Bar Handling**: Automatically configure status and navigation bar colors accross all Android API versions.
- **Page Removal Events**: Subscribe to page removal notifications wherever you want. 

---
# The ViewPresenter

The purpose of the `ViewPresenter` is to be a replacement for `AppShell` and get rid of some questionable features of `AppShell`,
in particular:
- URL-based routing which uses web-style navigation where apps are typically state-driven.
- Shell hides too much behavior behind implicit magic: page instantiation, parameter injection, ..
- Dependency Injection is awkward and fragile with Shell.
- Shell enforces UI structure too early.
- Shell doesn't scale well.

You can open and close pages by using the following methods of the `ViewPresenter` singleton instance:

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
    var page = ViewPresenter.Instance.OpenMainPage(typeof(Views.DemoTabbedPage), null);
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
    var page = ViewPresenter.Instance.OpenMainNavigationPage(typeof(Views.HomePage), new ViewModels.HomeViewModel());
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
        await ViewPresenter.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel(pageNumber: 1));
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

    await ViewPresenter.Instance.PopPageAsync();
}
```


### Method 5: `OpenModalPageAsync(Page modalPage)`

Displays the specified page as a modal dialog on top of the current page. The method can be called from any
open page and is put on the MAUI modal pages stack which supports multiple modal pages.</br>
The modal page can be instructed to open in FullScreen or Overlay mode.</br>

**Example - Full screen modal**

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
    await ViewPresenter.Instance.OpenModalPageAsync(new ModalPage(new ModalViewModel()));
}
```

**Example - Overlay modal / popup**

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
    await ViewPresenter.Instance.OpenModalPageAsync(new MyPopupDialog());
}
```
**Remark** - See also `ContentDialog` for popups returning a result.

### Method 6: `CloseModalPageAsync()`

Closes the topmost modal page if one is present on the application's main window. The system will bring back
a possible previous modal page or the underlying non-modal page.

Example:

```csharp
[RelayCommand]
async Task CloseModal()
{
    // Close the modal page
    await ViewPresenter.Instance.CloseModalPageAsync();
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
# `ContentDialog` for easy popup dialogs

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
# System bars and navigation bar coloring

The latest UI recommendations for mobile apps call for a UI where all page background colors extend to the device's status
bar, the maui navigation bar (when available) and the system navigation bar (when available).</br>
Out of the box MAUI does not provide this functionality on all platforms. Especially on Android support for
configuring the system bars across all Android versions is limited.</br>

How does the library help you with this?

## 1. iOS

On iOS 13+ the color of the status bar is transparant. This has the following advantages:
- On a page with a MAUI navigation stack the MAUI navigation bar is drawn as an overlay on top of the system bar with a 
vertical offset, so true edge-to-edge display is possible by assigning the same background color to the navigation bar and page
background.
- On a page that doesn't use the MAUI navigation stack and on modal pages (full screen or overlay) the background color of
the page extends to the bar. Depending on the value of `IgnoreSafeArea` (typically set as a property of the root container
of a page) the page content starts below or at the top of the bar. So it is possible to achieve true edge-to-edge display.

**Summary** - With iOS the library doesn't need to do anything special to achieve edge-to-edge display.


## 2. Windows

On Windows all pages start below the title bar and therefore true edge-to-edge display is not possible unless you create a
`TitleBar` control for the window and style it to match the background color with the page background and, if available,
assign the same color to the MAUI navigation bar.</br>

The `TitleBar` is created as follows:


The `TitleBar` control is ignored when the app is running on Android or iOS.

**Remark** - Modal pages (full screen and overlay) also start below the title bar. When a page has a semi-transparant
background color then the previous page will shine through and typically will be dimmed. The title bar however will stay
untouched.

**Summary** - With Windows the library doesn't need to do anything special to achieve edge-to-edge display, provided you
configure a custom `TitleBar`.


## 3. Android

To achieve a true edge-to-edge layout on Android the library comes with a number of configuration options. The 2 most
important are: `UseSmartSystemBarColoring` and `UseSmartSystemBarColoringWithModals`.  Both are true by default. If you
set them to false then the library will not do anything special and the system bars (status bar at the top and navigation
bar at the bottom) will stay as configured by the OS or by other libraries such as CommunityToolkit.Maui.

The next thing to consider is whether you want edge-to-edge for root pages and/or for modal pages.

### 3.1 Root pages

`EdgeToEdgeForRootPages` - This option indicates whether at the start of the app (and when the theme changes) the
library must extend the page content under the system bars or not.</br>
- If **true** (the default) the library will configure the system not to adjust the view to fit inside the "safe areas" and remove the
behavior to draw a solid color behind the bars. This works well for all type of pages except for a TabbedPage when the tab buttons are
at the bottom.</br>
Per default there will be a offset for the page content in order to start below the status bar. If you don't want that you must set
the option `EdgeToEdgeStartContentBelowBar` to false.
- If **false** the library will explicitly color the systems bars with the color referenced by
`SystemBarsBackgroundColor` (light mode) and `SystemBarsBackgroundDarkColor` (dark mode). No change is done on the insets
and therefore the page starts below the status bar.</br>
This is what you must use when the root page is a TabbedPage with the tab buttons at the bottom.

In both cases the library also sets the tint of the icons of the system bars to light or dark by looking at the brightness
of `SystemBarsBackgroundColor` or `SystemBarsBackgroundDarkColor`.

#### Required resource settings for any type of page

The library retrieves the color of the system bar from `Application.Current.Resources`, i.e. from `Colors.xaml`. The resource keys
can be changed in `SystemBarsBackgroundColor` and `SystemBarsBackgroundDarkColor` but if you want to use the default keys you
need a definition for:
- "SystemBarsBackground" and
- "SystemBarsBackgroundDark"

#### Required resource settings for a MAUI NavigationPage

When the root page is a MAUI NavigationPage and you want to have a edge-to-edge background color then you must also configure the
navigation bar and page background colors to match the system bar colors. This can be easily done by opening the
main page using `ViewPresenter.OpenMainNavigationPage`. This method automatically sets both the background color and the
text color of the MAUI navigation bar to the color referenced by `NavigationBarBackgroundColor` or `NavigationBarBackgroundDarkColor`
and `NavigationBarTextColor` or `NavigationBarTextDarkColor`.

If you are OK with with the default expected resource keys then you must configure the following keys in `Colors.xaml`:
- "SystemBarsBackground" and "SystemBarsBackgroundDark"
- "NavigationBarBackground" and "NavigationBarBackgroundDark"
- "NavigationBarText" and "NavigationBarTextDark"

### 3.2 Modal and Popup pages

`EdgeToEdgeForModalPages` - This option indicates whether the library must extend the modal page content under the
system bars or not.</br>
- If **true** (the default) the library will configure the page to extend beneath the system bars. This is the best option for
a edge-to-edge layout with a single background color on the whole screen.</br>
Per default there will be a offset for the page content in order to start below the status bar. If you don't want that you must set
the option `EdgeToEdgeStartContentBelowBar` to false.

- If **false** the library will explicitly color the systems bars with the color referenced by
`SystemBarsBackgroundColor` (light mode) and `SystemBarsBackgroundDarkColor` (dark mode). No change is done on the insets
and therefore the modal page starts below the status bar.</br>

In both cases the tint of the icons of the system bars is set to light or dark depending on the setting of the configuration
option `UseDarkSystemBarIconsWithModalPages` or `UseDarkSystemBarIconsWithModalPagesDark`.

#### Required resource settings for modal pages
You must provide a key definition in Colors.xaml for:
- "SystemBarsBackground" and
- "SystemBarsBackgroundDark"

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

## IDisposableOnViewClosed

Automatically dispose resources when the page is removed from navigation:

```csharp
using cw.MauiExtensions.Services.Interfaces;

public class MyViewModel : ObservableObject, 
                           IPageLifecycleAware,
                           IDisposableOnViewClosed
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
ViewPresenter hooks lifecycle events
    ↓
Page.Appearing → IPageLifecycleAware.OnNavigatedTo()
    ↓
[User interacts with page]
    ↓
Page.Disappearing → IPageLifecycleAware.OnNavigatedFrom()
    ↓
Page Removed/Popped
    ↓
Unhook events → IDisposableOnViewClosed.Dispose()
    ↓
PageRemoved event raised
```

---
# Page Removal Notifications

The library alse provides a `PageRemoved` event in `ViewPresenter` to get a notification when a page is removed. It is triggered in
the following scenarios:
- A page is popped from the navigation stack.
- A modal page is closed.

Note: before the event is raised and when the page has a `IDisposableOnPageClosed` viewmodel assigned to its BindingContext, the viewmodel's
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
        ViewPresenter.Instance.PageRemoved += OnPageRemoved;
    }

    private void OnPageRemoved(object? sender, PageRemovedEventArgs e)
    {
        Debug.WriteLine($"Page removed: {e.RemovedPage.GetType().Name}");
        
        // Optionally broadcast via WeakReferenceMessenger
        WeakReferenceMessenger.Default.Send(e);
    }
}
```

An object; e.g. a viewModel can then subscribe to this event like so:

```csharp
using CommunityToolkit.Mvvm.Messaging;
using cw.MauiExtensions.Services.Events;

public class MyViewModel : ObservableObject,
                           IDisposableOnViewClosed
{
    public MyViewModel()
    {
        WeakReferenceMessenger.Default.Register<PageRemovedEventArgs>(this, (r, m) =>
        {
           // Do something
        });
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
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

Define these colors in your `App.xaml` or `Colors.xaml` (choose your own colors):

```xaml
<!-- System bars (Status bar and Navigation bar on Android) -->
<Color x:Key="SystemBarsBackground">#FFFFFF</Color>
<Color x:Key="SystemBarsBackgroundDark">#000000</Color>

<!-- MAUI NavigationBar (the bar with back button and title) -->
<Color x:Key="NavigationBarBackground">#FFFFFF</Color>
<Color x:Key="NavigationBarBackgroundDark">#000000</Color>
<Color x:Key="NavigationBarText">#101010</Color>
<Color x:Key="NavigationBarTextDark">#FFFFFF</Color>

<!-- ContentDialog overlay backgrounds (semi-transparent for popups/dialogs) -->
<Color x:Key="ContentDialogBackgroundOverlay">#4C000000</Color>
<Color x:Key="ContentDialogBackgroundOverlayDark">#80000000</Color>
```

Additional required Colors in the ContentDialogBorder style when AlertDialog is used

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
                options.ResourceKeys.NavigationBarBackgroundColor = "PageBackground";
                options.ResourceKeys.NavigationBarBackgroundDarkColor = "PageBackgroundDark";
                options.ResourceKeys.NavigationBarTextColor = "NavigationBarText";
                options.ResourceKeys.NavigationBarTextDarkColor = "NavigationBarTextDark";
                options.ResourceKeys.ContentDialogBackgroundOverlayColor = "ContentDialogBackgroundOverlay";
                options.ResourceKeys.ContentDialogBackgroundOverlayDarkColor = "ContentDialogBackgroundOverlayDark";
                
                options.UseSmartSystemBarColoring = true; // Default: true
                options.UseSmartSystemBarColoringWithModals = true; // Default: true
                options.UseDarkSystemBarIconsWithModalPages = true; // Default: true
                options.UseDarkSystemBarIconsWithModalPagesDark = false; // Default: false
                
                // Configure edge-to-edge display
                options.EdgeToEdgeForRootPages = true; // Default: true
                options.EdgeToEdgeForModalPages = true; // Default: true
                options.EdgeToEdgeStartContentBelowBar = true;  // Default: true
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
| `ResourceKeys.AlertDialogBorderStyle` | `string` | `"ContentDialogBorder"` | Border style applied to alert dialogs |
| `ResourceKeys.AlertDialogButtonStyle` | `string` | `"TextOnlyButton"` | Style applied to alert dialog buttons |
| `ResourceKeys.ContentDialogBackgroundOverlayColor` | `string` | `"ContentDialogBackgroundOverlay"` | Overlay color for content dialogs (light mode) |
| `ResourceKeys.ContentDialogBackgroundOverlayDarkColor` | `string` | `"ContentDialogBackgroundOverlayDark"` | Overlay color for content dialogs (dark mode) |
| `ResourceKeys.SystemBarsBackgroundColor` | `string` | `"SystemBarsBackground"` | System bars background color (light mode) |
| `ResourceKeys.SystemBarsBackgroundDarkColor` | `string` | `"SystemBarsBackgroundDark"` | System bars background color (dark mode) |
| `ResourceKeys.NavigationBarBackgroundColor` | `string` | `"NavigationBarBackground"` | MAUI NavigationBar background color (light mode) |
| `ResourceKeys.NavigationBarBackgroundDarkColor` | `string` | `"NavigationBarBackgroundDark"` | MAUI NavigationBar background color (dark mode) |
| `ResourceKeys.NavigationBarTextColor` | `string` | `"NavigationBarText"` | MAUI NavigationBar text color (light mode) |
| `ResourceKeys.NavigationBarTextDarkColor` | `string` | `"NavigationBarTextDark"` | MAUI NavigationBar text color (dark mode) |
| `EdgeToEdgeForRootPages` | `bool` | `true` | Enable edge-to-edge display for root pages |
| `EdgeToEdgeForModalPages` | `bool` | `true` | Enable edge-to-edge display for modal pages |
| `EdgeToEdgeStartContentBelowBar` | `bool` | `true` | When edge-to-edge is enabled, add padding to start content below the status bar |
| `UseDarkSystemBarIconsWithModalPages` | `bool` | `true` | Use dark system bar icons when modal page is open (light mode) |
| `UseDarkSystemBarIconsWithModalPagesDark` | `bool` | `false` | Use dark system bar icons when modal page is open (dark mode) |
| `UseSmartSystemBarColoringWithModals` | `bool` | `true` | Enable smart system bar color handling for modal pages and popups |
| `UseSmartSystemBarColoring` | `bool` | `true` | Enable smart system bar coloring at app startup and on theme changes |


---
# Troubleshooting

### MissingResourceException

If you get a `MissingResourceException`, ensure you have defined all required color resources in your `Colors.xaml` or `App.xaml`.
The exception message will tell you which resource key is missing.

---
# License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 [Jos Huybrighs]

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
