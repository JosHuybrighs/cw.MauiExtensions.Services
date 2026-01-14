using CommunityToolkit.Mvvm.Messaging;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Demo.ViewModels;
using cw.MauiExtensions.Services.Demo.Views;
using cw.MauiExtensions.Services.Events;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            // Subscribe to PageRemoved event
            PagePresentationService.Instance.PageRemoved += OnPageRemoved;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // return new Window(new AppShell());
            //var page = PagePresentationService.Instance.OpenMainPage(typeof(ModalPage), new ModalViewModel());
            var page = PagePresentationService.Instance.OpenMainNavigationPage(typeof(Views.HomePage), new HomeViewModel());
            //var page = PagePresentationService.Instance.OpenMainPage(typeof(Views.DemoTabbedPage), null);
            var titleBar = new DesktopTitleBar(new DesktopTitleBarViewModel());
            Window window = new Window()
            {
                Page = page,
                TitleBar = titleBar
            };
            return window;
        }

        private void OnPageRemoved(object? sender, PageRemovedEventArgs e)
        {
            // Handle the page removal event
            Debug.WriteLine($"App: Page is removed: {e.RemovedPage.GetType().Name}");
            
            // Broadcast the PageRemovedEventArgs directly via WeakReferenceMessenger
            WeakReferenceMessenger.Default.Send(e);
        }
    }
}