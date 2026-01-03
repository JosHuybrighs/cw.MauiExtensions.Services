using cw.MauiExtensions.Services.Demo.ViewModels;
using cw.MauiExtensions.Services.Views;

namespace cw.MauiExtensions.Services.Demo.Views;

public partial class MyPopup : ContentDialog
{
	public MyPopup(MyPopupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(ContentDialogResult.Primary);
    }

    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(ContentDialogResult.Secondary);
    }
}