using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class ModalViewModel : ObservableObject, IPageLifecycleAware, IAutoDisposableOnPageClosed
    {
        [RelayCommand]
        async Task CloseModal()
        {
            // Close the modal page
            await PagePresentationService.Instance.CloseModalPageAsync();
        }

        public void Dispose()
        {
            Debug.WriteLine("ModalViewModel: Dispose - Cleanup");
        }

        public void OnNavigatedTo()
        {
            Debug.WriteLine("ModalViewModel: OnNavigatedTo - Page is appearing");
        }

        public void OnNavigatedFrom()
        {
            Debug.WriteLine("ModalViewModel: OnNavigatedFrom - Page is disappearing");
        }
    }
}
