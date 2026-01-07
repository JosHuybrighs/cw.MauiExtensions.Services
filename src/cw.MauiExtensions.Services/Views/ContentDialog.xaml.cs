using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Helpers;

namespace cw.MauiExtensions.Services.Views;

public enum ContentDialogResult
{
    None,
    Primary,
    Secondary
}


public partial class ContentDialog : ContentPage
{
    TaskCompletionSource<ContentDialogResult>? _tcs;
    ContentDialogResult _closedWithResult = ContentDialogResult.None;

    public View ContentView
    {
        get => ContentContainer.Content;
        set => ContentContainer.Content = value;
    }

    public ContentDialog()
    {
        InitializeComponent();
        
        // Mark this as an overlay modal page for Android status bar handling
        ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
        
        // Overlay color will be set in ShowAsync when we have access to the underlying page
    }


    /// <summary>
    /// Asynchronously opens the modal page associated with this instance and wait for a result.
    /// </summary>
    /// <remarks>Awaiting the returned task will complete when the modal page is closed. This method should be
    /// called when you need to display the modal page and wait for the user providing a result.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user
    /// hasn't canceled the dialog; otherwise, <see langword="false"/>. The actual result must then be retrieved by
    /// retrieving specific public properties of the page that derives from ContentDialog.</returns>
    public async Task<ContentDialogResult> ShowAsync()
    {
        // Set overlay background color derived from the underlying page
        //var underlyingPage = Application.Current?.Windows[0]?.Page;
        //this.BackgroundColor = StyleProvider.GetContentDialogBackgroundOverlayColor(underlyingPage);
        bool darkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
        this.BackgroundColor = ResourcesHelper.GetColor(darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayDarkColor : MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayColor,
                                                        darkTheme ? Color.FromRgba(0, 0, 0, 0.5) : Color.FromRgba(0, 0, 0, 0.55));

        this.Disappearing += OnPageDisappearing;
        _tcs = new TaskCompletionSource<ContentDialogResult>();

        await PagePresentationService.Instance.OpenModalPageAsync(this);

        return await _tcs.Task;
    }

    private void OnPageDisappearing(object? sender, EventArgs e)
    {
        this.Disappearing -= OnPageDisappearing;
        if (_tcs != null && !_tcs.Task.IsCompleted)
        {
            _tcs.TrySetResult(_closedWithResult);
        }
    }

    protected async Task CloseWithResultAsync(ContentDialogResult result)
    {
        _closedWithResult = result;
        await PagePresentationService.Instance.CloseModalPageAsync();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await CloseWithResultAsync(ContentDialogResult.None);
    }

    private void ContentContainerTapped(object sender, TappedEventArgs e)
    {
    }
}