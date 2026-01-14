using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Demo.Views;

namespace cw.MauiExtensions.Services.Demo.ViewModels
{
    public partial class DesktopTitleBarViewModel : ObservableObject
    {
        [RelayCommand]
        async Task ShowSettings()
        {
            await PagePresentationService.Instance.PushPageAsync(typeof(NonModalPage), new NonModalViewModel(pageNumber: 1));
        }
    }
}
