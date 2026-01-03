using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Helpers;
using cw.MauiExtensions.Services.Events;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;

namespace cw.MauiExtensions.Services.Core
{
    public class PageNavigationService
    {
        private static volatile PageNavigationService? sInstance;

        public static PageNavigationService Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new PageNavigationService();
                }
                return sInstance;
            }
        }

        /// <summary>
        /// Event raised when a page is removed from the navigation stack.
        /// </summary>
        public event EventHandler<PageRemovedEventArgs>? PageRemoved;


        NavigationPage? _navigationPage;
        readonly SemaphoreSlim _modalSemaphore = new(1, 1);


        public enum OpenMode
        {
            PushToStack,
            ReplaceCurrent
        }

        PageNavigationService()
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            }
        }

        public NavigationPage OpenMainPage(Type viewType, object viewModel)
        {
            if (Application.Current == null)
            {
                throw new InvalidOperationException("Application.Current is null.");
            }
            // 1. Before creating the new page we must cleanup the current page, including possible
            // pages on the navigation stack. This is done by popping all the pages from the
            // navigation stack and telling the bound ViewModels that the page they are bound to
            // is gone.
            object? pageObject;
            if (_navigationPage != null)
            {
                foreach (var p in _navigationPage.Navigation.NavigationStack)
                {
                    OnNavigationPagePopped(this, new NavigationEventArgs(p));
                }
                // Deregister 'popped' event handlers
                _navigationPage.Popped -= OnNavigationPagePopped;
                _navigationPage.PoppedToRoot -= OnNavigationPagePoppedToRoot;
            }
            // 2. Create the new main page
            if (viewModel == null)
            {
                pageObject = Activator.CreateInstance(viewType);
            }
            else
            {
                pageObject = Activator.CreateInstance(viewType, viewModel);
            }
            if (pageObject == null)
            {
                Debug.WriteLine($"AppShellNavigationService.OpenMainPage - Could not create instance of type {viewType.FullName}");
                throw new InvalidOperationException($"Could not create instance of type {viewType.FullName}");
            }
            // Create a new NavigationPage with the requested page as root
            _navigationPage = new NavigationPage((Page)pageObject);
            _navigationPage.Popped += OnNavigationPagePopped;
            _navigationPage.PoppedToRoot += OnNavigationPagePoppedToRoot;

            // Hook up lifecycle events for the root page
            HookPageLifecycleEvents((Page)pageObject);

            // Set colors of the top navigation bar
            UpdateNavigationBarColors();

            // Check if we are replacing the current MainPage
            if (Application.Current.Windows.Count != 0)
            {
                // Yes - Set the new page
                Application.Current.Windows[0].Page = _navigationPage;
            }
            return _navigationPage;
        }

        public async Task OpenContentPageAsync(Type viewType, object viewModel, OpenMode mode = OpenMode.PushToStack, int pagesToPopCount = 0)
        {
            try
            {
                // Close a possible popup that is still open.
                // Note: the service only allows 1 popup to be open.
                //await AppPopupService.Instance.ClosePopupAsync();
                switch (mode)
                {
                    case OpenMode.ReplaceCurrent:
                        break;

                    case OpenMode.PushToStack:
                        if (_navigationPage == null)
                        {
                            Debug.WriteLine("AppShellNavigationService.OnOpenContentPageEvent - NavigationPage is null");
                            return;
                        }
                        // Remove possible modal pages anyhow
                        while (_navigationPage.Navigation.ModalStack.Count != 0)
                        {
                            await _navigationPage.Navigation.PopModalAsync(animated: false);
                        }
                        object? instanceObject;
                        if (viewModel == null)
                        {
                            instanceObject = Activator.CreateInstance(viewType);
                        }
                        else
                        {
                            instanceObject = Activator.CreateInstance(viewType, viewModel);
                        }
                        if (instanceObject == null)
                        {
                            Debug.WriteLine($"AppShellNavigationService.OnOpenContentPageEvent - Could not create instance of type {viewType.FullName}");
                            return;
                        }
                        Page newPage = (Page)instanceObject;
                        
                        // Hook up lifecycle events for the new page
                        HookPageLifecycleEvents(newPage);
                        
                        int popCount = pagesToPopCount;
                        if (popCount != 0)
                        {
                            int navStackCount = _navigationPage.Navigation.NavigationStack.Count;
                            if (navStackCount > 6)
                            {

                            }
                            Page insertBeforePage = _navigationPage.Navigation.NavigationStack[navStackCount - 1];
                            _navigationPage.Navigation.InsertPageBefore(newPage, insertBeforePage);
                            while (popCount != 0 &&
                                   _navigationPage.Navigation.NavigationStack.Count != 1)
                            {
                                if (popCount != 1)
                                {
                                    Page pageToRemove = _navigationPage.Navigation.NavigationStack[navStackCount - pagesToPopCount];
                                    _navigationPage.Navigation.RemovePage(pageToRemove);
                                    // Report popped
                                    OnNavigationPagePopped(this, new NavigationEventArgs(pageToRemove));
                                }
                                else
                                {
                                    Page poppedPage = await _navigationPage.Navigation.PopAsync();
                                }
                                popCount--;
                            }
                        }
                        else
                        {
                            await _navigationPage.Navigation.PushAsync(newPage);
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShellNavigationService.OnOpenContentPageEvent - Exception {ex.Message}");
            }
        }


        /// <summary>
        /// Opens a modal page and updates system bar colors.
        /// </summary>
        /// <param name="modalPage">The page to display as a modal.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when required color resources are not defined in App.xaml.
        /// Required resources: 
        /// - ModalStatusBarBackground (light theme)
        /// - ModalStatusBarBackgroundDark (dark theme)
        /// </exception>
        public async Task OpenModalPageAsync(Page modalPage)
        {
            if (Application.Current == null)
            {
                throw new InvalidOperationException("Application.Current is null.");
            }
            
            await _modalSemaphore.WaitAsync();
            try
            {
                // Hook up lifecycle events for the modal page
                HookPageLifecycleEvents(modalPage);
                
                // Push modal page on the stack
                var page = Application.Current.Windows[0].Page;
                if (page == null)
                {
                    throw new InvalidOperationException("Current page is null.");
                }
                await page.Navigation.PushModalAsync(modalPage);
            }
            finally
            {
                _modalSemaphore.Release();
            }
        }


        public async Task CloseContentPageAsync(int nrofPagesToPop = 1)
        {
            if (_navigationPage == null)
            {
                Debug.WriteLine("AppShellNavigationService.CloseContentPageAsync - NavigationPage is null");
                return;
            }
            while (nrofPagesToPop-- > 0)
            {
                Page pageToRemove = _navigationPage.Navigation.NavigationStack.Last();
                _navigationPage.Navigation.RemovePage(pageToRemove);
                // Report popped
                OnNavigationPagePopped(this, new NavigationEventArgs(pageToRemove));
            }
        }

        public async Task CloseModalPageAsync()
        {
            if (Application.Current == null)
            {
                throw new InvalidOperationException("Application.Current is null.");
            }
            
            await _modalSemaphore.WaitAsync();
            try
            {
                var page = Application.Current.Windows[0].Page;
                if (page?.Navigation.ModalStack.Count > 0)
                {
                    var modalPage = page.Navigation.ModalStack.Last();
                    
                    // Unhook lifecycle events
                    UnhookPageLifecycleEvents(modalPage);
                    
                    // Dispose ViewModel if it implements IAutoDisposableOnViewClosed
                    if (modalPage.BindingContext is IAutoDisposableOnPageClosed disposable)
                    {
                        disposable.Dispose();
                    }
                    
                    await page.Navigation.PopModalAsync();
                    
                    // Raise PageRemoved event
                    PageRemoved?.Invoke(this, new PageRemovedEventArgs(modalPage));
                }
            }
            finally
            {
                _modalSemaphore.Release();
            }
        }


        private void OnNavigationPagePopped(object? sender, NavigationEventArgs e)
        {
            // Get the popped Page
            Page page = e.Page;
            if (page != null)
            {
                // Unhook lifecycle events
                UnhookPageLifecycleEvents(page);

                // Get possible viewmodel and issue Dispose if it supports this
                var viewModel = page.BindingContext;
                if (viewModel is IAutoDisposableOnPageClosed disposable)
                {
                    disposable.Dispose();
                }
                // Raise PageRemoved event
                PageRemoved?.Invoke(this, new PageRemovedEventArgs(page));
            }
            //GC.Collect();
        }

        private void OnNavigationPagePoppedToRoot(object? sender, NavigationEventArgs e)
        {
            OnNavigationPagePopped(sender, e);
        }


        /// <summary>
        /// Hooks up page lifecycle events (Appearing and Disappearing) to notify the ViewModel.
        /// </summary>
        /// <param name="page">The page to hook up lifecycle events for.</param>
        private void HookPageLifecycleEvents(Page page)
        {
            if (page == null) return;

            page.Appearing += OnPageAppearing;
            page.Disappearing += OnPageDisappearing;
        }

        /// <summary>
        /// Unhooks page lifecycle events when the page is removed.
        /// </summary>
        /// <param name="page">The page to unhook lifecycle events from.</param>
        private void UnhookPageLifecycleEvents(Page page)
        {
            if (page == null) return;

            page.Appearing -= OnPageAppearing;
            page.Disappearing -= OnPageDisappearing;
        }

        /// <summary>
        /// Called when a page is appearing.
        /// </summary>
        private void OnPageAppearing(object? sender, EventArgs e)
        {
            if (sender is Page page && page.BindingContext is IPageLifecycleAware lifecycleAware)
            {
                lifecycleAware.OnNavigatedTo();
            }
        }

        /// <summary>
        /// Called when a page is disappearing.
        /// </summary>
        private void OnPageDisappearing(object? sender, EventArgs e)
        {
            if (sender is Page page && page.BindingContext is IPageLifecycleAware lifecycleAware)
            {
                lifecycleAware.OnNavigatedFrom();
            }
        }

        private void UpdateNavigationBarColors()
        {
            if (_navigationPage != null)
            {
                bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
                var backgroundColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.MauiNavigationBarBackgroundDarkColor
                                                                         : MauiExtensionsConfiguration.Instance.ResourceKeys.MauiNavigationBarBackgroundColor,
                                                                  darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));
                _navigationPage.BarBackgroundColor = backgroundColor;

                var textColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.MauiNavigationBarTextDarkColor
                                                                   : MauiExtensionsConfiguration.Instance.ResourceKeys.MauiNavigationBarTextColor,
                                                                  darkTheme ? Color.FromRgba(255, 255, 255, 255) : Color.FromRgba(0, 0, 0, 255));
                _navigationPage.BarTextColor = textColor;
            }
        }


        void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            UpdateNavigationBarColors();
        }
    }
}
