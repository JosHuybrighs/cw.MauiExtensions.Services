using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Demo.ViewModels;

namespace cw.MauiExtensions.Services.Demo.Views
{
    public partial class Tab3Page : ContentPage
    {
        public Tab3Page()
        {
            InitializeComponent();
        }

        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            // Close the tabbed page (pop it from navigation stack)
            //await PagePresentationService.Instance.CloseContentPageAsync();

            // Open the main page again
            PagePresentationService.Instance.OpenMainNavigationPage(typeof(HomePage), new HomeViewModel());
        }
    }
}
