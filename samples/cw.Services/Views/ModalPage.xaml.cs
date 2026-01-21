using cw.MauiExtensions.Services.Demo.ViewModels;
using cw.MauiExtensions.Services.Views;

namespace MauiExtensions.Demo.Views;

public partial class ModalPage : ContentPage
{
	bool _isGradientBackgroundColor;

	public ModalPage(ModalViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
		
		//// Mark this as a full-screen modal page for Android status bar handling
		//// This ensures the status bar color matches the page background (PageBackground resource)
		//ModalPageProperties.SetMode(this, ModalPageMode.FullScreen);
	}

    private async void OnToggleBttnClicked(object sender, EventArgs e)
    {
        _isGradientBackgroundColor = !_isGradientBackgroundColor;
        string key;
        bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
        if (darkTheme)
        {
            key = _isGradientBackgroundColor ? "PageGradientDark" : "PageBackgroundDark";
        }
        else
        {
            key = _isGradientBackgroundColor ? "PageGradient" : "PageBackground";
        }
        if (Application.Current.Resources.TryGetValue(key, out var val))
        {
            if (val is Color solidColor)
                Background = solidColor;
            else if (val is Brush brush)
                Background = brush;
        }
        else
        {
            var alert = new AlertDialog(title: "Error",
                                        text: $"Color {key} not found",
                                        primaryBttnText: "Continue");

            await alert.ShowAsync();
        }
    }

}