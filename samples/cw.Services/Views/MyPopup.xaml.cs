using cw.MauiExtensions.Services.Demo.ViewModels;
using cw.MauiExtensions.Services.Views;

namespace cw.MauiExtensions.Services.Demo.Views;

public record MyPopupResult(bool IsSaved, bool IsChecked = false);

public partial class MyPopup : ContentDialog<MyPopupResult>
{
	public MyPopup(MyPopupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(new MyPopupResult(true, ((MyPopupViewModel)BindingContext).IsChecked));
    }

    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        await CloseWithResultAsync(new MyPopupResult(false));
    }
}