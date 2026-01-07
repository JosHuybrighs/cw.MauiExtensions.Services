# cw.MauiExtensions.Services Demo App

A comprehensive demonstration application showcasing all features of the **cw.MauiExtensions.Services** library for .NET MAUI.

## Overview

This demo app illustrates how to use the cw.MauiExtensions.Services library to:
- Navigate between pages (modal and non-modal)
- Display content dialogs and alert dialogs
- Implement ViewModel lifecycle management
- Handle Android system bar styling automatically
- Handle Windows title bar styling automatically
- Integrate with CommunityToolkit.Maui popups (optional)

## Features Demonstrated

### 1. **Page Navigation**
- **Non-Modal Pages**: Standard navigation stack pages with back button support
- **Modal Pages**: Full-screen overlay pages that appear on top of existing content
- **OpenMode Options**: Replace current page or push onto navigation stack

### 2. **Dialog Services**
- **AlertDialog**: Simple confirmation dialogs with customizable title, message, and buttons
- **ContentDialog**: Custom content dialogs for complex UI scenarios
- **CommunityToolkit Popup**: Integration example with CommunityToolkit.Maui

### 3. **ViewModel Lifecycle Management**
- **IPageLifecycleAware**: Automatic notifications when pages appear/disappear
- **IAutoDisposableOnPageClosed**: Automatic cleanup when pages are removed
- **Page Removal Events**: Subscribe to page removal notifications

### 4. **Platform-Specific Styling**
- **Android**: Automatic status bar and navigation bar color management
  - Different color schemes for modal overlay vs full-screen modals
  - Edge-to-edge UI support
- **Windows**: Automatic title bar color management
  - Theme-aware title bar colors
  - Back button and controls clearly visible
  - Automatic window registration (no user code required)

## Project Structure

```
cw.Services/
├── Views/
│   ├── HomePage.xaml              # Main demo page with all feature buttons
│   ├── NonModalPage.xaml          # Example non-modal page
│   ├── ModalPage.xaml             # Example modal page
│   ├── MyPopup.xaml               # Custom ContentDialog example
│   └── CommunityToolkitPopup.xaml # CommunityToolkit.Maui popup example
├── ViewModels/
│   ├── HomeViewModel.cs           # Main page ViewModel
│   ├── NonModalViewModel.cs       # Non-modal page ViewModel with lifecycle
│   ├── ModalViewModel.cs          # Modal page ViewModel with lifecycle
│   ├── MyPopupViewModel.cs        # Custom dialog ViewModel
│   └── ExampleLifecycleViewModel.cs # Advanced lifecycle example
├── Resources/
│   └── Styles/
│       ├── Colors.xaml            # Color definitions
│       └── Styles.xaml            # Style definitions including NavigationPage
├── App.xaml.cs                    # App initialization with page removal events
├── MauiProgram.cs                 # Service registration and configuration
└── README.md                      # This file
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 (17.13+) or Visual Studio Code
- Android/iOS/Windows development workloads

### Building and Running

1. **Open the solution**
   ```
   cw.MauiExtensions.Services.Lib.sln
   ```

2. **Set the Demo project as startup project**
   - Right-click on `cw.MauiExtensions.Services.Demo`
   - Select "Set as Startup Project"

3. **Select your target platform**
   - Android
   - iOS
   - Mac Catalyst
   - Windows

4. **Run the app** (F5 or Ctrl+F5)

## Demo Walkthrough

### Home Page

The main page provides buttons to demonstrate each feature:

#### 1. **Open NonModal Page**
Opens a standard navigation page that:
- Pushes onto the navigation stack
- Shows back button in navigation bar
- Implements `IPageLifecycleAware` and `IAutoDisposableOnPageClosed`
- Demonstrates ViewModel lifecycle logging

**Code Reference**: `NonModalViewModel.cs`

#### 2. **Open Modal Page**
Opens a full-screen modal page that:
- Appears as an overlay on top of current page
- Has explicit close button (no back button)
- Demonstrates modal page lifecycle
- Shows automatic system bar color handling

**Code Reference**: `ModalViewModel.cs`, `ModalPage.xaml`

#### 3. **Show Content Dialog**
Displays a custom content dialog that:
- Centers on screen with semi-transparent background
- Contains custom UI (checkbox in this example)
- Returns result based on button clicked
- Automatically handles system bar colors for overlay mode

**Code Reference**: `MyPopup.xaml`, `MyPopupViewModel.cs`

#### 4. **Show Alert**
Shows a simple alert dialog with:
- Title and message
- Two buttons (OK and Cancel)
- Boolean result (`true` for primary, `false` for secondary)
- Automatic overlay styling

**Code Reference**: `HomeViewModel.cs` → `ShowAlert()` method

#### 5. **Show Community Toolkit Popup**
Demonstrates integration with CommunityToolkit.Maui:
- Opens a popup using `CommunityToolkit.Maui.Views.Popup`
- Shows alternative popup implementation
- Useful for apps already using CommunityToolkit

**Code Reference**: `CommunityToolkitPopup.xaml`

## Configuration Example

The demo app configures the library in `MauiProgram.cs`:

```csharp
.UseMauiExtensionsServices(options =>
{
    // Customize resource key names (optional)
    options.ResourceKeys.AlertDialogBorderStyle = "ContentDialogBorder";
    
    // Enable/disable system bar styling
    options.UseSystemStatusBarStyling = true;
    options.UseSystemNavigationBarStyling = true;
    
    // Disable if using CommunityToolkit.Maui for system bars
    options.UseSmartSystemBarColoringWithModals = false;
})
```

## ViewModel Lifecycle Examples

### Basic Lifecycle (HomeViewModel)
```csharp
public class HomeViewModel : ObservableObject, IPageLifecycleAware
{
    public void OnNavigatedTo()
    {
        Debug.WriteLine("Page is appearing");
        // Refresh data, start timers, etc.
    }

    public void OnNavigatedFrom()
    {
        Debug.WriteLine("Page is disappearing");
        // Pause operations
    }
}
```

### Lifecycle with Cleanup (NonModalViewModel)
```csharp
public class NonModalViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnPageClosed
{
    public void OnNavigatedTo()
    {
        Debug.WriteLine("Page is appearing");
    }

    public void OnNavigatedFrom()
    {
        Debug.WriteLine("Page is disappearing");
    }

    public void Dispose()
    {
        Debug.WriteLine("Cleanup - page removed");
        // Dispose resources, cancel operations
    }
}
```

### Modal Page with Close Action (ModalViewModel)
```csharp
public partial class ModalViewModel : ObservableObject, 
    IPageLifecycleAware, 
    IAutoDisposableOnPageClosed
{
    [RelayCommand]
    async Task CloseModal()
    {
        await PagePresentationService.Instance.CloseModalPageAsync();
    }

    public void OnNavigatedTo() { /* ... */ }
    public void OnNavigatedFrom() { /* ... */ }
    public void Dispose() { /* ... */ }
}
```

## Page Removal Event Handling

The demo app subscribes to page removal events in `App.xaml.cs`:

```csharp
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Subscribe to PageRemoved event
        PagePresentationService.Instance.PageRemoved += OnPageRemoved;
    }

    private void OnPageRemoved(object? sender, PageRemovedEventArgs e)
    {
        Debug.WriteLine($"Page removed: {e.RemovedPage.GetType().Name}");
        
        // Broadcast via WeakReferenceMessenger for ViewModels to receive
        WeakReferenceMessenger.Default.Send(e);
    }
}
```

## Color Resources

The demo app defines color resources that the library uses for system bar styling:

### In Colors.xaml:
```xaml
<!-- System bars (Android Status Bar and Navigation Bar) -->
<Color x:Key="SystemBarsBackground">#FFFFFF</Color>
<Color x:Key="SystemBarsBackgroundDark">#000000</Color>

<!-- Page backgrounds -->
<Color x:Key="PageBackground">#FFFFFF</Color>
<Color x:Key="PageBackgroundDark">#000000</Color>

<!-- Toolbar colors (page title and toolbar items) -->
<Color x:Key="ToolBarBackground">#FFFFFF</Color>
<Color x:Key="ToolBarBackgroundDark">#1A1A1A</Color>
<Color x:Key="ToolBarText">#202020</Color>
<Color x:Key="ToolBarTextDark">#E0E0E0</Color>

<!-- Optional: Separate navigation bar colors (back button bar)
     If not defined, will fall back to Toolbar colors above -->
<Color x:Key="NavigationBarBackground">#FFFFFF</Color>
<Color x:Key="NavigationBarBackgroundDark">#000000</Color>
<Color x:Key="NavigationBarText">#101010</Color>
<Color x:Key="NavigationBarTextDark">#FFFFFF</Color>

<!-- Dialog overlay backgrounds (semi-transparent) -->
<Color x:Key="ContentDialogBackgroundOverlay">#4C000000</Color>
<Color x:Key="ContentDialogBackgroundOverlayDark">#80000000</Color>

<!-- Dialog border styling -->
<Color x:Key="ContentDialogBorderBackground">#FAFCFE</Color>
<Color x:Key="ContentDialogBorderBackgroundDark">#202020</Color>
<Color x:Key="ContentDialogBorderStroke">#E0E0E0</Color>
<Color x:Key="ContentDialogBorderStrokeDark">#3D3D3D</Color>
```

## Style Resources

### In Styles.xaml:
```xaml
<!-- Border style for dialogs -->
<Style x:Key="ContentDialogBorder" TargetType="Border">
    <Setter Property="VerticalOptions" Value="Center" />
    <Setter Property="Padding" Value="10,24,10,24"/>
    <Setter Property="StrokeShape" Value="RoundRectangle 15"/>
    <Setter Property="StrokeThickness" Value="1"/>
    <Setter Property="Stroke" Value="{AppThemeBinding ...}" />
    <Setter Property="BackgroundColor" Value="{AppThemeBinding ...}" />
</Style>

<!-- Button style for dialog buttons -->
<Style x:Key="TextOnlyButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent" />
    <Setter Property="TextColor" Value="{AppThemeBinding ...}" />
    <Setter Property="BorderWidth" Value="0" />
</Style>

<!-- NavigationPage style - IMPORTANT for icon colors in a Windows App -->
<Style TargetType="NavigationPage">
    <Setter Property="BarBackgroundColor" Value="{AppThemeBinding Light={StaticResource ToolBarBackground}, Dark={StaticResource ToolBarBackgroundDark}}" />
    <Setter Property="BarTextColor" Value="{AppThemeBinding Light={StaticResource ToolBarText}, Dark={StaticResource ToolBarTextDark}}" />
    <Setter Property="IconColor" Value="{AppThemeBinding Light={StaticResource ToolBarText}, Dark={StaticResource ToolBarTextDark}}" />
</Style>
```

**⚠️ Windows Platform Note**: The `IconColor` property (which controls the back button icon color) can ONLY be set via XAML styles in .NET MAUI. It is not available as a settable property in code-behind. The library's `PagePresentationService` sets `BarBackgroundColor` and `BarTextColor` programmatically, but you must define the `NavigationPage` style in your `Styles.xaml` to control icon colors.

## Testing the Demo

### On Android
1. Run the app on Android device or emulator
2. Observe status bar and navigation bar colors:
   - Main page: Uses `PageBackground` color
   - Modal page: Full-screen with `PageBackground`
   - Dialogs: Overlay with blended color from `PageBackground` + `ContentDialogBackgroundOverlay`
3. Toggle between light and dark mode to see theme-aware colors
4. Check Debug output for lifecycle events

### On iOS/Mac Catalyst
1. Run the app on iOS/Mac
2. Test page navigation and modal presentations
3. Verify lifecycle events in Debug output

### On Windows
1. Run the app on Windows
2. Test all navigation scenarios
3. Verify dialog behavior
4. Notice the automatic title bar color synchronization:
   - Title bar matches your toolbar colors
   - Back button is clearly visible
   - Colors update automatically on theme changes

## Debugging Tips

### Enable Debug Logging
The demo app already has debug logging enabled in `MauiProgram.cs`:

```csharp
#if DEBUG
    builder.Logging.AddDebug();
#endif
```

### Lifecycle Event Logging
Watch the Debug Output window for lifecycle events:
- `OnNavigatedTo` - Page is appearing
- `OnNavigatedFrom` - Page is disappearing
- `Dispose` - Page is being removed and cleaned up
- `PageRemoved` - Page has been removed from navigation

### Common Debugging Scenarios
1. **Lifecycle not firing**: Ensure ViewModel implements the interface
2. **Dispose not called**: Verify page is actually removed (not just hidden)
3. **System bars wrong color**: Check color resource definitions
4. **Navigation issues**: Verify PagePresentationService usage
5. **Windows back button not visible**: Check `IconColor` in `NavigationPage` style in `Styles.xaml`

## Key Takeaways

### ✅ Do's
- ✅ Implement `IPageLifecycleAware` for pages that need lifecycle events
- ✅ Implement `IAutoDisposableOnPageClosed` to clean up resources
- ✅ Use `PagePresentationService` for all navigation
- ✅ Define all required color resources
- ✅ Set `ModalPageMode.Overlay` for custom overlay dialogs
- ✅ Subscribe to page removal events when needed
- ✅ Define `NavigationPage` style with `IconColor` in `Styles.xaml` in a Windows app

### ❌ Don'ts
- ❌ Don't forget to dispose timers and event subscriptions
- ❌ Don't use `await` in `OnNavigatedFrom()` (keep it fast)
- ❌ Don't manually manage system bar colors (let the library handle it)
- ❌ Don't mix navigation methods (use PagePresentationService consistently)

## Platform-Specific Notes

### Windows
- **Title Bar Styling**: Automatically configured by the library (no user code needed)
- **Back Button Icon**: Must configure `IconColor` in `NavigationPage` style in `Styles.xaml`

### Android
- **System Bars**: Automatically styled based on page mode (FullScreen vs Overlay)
- **Edge-to-Edge**: Supported with proper color blending for status/navigation bars
- **API Compatibility**: Works with Android API 26+ (different handling for API 35+)

### iOS/Mac Catalyst
- **Navigation Bar**: Standard MAUI navigation bar styling applies
- **Status Bar**: Follows iOS conventions

## Next Steps

After exploring the demo:
1. Read the [main README](../../README.md) for detailed documentation
2. Check the [Page Lifecycle Management Guide](../../Docs/PageLifecycleManagement.md)
3. Review the [Page Removal Events Guide](../../Docs/PageRemovalEvents.md)
4. Integrate the library into your own MAUI app

## Support

For issues, questions, or contributions:
- GitHub Issues: [https://github.com/JosHuybrighs/cw.MauiExtensions.Services/issues](https://github.com/JosHuybrighs/cw.MauiExtensions.Services/issues)
- Main Documentation: [../../README.md](../../README.md)

## License

This demo app is part of the cw.MauiExtensions.Services library and is licensed under the MIT License.
