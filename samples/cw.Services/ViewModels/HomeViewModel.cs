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
            await ViewPresenter.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel(pageNumber: 1));
        }

        [RelayCommand]
        async Task OpenModalPage()
        {
            // Navigate to modal page on the stack
            await ViewPresenter.Instance.OpenModalPageAsync(new ModalPage(new ModalViewModel()));
        }

        [RelayCommand]
        async Task OpenContentDialog()
        {
            var vm = new MyPopupViewModel();
            var myPopup = new MyPopup(vm);
            var result = await myPopup.ShowAsync();
            if (result != null &&
                result.IsSaved &&
                result.IsChecked)
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
            var page = ViewPresenter.Instance.OpenMainPage(typeof(DemoTabbedPage), null);
        }


        public void OnPageCreated()
        {
            Debug.WriteLine("HomeViewModel: OnPageCreated - Page is appearing");
        }

        public void OnPageDestroyed()
        {
            Debug.WriteLine("HomeViewModel: OnPageDestroyed - Page is disappearing");
        }
    }
}
