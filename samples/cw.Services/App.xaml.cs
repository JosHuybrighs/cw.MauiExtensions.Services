using CommunityToolkit.Mvvm.Messaging;
using cw.MauiExtensions.Services.Core;
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
            PageNavigationService.Instance.PageRemoved += OnPageRemoved;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // return new Window(new AppShell());
            var navPage = PageNavigationService.Instance.OpenMainPage(typeof(Views.HomePage), new ViewModels.HomeViewModel());
            return new Window(navPage);
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