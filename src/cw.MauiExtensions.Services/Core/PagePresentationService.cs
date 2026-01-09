using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Helpers;
using cw.MauiExtensions.Services.Events;
using cw.MauiExtensions.Services.Interfaces;
using System.Diagnostics;
#if WINDOWS
using cw.MauiExtensions.Services.Platforms.Windows;
#endif

namespace cw.MauiExtensions.Services.Core
{
    public class PagePresentationService
    {
        private static volatile PagePresentationService? sInstance;

        public static PagePresentationService Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new PagePresentationService();
                }
                return sInstance;
            }
        }

        /// <summary>
        /// Event raised when a page is removed from the navigation stack.
        /// </summary>
        public event EventHandler<PageRemovedEventArgs>? PageRemoved;


        NavigationPage? _mainNavigationPage;
        Page? _mainPage;
        readonly SemaphoreSlim _modalSemaphore = new(1, 1);


        PagePresentationService()
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
            }
        }

        /// <summary>
        /// Creates and returns a new main page instance of the specified type, optionally initialized with the provided
        /// view model. The page is not created in a NavigationPage and so doesn't support navigation stack.
        /// </summary>
        /// <remarks>If a navigation page is currently active, this method performs cleanup before
        /// creating the new main page. The method uses reflection to instantiate the page, so ensure that the specified
        /// type and constructor are accessible. The caller is responsible for handling any exceptions that may occur
        /// during page creation.</remarks>
        /// <param name="viewType">The type of the page to create. Must be a type that derives from Page and has a public constructor that
        /// matches the provided parameters.</param>
        /// <param name="viewModel">An optional view model to pass to the page's constructor. If null, the page is created using its
        /// parameterless constructor.</param>
        /// <returns>A new instance of the specified page type, or null if the page could not be created.</returns>
        public Page? OpenMainPage(Type viewType, object? viewModel)
        {
            // 1. Before creating the new page we must cleanup a possibly existing NavigationPage or MainPage.
            if (_mainNavigationPage != null)
            {
                foreach (var p in _mainNavigationPage.Navigation.NavigationStack)
                {
                    OnNavigationPagePopped(this, new NavigationEventArgs(p));
                }
                // Deregister 'popped' event handlers
                _mainNavigationPage.Popped -= OnNavigationPagePopped;
                _mainNavigationPage.PoppedToRoot -= OnNavigationPagePoppedToRoot;
                _mainNavigationPage = null;
            }
            if (_mainPage != null)
            {
                HandlePageRemoved(_mainPage);
                _mainPage = null;
            }
            // 2. Create page instance
            object? pageObject = viewModel == null
                ? Activator.CreateInstance(viewType)
                : Activator.CreateInstance(viewType, viewModel);

            // Hook up lifecycle events for the root page
            HookPageLifecycleEvents((Page)pageObject);

            _mainPage = (Page)pageObject;
            if (_mainPage != null &&
                Application.Current.Windows.Count != 0)
            {
                // Yes - Set the new page
                Application.Current.Windows[0].Page = _mainPage;
            }

            return _mainPage;
        }

        /// <summary>
        /// Replaces the application's main page with a new navigation page whose root is created from the specified
        /// view type and optional view model.
        /// </summary>
        /// <remarks>This method clears the existing navigation stack and detaches event handlers before
        /// setting the new main page. The new navigation page becomes the application's main page, and its navigation
        /// bar appearance is updated accordingly.</remarks>
        /// <param name="viewType">The type of the page to use as the root of the new navigation stack. Must derive from Page and have a public
        /// constructor compatible with the provided view model.</param>
        /// <param name="viewModel">An optional view model to pass to the page's constructor. If null, the page is created using its
        /// parameterless constructor.</param>
        /// <returns>A NavigationPage instance representing the new main navigation page, with the specified page as its root.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the application is not initialized or if an instance of the specified view type cannot be created.</exception>
        public NavigationPage OpenMainNavigationPage(Type viewType, object? viewModel)
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
            if (_mainNavigationPage != null)
            {
                foreach (var p in _mainNavigationPage.Navigation.NavigationStack)
                {
                    OnNavigationPagePopped(this, new NavigationEventArgs(p));
                }
                // Deregister 'popped' event handlers
                _mainNavigationPage.Popped -= OnNavigationPagePopped;
                _mainNavigationPage.PoppedToRoot -= OnNavigationPagePoppedToRoot;
            }
            if (_mainPage != null)
            {
                HandlePageRemoved(_mainPage);
                _mainPage = null;
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
            _mainNavigationPage = new NavigationPage((Page)pageObject);
            _mainNavigationPage.Popped += OnNavigationPagePopped;
            _mainNavigationPage.PoppedToRoot += OnNavigationPagePoppedToRoot;

            // Hook up lifecycle events for the root page
            HookPageLifecycleEvents((Page)pageObject);

            // Set colors of the top navigation bar
            UpdateNavigationBarColors();

            // Check if we are replacing the current MainPage
            if (Application.Current.Windows.Count != 0)
            {
                // Yes - Set the new page
                Application.Current.Windows[0].Page = _mainNavigationPage;
            }
            return _mainNavigationPage;
        }

        /// <summary>
        /// Navigates to a new page of the specified type, optionally associating a view model and controlling how the
        /// navigation stack is modified.
        /// </summary>
        /// <remarks>If pagesToPopCount is greater than zero, the method removes the specified number of
        /// pages from the navigation stack before navigating to the new page. The method ensures that any modal pages
        /// are closed before navigation. If the page instance cannot be created, navigation is not performed. Only one
        /// popup is supported at a time; any open popup is closed before navigation.</remarks>
        /// <param name="viewType">The type of the page to navigate to. Must be a subclass of Page and have a public constructor that matches
        /// the provided view model, if any.</param>
        /// <param name="viewModel">An optional view model to associate with the new page. If null, the page is created using its default
        /// constructor.</param>
        /// <param name="pagesToPopCount">The number of pages to remove from the navigation stack before pushing the new page. Must be zero or a
        /// positive integer.</param>
        /// <returns>A task that represents the asynchronous navigation operation.</returns>
        public async Task PushPageAsync(Type viewType, object viewModel, int pagesToPopCount = 0)
        {
            try
            {
                if (_mainNavigationPage == null)
                {
                    Debug.WriteLine("AppShellNavigationService.OnOpenContentPageEvent - NavigationPage is null");
                    return;
                }
                // Remove possible modal pages.
                // Note: the service doesn't allow a modal page to remain open when a new page is opened.
                while (_mainNavigationPage.Navigation.ModalStack.Count != 0)
                {
                    await _mainNavigationPage.Navigation.PopModalAsync(animated: false);
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
                    int navStackCount = _mainNavigationPage.Navigation.NavigationStack.Count;
                    if (navStackCount > 6)
                    {

                    }
                    Page insertBeforePage = _mainNavigationPage.Navigation.NavigationStack[navStackCount - 1];
                    _mainNavigationPage.Navigation.InsertPageBefore(newPage, insertBeforePage);
                    while (popCount != 0 &&
                            _mainNavigationPage.Navigation.NavigationStack.Count != 1)
                    {
                        if (popCount != 1)
                        {
                            Page pageToRemove = _mainNavigationPage.Navigation.NavigationStack[navStackCount - pagesToPopCount];
                            _mainNavigationPage.Navigation.RemovePage(pageToRemove);
                            // Report popped
                            OnNavigationPagePopped(this, new NavigationEventArgs(pageToRemove));
                        }
                        else
                        {
                            Page poppedPage = await _mainNavigationPage.Navigation.PopAsync();
                        }
                        popCount--;
                    }
                }
                else
                {
                    await _mainNavigationPage.Navigation.PushAsync(newPage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShellNavigationService.OnOpenContentPageEvent - Exception {ex.Message}");
            }
        }


        /// <summary>
        /// Asynchronously displays the specified page as a modal dialog on top of the current navigation stack.
        /// </summary>
        /// <remarks>This method ensures that only one modal page is presented at a time by synchronizing
        /// access. The modal page is pushed onto the navigation stack of the application's main window.</remarks>
        /// <param name="modalPage">The page to present modally. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the current application or the main page is not available.</exception>
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


        /// <summary>
        /// Removes one or more pages from the top of the navigation stack asynchronously.
        /// </summary>
        /// <remarks>If the navigation stack contains fewer pages than specified by nrofPagesToPop, only
        /// the available pages will be removed. No action is taken if the navigation page is not set.</remarks>
        /// <param name="nrofPagesToPop">The number of pages to remove from the navigation stack. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <returns></returns>
        public async Task PopPageAsync(int nrofPagesToPop = 1)
        {
            if (_mainNavigationPage == null)
            {
                Debug.WriteLine("AppShellNavigationService.CloseContentPageAsync - NavigationPage is null");
                return;
            }
            while (nrofPagesToPop-- > 0)
            {
                Page pageToRemove = _mainNavigationPage.Navigation.NavigationStack.Last();
                _mainNavigationPage.Navigation.RemovePage(pageToRemove);
                // Report popped
                OnNavigationPagePopped(this, new NavigationEventArgs(pageToRemove));
            }
        }

        /// <summary>
        /// Asynchronously closes the topmost modal page if one is present on the application's main window.
        /// </summary>
        /// <remarks>If the modal page's binding context implements IAutoDisposableOnPageClosed, it is
        /// disposed before the page is closed. The PageRemoved event is raised after a modal page is successfully
        /// removed. This method is thread-safe and ensures only one modal close operation occurs at a time.</remarks>
        /// <returns>A task that represents the asynchronous close operation. The task completes when the modal page has been
        /// closed or if no modal page was present.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the current application instance is not available.</exception>
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
            HandlePageRemoved(page);
        }

        private void OnNavigationPagePoppedToRoot(object? sender, NavigationEventArgs e)
        {
            OnNavigationPagePopped(sender, e);
        }


        private void HandlePageRemoved(Page page)
        {
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
            if (_mainNavigationPage != null)
            {
                // Set MAUI NavigationPage colors on all platforms
                bool darkTheme = Microsoft.Maui.Controls.Application.Current.RequestedTheme == AppTheme.Dark;
                
                // Try to get toolbar-specific colors first, fall back to navigation bar colors
                string toolbarBgKey = darkTheme 
                    ? MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarBackgroundDarkColor
                    : MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarBackgroundColor;
                
                string toolbarTextKey = darkTheme
                    ? MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarTextDarkColor
                    : MauiExtensionsConfiguration.Instance.ResourceKeys.NavigationBarTextColor;
                
                var backgroundColor = ResourcesHelper.GetColor(toolbarBgKey,
                                                               darkTheme ? Color.FromRgba(0, 0, 0, 255) : Color.FromRgba(255, 255, 255, 255));
                _mainNavigationPage.BarBackgroundColor = backgroundColor;

                var textColor = ResourcesHelper.GetColor(toolbarTextKey,
                                                         darkTheme ? Color.FromRgba(255, 255, 255, 255) : Color.FromRgba(0, 0, 0, 255));
                _mainNavigationPage.BarTextColor = textColor;

#if WINDOWS
                // Also configure Windows title bar to match
                // Pass both navigation bar and toolbar colors to Windows service
                WindowsTitleBarService.ConfigureTitleBar();
#endif
            }
        }


        void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            UpdateNavigationBarColors();
        }
    }
}
