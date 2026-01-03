using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class MyPopupViewModel : ObservableObject, IPageLifecycleAware, IAutoDisposableOnPageClosed
    {
        [ObservableProperty]
        bool _isChecked = false;


        public MyPopupViewModel()
        { }

        public void Dispose()
        {
            Debug.WriteLine("MyPopupViewModel: Dispose - Cleanup");
        }

        public void OnNavigatedTo()
        {
            Debug.WriteLine("MyPopupViewModel: OnNavigatedTo - Page is appearing");
        }

        public void OnNavigatedFrom()
        {
            Debug.WriteLine("MyPopupViewModel: OnNavigatedTo - Page is disappearing");
        }
    }
}
