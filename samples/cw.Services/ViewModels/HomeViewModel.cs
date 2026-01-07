using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExtensions.Demo.Views;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Demo.Views;
using cw.MauiExtensions.Services.Interfaces;
using cw.MauiExtensions.Services.Views;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class HomeViewModel : ObservableObject, IPageLifecycleAware
    {
        [RelayCommand]
        async Task OpenNonModalPage()
        {
            // Navigate to a non-modal page
            await PagePresentationService.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel());
        }

        [RelayCommand]
        async Task OpenModalPage()
        {
            // Navigate to a non-modal page
            await PagePresentationService.Instance.OpenModalPageAsync(new ModalPage(new ModalViewModel()));
        }

        [RelayCommand]
        async Task OpenContentDialog()
        {
            var vm = new MyPopupViewModel();
            var myPopup = new MyPopup(vm);
            var result = await myPopup.ShowAsync();
            if (result == ContentDialogResult.Primary &&
                vm.IsChecked)
            {
                // User clicked Save and has checked the checkbox
            }
            else
            {
                // User clicked Cancel or closed the dialog
            }   
        }

        [RelayCommand]
        async Task ShowAlert()
        {
            var alertDialog = new AlertDialog("Alert", "This is an alert dialog", "OK", "Cancel");
            var result = await alertDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // User clicked OK
            }
            else
            {
                // User clicked Cancel or closed the dialog
            }
        }


        [RelayCommand]
        async Task OpenTabbedPage()
        {
            PagePresentationService.Instance.OpenMainPage(typeof(DemoTabbedPage), null);
        }


        public void OnNavigatedTo()
        {
            Debug.WriteLine("HomeViewModel: OnNavigatedTo - Page is appearing");
        }

        public void OnNavigatedFrom()
        {
            Debug.WriteLine("HomeViewModel: OnNavigatedFrom - Page is disappearing");
        }
    }
}
