# ContentDialog<TResult> - Comprehensive Guide

## Overview

`ContentDialog<TResult>` is a generic, strongly-typed dialog class in the **cw.MauiExtensions.Services** library that allows you to create custom popup dialogs that return specific result types. It provides a flexible foundation for building modal dialogs with:

- **Type-safe results**: Return any type (primitive, enum, custom class, record)
- **Full XAML support**: Design your dialogs entirely in XAML
- **Automatic overlay styling**: Semi-transparent background with system bar color handling
- **TaskCompletionSource pattern**: Async/await support with `ShowAsync()`
- **Lifecycle management**: Proper cleanup when dialogs are closed

## Table of Contents

- [Class Definition](#class-definition)
- [Basic Usage](#basic-usage)
- [Creating Custom Dialogs with XAML](#creating-custom-dialogs-with-xaml)
  - [Example 1: Text Input Dialog](#example-1-text-input-dialog)
  - [Example 2: Selection Dialog with ViewModel](#example-2-selection-dialog-with-viewmodel)
  - [Example 3: Complex Result with Record Type](#example-3-complex-result-with-record-type)
- [Built-in Dialog: AlertDialog](#built-in-dialog-alertdialog)
- [Advanced Scenarios](#advanced-scenarios)
- [XAML Syntax Reference](#xaml-syntax-reference)
- [Best Practices](#best-practices)
- [Styling Your Dialogs](#styling-your-dialogs)
- [Platform-Specific Behavior](#platform-specific-behavior)

---

## Class Definition

```csharp
namespace cw.MauiExtensions.Services.Views;

public class ContentDialog<TResult> : ContentPage
{
    public View ContentView { get; set; }
    public async Task<TResult> ShowAsync() { }
    protected async Task CloseWithResultAsync(TResult result) { }
    protected Grid BackgroundGrid { get; }
    protected ContentView ContentContainer { get; }
}
```

### Key Members

| Member | Description |
|--------|-------------|
| `ContentView` | Gets or sets the view to display in the dialog center |
| `ShowAsync()` | Opens the dialog and returns a `Task<TResult>` that completes when the dialog is closed |
| `CloseWithResultAsync(TResult)` | Closes the dialog with the specified result value |
| `BackgroundGrid` | Protected property - the root grid container (for advanced scenarios) |
| `ContentContainer` | Protected property - the content container view |

### How It Works

1. The dialog displays as a modal overlay with a semi-transparent background
2. Your content is centered on the screen
3. Tapping outside the dialog (on the background) closes it with `default(TResult)`
4. Call `CloseWithResultAsync()` from your code to close with a specific result
5. The `ShowAsync()` method returns when the dialog is closed

---

## Basic Usage

### 1. Simple Dialog with Primitive Result

Create a dialog that returns a boolean:

```csharp
using cw.MauiExtensions.Services.Views;

var confirmDialog = new ContentDialog<bool>
{
    ContentView = new VerticalStackLayout
    {
        Spacing = 16,
        Padding = 20,
        Children =
        {
            new Label { Text = "Are you sure?", FontSize = 18 },
            new Button { Text = "Yes" },
            new Button { Text = "No" }
        }
    }
};

bool confirmed = await confirmDialog.ShowAsync();
```

### 2. Dialog with Enum Result

Using the built-in `ContentDialogResult` enum:

```csharp
public enum ContentDialogResult
{
    None,      // Dialog dismissed or tapped outside
    Primary,   // Primary action button clicked
    Secondary  // Secondary action button clicked
}

var dialog = new ContentDialog<ContentDialogResult>
{
    ContentView = new MyDialogView()
};

var result = await dialog.ShowAsync();

switch (result)
{
    case ContentDialogResult.Primary:
        // User clicked primary button
        break;
    case ContentDialogResult.Secondary:
        // User clicked secondary button
        break;
    case ContentDialogResult.None:
        // User tapped outside dialog or dismissed it
        break;
}
```

---

## Creating Custom Dialogs with XAML

For complex dialogs, it's recommended to create custom classes with XAML.

### Example 1: Text Input Dialog

This example shows a dialog that prompts the user for text input and returns a result indicating whether they confirmed or cancelled.

#### Step 1: Define the Result Type

```csharp
namespace MyApp.Dialogs;

public record TextInputResult(bool Confirmed, string Text);
```

#### Step 2: Create the XAML

**TextInputDialog.xaml:**

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<views:ContentDialog xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                     xmlns:views="clr-namespace:cw.MauiExtensions.Services.Views;assembly=cw.MauiExtensions.Services"
                     xmlns:local="clr-namespace:MyApp.Dialogs"
                     x:Class="MyApp.Dialogs.TextInputDialog"
                     x:TypeArguments="local:TextInputResult">
  <views:ContentDialog.ContentView>
    <Border Style="{StaticResource ContentDialogBorder}" WidthRequest="350">
      <VerticalStackLayout Spacing="16" Padding="16">
        <Label x:Name="TitleLabel" FontSize="Title" FontAttributes="Bold"/>
        <Entry x:Name="InputEntry" Placeholder="Enter text"/>
        
        <Grid ColumnDefinitions="*,*" ColumnSpacing="12">
          <Button Grid.Column="0" Text="OK" Clicked="OnOkClicked"/>
          <Button Grid.Column="1" Text="Cancel" Clicked="OnCancelClicked"/>
        </Grid>
      </VerticalStackLayout>
    </Border>
  </views:ContentDialog.ContentView>
</views:ContentDialog>
```

#### Step 3: Create the Code-Behind

**TextInputDialog.xaml.cs:**

```csharp
using cw.MauiExtensions.Services.Views;

namespace MyApp.Dialogs;

public partial class TextInputDialog : ContentDialog<TextInputResult>
{
    public TextInputDialog(string title, string placeholder = "")
    {
        InitializeComponent();
        TitleLabel.Text = title;
        InputEntry.Placeholder = placeholder;
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(new TextInputResult(true, InputEntry.Text));
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(new TextInputResult(false, string.Empty));
    }
}
```

#### Step 4: Use the Dialog

```csharp
var dialog = new TextInputDialog("Enter your name", "John Doe");
var result = await dialog.ShowAsync();

if (result != null &&
    result.Confirmed)
{
    Console.WriteLine($"User entered: {result.Text}");
}
else
{
    Console.WriteLine("User cancelled");
}
```

---

### Example 2: Selection Dialog with ViewModel

This example demonstrates a color picker dialog using the MVVM pattern.

#### Step 1: Create the ViewModel

**ColorPickerViewModel.cs:**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MyApp.ViewModels;

public partial class ColorPickerViewModel : ObservableObject
{
    public event EventHandler<Color?>? CloseRequested;

    [ObservableProperty]
    private ObservableCollection<Color> _availableColors = new()
    {
        Colors.Red, Colors.Green, Colors.Blue,
        Colors.Yellow, Colors.Purple, Colors.Orange
    };

    [RelayCommand]
    void SelectColor(Color color)
    {
        CloseRequested?.Invoke(this, color);
    }

    [RelayCommand]
    void Cancel()
    {
        CloseRequested?.Invoke(this, null);
    }
}
```

#### Step 2: Create the XAML

**ColorPickerDialog.xaml:**

```csharp
<?xml version="1.0" encoding="utf-8" ?>
<views:ContentDialog xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                     xmlns:views="clr-namespace:cw.MauiExtensions.Services.Views;assembly=cw.MauiExtensions.Services"
                     xmlns:vm="clr-namespace:MyApp.ViewModels"
                     x:Class="MyApp.Dialogs.ColorPickerDialog"
                     x:DataType="vm:ColorPickerViewModel"
                     x:TypeArguments="x:Color">
  <views:ContentDialog.ContentView>
    <Border Style="{StaticResource ContentDialogBorder}" WidthRequest="300">
      <VerticalStackLayout Spacing="12" Padding="16">
        <Label Text="Choose a Color" FontSize="Title"/>
        
        <CollectionView ItemsSource="{Binding AvailableColors}" SelectionMode="None">
          <CollectionView.ItemTemplate>
            <DataTemplate x:DataType="x:Color">
              <Border BackgroundColor="{Binding .}" HeightRequest="60" Margin="0,4">
                <Border.GestureRecognizers>
                  <TapGestureRecognizer 
                    Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ColorPickerViewModel}}, Path=SelectColorCommand}"
                    CommandParameter="{Binding .}"/>
                </Border.GestureRecognizers>
              </Border>
            </DataTemplate>
          </CollectionView.ItemTemplate>
        </CollectionView>
        
        <Button Text="Cancel" Command="{Binding CancelCommand}"/>
      </VerticalStackLayout>
    </Border>
  </views:ContentDialog.ContentView>
</views:ContentDialog>
```

#### Step 3: Create the Code-Behind

**ColorPickerDialog.xaml.cs:**

```csharp
using cw.MauiExtensions.Services.Views;

namespace MyApp.Dialogs;

public partial class ColorPickerDialog : ContentDialog<Color?>
{
    public ColorPickerDialog(ColorPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private async void OnCloseRequested(object? sender, Color? selectedColor)
    {
        await CloseWithResultAsync(selectedColor);
    }
}
```

#### Step 4: Use the Dialog

```csharp
var viewModel = new ColorPickerViewModel();
var dialog = new ColorPickerDialog(viewModel);
var selectedColor = await dialog.ShowAsync();

if (selectedColor != null)
{
    BackgroundColor = selectedColor;
}
```

---

### Example 3: Complex Result with Record Type

This example shows a settings dialog that returns multiple values using a record type.

#### Define the Result Type

```csharp
namespace MyApp.Dialogs;

public record SettingsResult(
    bool Confirmed,
    bool NotificationsEnabled,
    string Theme,
    int FontSize
);
```

#### Create the ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private string _selectedTheme = "Light";

    [ObservableProperty]
    private int _fontSize = 14;

    public List<string> AvailableThemes { get; } = new() { "Light", "Dark", "Auto" };
}
```

#### Usage

```csharp
var viewModel = new SettingsViewModel
{
    NotificationsEnabled = userSettings.NotificationsEnabled,
    SelectedTheme = userSettings.Theme,
    FontSize = userSettings.FontSize
};

var dialog = new SettingsDialog(viewModel);
var result = await dialog.ShowAsync();

if (result != null &&
    result.Confirmed)
{
    userSettings.NotificationsEnabled = result.NotificationsEnabled;
    userSettings.Theme = result.Theme;
    userSettings.FontSize = result.FontSize;
    await userSettings.SaveAsync();
}
```

---

## Built-in Dialog: AlertDialog

The library includes a pre-built `AlertDialog` class for simple alert/confirmation scenarios.

### Usage

```csharp
using cw.MauiExtensions.Services.Views;

var alert = new AlertDialog(
    title: "Confirm Delete",
    text: "Are you sure you want to delete this item? This action cannot be undone.",
    primaryBttnText: "Delete",
    secondaryBttnText: "Cancel"
);

var result = await alert.ShowAsync();

if (result == ContentDialogResult.Primary)
{
    await DeleteItemAsync();
}
```

---

## Advanced Scenarios

### 1. Validation Before Closing

```csharp
private async void OnSaveClicked(object sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(NameEntry.Text))
    {
        await DisplayAlert("Validation Error", "Name is required", "OK");
        return; // Don't close dialog
    }

    await CloseWithResultAsync(new Result(true, NameEntry.Text));
}
```

### 2. Prevent Tap-Outside Close

```csharp
var dialog = new TextInputDialog("Enter your name", "John Doe")
{
    CloseOnBackgroundTap = false
};
var result = await dialog.ShowAsync();
```

### 3. Dialog with Timeout

```csharp
var dialog = new MyDialog();
var dialogTask = dialog.ShowAsync();

var timeoutTask = Task.Delay(30000);
var completedTask = await Task.WhenAny(dialogTask, timeoutTask);

if (completedTask == timeoutTask)
{
    await PagePresentationService.Instance.CloseModalPageAsync();
}

var result = await dialogTask;
```

---

## XAML Syntax Reference

### Basic Structure

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<views:ContentDialog 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:cw.MauiExtensions.Services.Views;assembly=cw.MauiExtensions.Services"
    x:Class="YourNamespace.YourDialog"
    x:TypeArguments="YourResultType">
  
  <views:ContentDialog.ContentView>
    <!-- Your dialog UI here -->
  </views:ContentDialog.ContentView>
  
</views:ContentDialog>
```

### Type Arguments Reference

| Result Type | XAML Syntax | Example |
|-------------|-------------|---------|
| `bool` | `x:TypeArguments="x:Boolean"` | Yes/no dialogs |
| `int` | `x:TypeArguments="x:Int32"` | Number selection |
| `string` | `x:TypeArguments="x:String"` | Text input |
| `Color` | `x:TypeArguments="x:Color"` | Color picker |
| Custom enum | `x:TypeArguments="local:MyEnum"` | Multiple choice |
| Custom record | `x:TypeArguments="local:MyRecord"` | Complex data |

---

## Best Practices

### ✅ Do's

1. **Use Records for Complex Results**

```csharp
public record LoginResult(bool Success, string Username, string Token);
```

2. **Always Handle Default/Null Results**

```csharp
 var result = await dialog.ShowAsync();
   if (result?.Confirmed == true)
   {
       // Process result
   }
```

3. **Use ViewModels for Complex Dialogs** - Separate UI logic from business logic

4. **Apply Consistent Styling** - Use `ContentDialogBorder` style for all dialogs

5. **Provide Clear Action Buttons** - Users should understand what each button does

### ❌ Don'ts

1. **Don't Block the UI Thread**

```csharp
// ❌ Bad
   var result = dialog.ShowAsync().Result;
   
   // ✅ Good
   var result = await dialog.ShowAsync();
```

2. **Don't Forget to Close** - Every code path should call `CloseWithResultAsync()`

3. **Don't Create Dialogs in Tight Loops** - Dialogs are heavy resources

4. **Don't Store Dialog Instances** - Create new instances each time

---

## Styling Your Dialogs

### Required Style Resources

Define in `Styles.xaml`:

```csharp
<Style x:Key="ContentDialogBorder" TargetType="Border">
    <Setter Property="VerticalOptions" Value="Center" />
    <Setter Property="HorizontalOptions" Value="Center" />
    <Setter Property="Padding" Value="20"/>
    <Setter Property="StrokeShape" Value="RoundRectangle 12"/>
    <Setter Property="StrokeThickness" Value="1"/>
    <Setter Property="Stroke" 
            Value="{AppThemeBinding Light={StaticResource ContentDialogBorderStroke}, 
                                     Dark={StaticResource ContentDialogBorderStrokeDark}}" />
    <Setter Property="BackgroundColor" 
            Value="{AppThemeBinding Light={StaticResource ContentDialogBorderBackground}, 
                                     Dark={StaticResource ContentDialogBorderBackgroundDark}}" />
</Style>
```

### Required Color Resources

Define in `Colors.xaml`:

```csharp
<!-- Dialog overlay (semi-transparent background) -->
<Color x:Key="ContentDialogBackgroundOverlay">#4C000000</Color>
<Color x:Key="ContentDialogBackgroundOverlayDark">#80000000</Color>

<!-- Dialog border -->
<Color x:Key="ContentDialogBorderBackground">#FFFFFF</Color>
<Color x:Key="ContentDialogBorderBackgroundDark">#2A2A2A</Color>
<Color x:Key="ContentDialogBorderStroke">#E0E0E0</Color>
<Color x:Key="ContentDialogBorderStrokeDark">#3D3D3D</Color>
```

---

## Platform-Specific Behavior

### Android
- **System Bar Handling**: Automatically handles status bar and navigation bar colors for overlay mode
- **API Compatibility**: Supports Android API 26+ with optimized handling for API 35+

### iOS / Mac Catalyst
- **Modal Presentation**: Uses standard iOS modal presentation style
- **Safe Area**: Respects device safe areas automatically

### Windows
- **Modal Window**: Presents as modal window overlay
- **Title Bar**: Automatically handles title bar styling

---

## Summary

`ContentDialog<TResult>` provides a powerful, type-safe foundation for creating custom dialogs in .NET MAUI.

### Quick Start Checklist

1. ✅ Define your result type (enum, record, class)
2. ✅ Create XAML with `x:TypeArguments` matching your result type
3. ✅ Create code-behind inheriting `ContentDialog<TResult>`
4. ✅ Set `ContentView` property in XAML with your UI
5. ✅ Call `CloseWithResultAsync(result)` when user completes action
6. ✅ Use `await dialog.ShowAsync()` to display and get result
7. ✅ Handle `null` or default result for cancellation

---

## Additional Resources

- **Library Documentation**: See main [README.md](../../../README.md)
- **AlertDialog Example**: See `AlertDialog.xaml` in the library
- **Demo App**: Check the samples project for working examples
- **GitHub**: [cw.MauiExtensions.Services](https://github.com/JosHuybrighs/cw.MauiExtensions.Services)

---

## License

Copyright (c) 2025 Jos Huybrighs - MIT License

