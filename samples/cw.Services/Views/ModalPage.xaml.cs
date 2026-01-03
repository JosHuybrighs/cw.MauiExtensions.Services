using cw.MauiExtensions.Services.Demo.ViewModels;

namespace MauiExtensions.Demo.Views;

public partial class ModalPage : ContentPage
{
	public ModalPage(ModalViewModel viewModel)
	{
		BindingContext = viewModel;
		InitializeComponent();
		
		//// Mark this as a full-screen modal page for Android status bar handling
		//// This ensures the status bar color matches the page background (PageBackground resource)
		//ModalPageProperties.SetMode(this, ModalPageMode.FullScreen);
	}
}