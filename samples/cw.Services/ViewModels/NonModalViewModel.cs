using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public class NonModalViewModel : ObservableObject, IPageLifecycleAware, IAutoDisposableOnPageClosed
    {
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
