# ContentDialog

## Overview

The `ContentDialog` class is a versatile popup dialog component provided by `cw.MauiExtensions.Services` that simplifies creating custom modal dialogs in your MAUI application. It handles all the complexity of displaying a modal popup, waiting for user interaction, and returning results.

## Why ContentDialog?

When creating popup dialogs in MAUI, you typically need to:

1. **Create a modal page** with semi-transparent background
2. **Display custom content** (forms, pickers, confirmation messages, etc.)
3. **Wait for user interaction** without blocking the UI thread
4. **Return a result** indicating which action the user took
5. **Properly manage lifecycle** and dispose resources

Doing this manually requires boilerplate code for each popup:
- Creating a `TaskCompletionSource` to await the result
- Setting up button click handlers
- Managing the modal page lifecycle
- Handling the semi-transparent overlay background
- Setting `ModalPageMode.Overlay` for proper system bar styling

**ContentDialog handles all of this for you**, allowing you to focus on your custom content.

## Key Features

- **Easy to use**: Just provide your custom view and optionally add buttons
- **Async/await support**: Returns a `Task<ContentDialogResult>` that you can await
- **Flexible button configuration**: Add primary and/or secondary buttons, or no buttons at all
- **Automatic styling**: Uses your app's configured `ContentDialogBorder` style
- **Proper lifecycle management**: Automatically cleans up when closed
- **System bar integration**: Correctly styles Android status bar for overlay mode
- **Tap-outside-to-close**: Optional feature to dismiss by tapping the overlay

## ContentDialogResult

The dialog returns a `ContentDialogResult` enum value:

```csharp
public enum ContentDialogResult
{
    None,      // Dialog was dismissed (closed without clicking buttons, or no buttons provided)
    Primary,   // Primary button was clicked
    Secondary  // Secondary button was clicked
}
```

## Basic Usage

### Example 1: Simple Message Popup

Display a custom message with an OK button:

```csharp
using cw.MauiExtensions.Services.Views;

private async Task ShowMessageAsync()
{
    var messageView = new VerticalStackLayout
    {
        Spacing = 15,
        Children =
        {
            new Label 
            { 
                Text = "Operation Successful", 
                FontSize = 18, 
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            },
            new Label 
            { 
                Text = "Your changes have been saved successfully.",
                HorizontalOptions = LayoutOptions.Center
            }
        }
    };

    var dialog = new ContentDialog
    {
        ContentView = messageView,
        PrimaryButtonText = "OK"
    };

    await dialog.ShowAsync();
}
```

### Example 2: Confirmation Dialog

Ask the user to confirm an action:

```csharp
private async Task<bool> ConfirmDeleteAsync(string itemName)
{
    var confirmView = new VerticalStackLayout
    {
        Spacing = 15,
        Children =
        {
            new Label 
            { 
                Text = "Confirm Deletion", 
                FontSize = 18, 
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            },
            new Label 
            { 
                Text = $"Are you sure you want to delete '{itemName}'? This action cannot be undone.",
                HorizontalOptions = LayoutOptions.Center
            }
        }
    };

    var dialog = new ContentDialog
    {
        ContentView = confirmView,
        PrimaryButtonText = "Delete",
        SecondaryButtonText = "Cancel"
    };

    var result = await dialog.ShowAsync();
    return result == ContentDialogResult.Primary;
}
```

### Example 3: Custom Input Form

Create a popup with input fields:

```csharp
private async Task<(bool confirmed, string name, string email)> GetUserInfoAsync()
{
    var nameEntry = new Entry { Placeholder = "Enter your name" };
    var emailEntry = new Entry { Placeholder = "Enter your email", Keyboard = Keyboard.Email };

    var formView = new VerticalStackLayout
    {
        Spacing = 15,
        Padding = 20,
        Children =
        {
            new Label 
            { 
                Text = "User Information", 
                FontSize = 18, 
                FontAttributes = FontAttributes.Bold
            },
            new Label { Text = "Name:" },
            nameEntry,
            new Label { Text = "Email:" },
            emailEntry
        }
    };

    var dialog = new ContentDialog
    {
        ContentView = formView,
        PrimaryButtonText = "Submit",
        SecondaryButtonText = "Cancel"
    };

    var result = await dialog.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        return (true, nameEntry.Text, emailEntry.Text);
    }
    
    return (false, string.Empty, string.Empty);
}
```

### Example 4: Custom View with ViewModel

Use a separate XAML view with its own ViewModel:

**MyCustomDialogView.xaml:**
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<VerticalStackLayout xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                     xmlns:vm="clr-namespace:MyApp.ViewModels"
                     x:Class="MyApp.Views.MyCustomDialogView"
                     x:DataType="vm:CustomDialogViewModel"
                     Spacing="15"
                     Padding="20">
    
    <Label Text="{Binding Title}" 
           FontSize="18" 
           FontAttributes="Bold" />
    
    <Label Text="{Binding Message}" />
    
    <Entry Text="{Binding UserInput}" 
           Placeholder="Enter value..." />
    
</VerticalStackLayout>
```

**Usage:**
```csharp
private async Task<string?> ShowCustomDialogAsync()
{
    var viewModel = new CustomDialogViewModel
    {
        Title = "Enter Information",
        Message = "Please provide the required information:"
    };

    var customView = new MyCustomDialogView
    {
        BindingContext = viewModel
    };

    var dialog = new ContentDialog
    {
        ContentView = customView,
        PrimaryButtonText = "OK",
        SecondaryButtonText = "Cancel"
    };

    var result = await dialog.ShowAsync();
    
    return result == ContentDialogResult.Primary ? viewModel.UserInput : null;
}
```

### Example 5: Information-Only Popup (No Buttons)

Display information that dismisses when tapping outside:

```csharp
private async Task ShowInfoAsync()
{
    var infoView = new VerticalStackLayout
    {
        Spacing = 10,
        Padding = 20,
        Children =
        {
            new Label 
            { 
                Text = "?? Did you know?", 
                FontSize = 18, 
                FontAttributes = FontAttributes.Bold
            },
            new Label 
            { 
                Text = "You can tap outside this dialog to dismiss it.",
                TextColor = Colors.Gray
            }
        }
    };

    var dialog = new ContentDialog
    {
        ContentView = infoView
        // No buttons - user can tap outside to close
    };

    await dialog.ShowAsync();
}
```

## Advanced Features

### Customizing Button Styles

The buttons use the style configured in `MauiExtensionsConfiguration`:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.ResourceKeys.AlertDialogButtonStyle = "MyCustomButtonStyle";
})
```

### Customizing the Border

The dialog border uses the configured border style:

```csharp
.UseMauiExtensionsServices(options =>
{
    options.ResourceKeys.AlertDialogBorderStyle = "MyCustomDialogBorder";
})
```

### Handling Different Results

```csharp
var result = await dialog.ShowAsync();

switch (result)
{
    case ContentDialogResult.Primary:
        // User clicked primary button
        await SaveChangesAsync();
        break;
        
    case ContentDialogResult.Secondary:
        // User clicked secondary button
        DiscardChanges();
        break;
        
    case ContentDialogResult.None:
        // User dismissed dialog (tapped outside or closed otherwise)
        // No action needed
        break;
}
```

## Best Practices

1. **Keep content focused**: Dialogs should have a clear, single purpose
2. **Use appropriate button text**: Make button labels action-oriented ("Save", "Delete", "Cancel" instead of "Yes", "No", "OK")
3. **Handle all result cases**: Always check for `ContentDialogResult.None` to handle dismissal
4. **Validate input**: If collecting user input, validate before accepting the result
5. **Avoid complex UI**: Keep dialogs simple - for complex interactions, use a full modal page instead
6. **Test on different screen sizes**: Ensure your content fits on smaller devices

## Comparison with AlertDialog

| Feature | ContentDialog | AlertDialog |
|---------|---------------|-------------|
| **Custom Content** | ? Any View | ? Title + Text only |
| **Flexibility** | ? Complete control | ?? Limited to predefined layout |
| **Ease of Use** | ?? Requires creating content | ? Very simple API |
| **Use Case** | Custom forms, pickers, complex UI | Simple alerts, confirmations |

**When to use ContentDialog:**
- You need custom UI beyond text and buttons
- You want to collect user input (forms, pickers)
- You need to display rich content (images, formatted text)
- You want complete control over the dialog appearance

**When to use AlertDialog:**
- Simple yes/no confirmations
- Displaying error messages
- Quick alerts without custom UI
- When you want the fastest implementation

## Under the Hood

The `ContentDialog` class:

1. **Creates a ContentPage** with a semi-transparent background overlay
2. **Sets ModalPageMode.Overlay** for proper system bar styling on Android
3. **Wraps your content** in a Border with the configured style
4. **Creates TaskCompletionSource** to enable async/await pattern
5. **Handles button clicks** and sets the appropriate result
6. **Manages lifecycle** through PagePresentationService
7. **Cleans up** when dismissed

This allows you to focus on your content while the dialog handles all the plumbing.

## Related Documentation

- [AlertDialog](AlertDialog.md) - For simple text-based alerts
- [PagePresentationService](PagePresentationService.md) - Understanding modal pages
- [ViewModel Lifecycle Management](../README.md#viewmodel-lifecycle-management) - Managing dialog ViewModels

## Summary

`ContentDialog` is your go-to solution when you need custom popup dialogs that:
- Are easy to create and use
- Support async/await patterns
- Properly integrate with MAUI's navigation system
- Handle Android system bars correctly
- Provide a clean API for handling user responses

For simple text-based alerts, consider using `AlertDialog` instead. For full-screen modals with complex navigation, use `PagePresentationService.OpenModalPageAsync()` directly.
