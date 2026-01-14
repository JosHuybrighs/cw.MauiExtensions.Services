using cw.MauiExtensions.Services.Demo.ViewModels;

namespace cw.MauiExtensions.Services.Demo.Views;

public partial class DesktopTitleBar : TitleBar
{
    public DesktopTitleBar(DesktopTitleBarViewModel viewModel)
	{
		BindingContext = viewModel;
        InitializeComponent();
	}
}