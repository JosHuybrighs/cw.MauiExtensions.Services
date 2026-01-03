using cw.MauiExtensions.Services.Demo.ViewModels;

namespace cw.MauiExtensions.Services.Demo.Views;

public partial class NonModalPage : ContentPage
{
	public NonModalPage(NonModalViewModel viewModel)
	{
		BindingContext = viewModel;
        InitializeComponent();
	}
}