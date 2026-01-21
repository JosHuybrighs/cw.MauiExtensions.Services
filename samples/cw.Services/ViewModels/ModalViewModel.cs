using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class ModalViewModel : ObservableObject, IPageLifecycleAware, IDisposableOnPageClosed
    {
        [RelayCommand]
        async Task CloseModal()
        {
            // Close the modal page
            await ViewPresenter.Instance.CloseModalPageAsync();
        }

        public ModalViewModel()
        {
        }

        public void Dispose()
        {
            Debug.WriteLine("ModalViewModel: Dispose - Cleanup");
        }

        public void OnPageCreated()
        {
            Debug.WriteLine("ModalViewModel: OnPageCreated - Page is appearing");
        }

        public void OnPageDestroyed()
        {
            Debug.WriteLine("ModalViewModel: OnPageDestroyed - Page is disappearing");
        }
    }
}
