using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Demo.Views;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class NonModalViewModel : ObservableObject, IPageLifecycleAware, IAutoDisposableOnPageClosed
    {
        int _pageNumber;

        public string Title
        {
            get => $"Page {_pageNumber} on stack";
        }

        [RelayCommand]
        async Task OpenNonModal()
        {
            // Navigate to non-modal page on the stack
            await PagePresentationService.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel(_pageNumber + 1));
        }

        public NonModalViewModel(int pageNumber)
        {
            _pageNumber = pageNumber;
        }

        public void Dispose()
        {
            Debug.WriteLine("NonModalViewModel: Dispose - Cleanup");
        }

        public void OnNavigatedTo()
        {
            Debug.WriteLine("NonModalViewModel: OnNavigatedTo - Page is appearing");
        }

        public void OnNavigatedFrom()
        {
            Debug.WriteLine("NonModalViewModel: OnNavigatedFrom - Page is disappearing");
        }
    }
}
