using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using cw.MauiExtensions.Services.Demo.ViewModels;

namespace cw.MauiExtensions.Services.Demo.Views;

public partial class HomePage : ContentPage
{
	public HomePage(HomeViewModel viewModel)
	{
		BindingContext = viewModel;
        InitializeComponent();
    }

    private async void OnShowCommunityToolkitPopupButtonClicked(object sender, EventArgs e)
    {
        var popup = new CommunityToolkitPopup();
        var result = await this.ShowPopupAsync(popup, PopupOptions.Empty);
        if (result is not null)
        {
            await DisplayAlert("Popup Result", $"You selected: {result}", "OK");
        }
    }
}