using CommunityToolkit.Mvvm.ComponentModel;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class MyPopupViewModel : ObservableObject, IPageLifecycleAware, IDisposableOnPageClosed
    {
        [ObservableProperty]
        bool _isChecked = false;


        public MyPopupViewModel()
        { }

        public void Dispose()
        {
            Debug.WriteLine("MyPopupViewModel: Dispose - Cleanup");
        }

        public void OnPageCreated()
        {
            Debug.WriteLine("MyPopupViewModel: OnPageCreated - Page is appearing");
        }

        public void OnPageDestroyed()
        {
            Debug.WriteLine("MyPopupViewModel: OnPageCreated - Page is disappearing");
        }
    }
}
