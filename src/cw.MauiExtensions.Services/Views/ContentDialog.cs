using cw.MauiExtensions.Services.Configuration;
using cw.MauiExtensions.Services.Core;
using cw.MauiExtensions.Services.Helpers;

namespace cw.MauiExtensions.Services.Views;


public class ContentDialog<TResult> : ContentPage
{
    public bool CloseOnBackgroundTap { set; get; } = true;

    TaskCompletionSource<TResult>? _tcs;
    TResult? _closedWithResult;

    protected Grid BackgroundGrid { get; }
    protected ContentView ContentContainer { get; }

    public View ContentView
    {
        get => ContentContainer.Content;
        set => ContentContainer.Content = value;
    }

    public ContentDialog()
    {
        // Create the UI structure
        ContentContainer = new ContentView
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = false
        };

        // Add tap gesture to ContentContainer to prevent background tap from closing
        var contentTapGesture = new TapGestureRecognizer();
        contentTapGesture.Tapped += ContentContainerTapped;
        ContentContainer.GestureRecognizers.Add(contentTapGesture);

        BackgroundGrid = new Grid
        {
            Children = { ContentContainer }
        };

        // Add tap gesture to background to close dialog
        var backgroundTapGesture = new TapGestureRecognizer();
        backgroundTapGesture.Tapped += TapGestureRecognizer_Tapped;
        BackgroundGrid.GestureRecognizers.Add(backgroundTapGesture);

        Content = BackgroundGrid;

        // Mark this as an overlay modal page for Android status bar handling
        ModalPageProperties.SetMode(this, ModalPageMode.Overlay);
    }


    /// <summary>
    /// Asynchronously opens the modal page associated with this instance and wait for a result.
    /// </summary>
    /// <remarks>Awaiting the returned task will complete when the modal page is closed. This method should be
    /// called when you need to display the modal page and wait for the user providing a result.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result of type <typeparamref name="TResult"/>.</returns>
    public async Task<TResult> ShowAsync()
    {
        // Set overlay background color derived from the underlying page
        bool darkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
        this.BackgroundColor = ResourcesHelper.GetColor(
            darkTheme ? MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayDarkColor
                      : MauiExtensionsConfiguration.Instance.ResourceKeys.ContentDialogBackgroundOverlayColor,
            darkTheme ? Color.FromRgba(0, 0, 0, 0.5)
                      : Color.FromRgba(0, 0, 0, 0.55));

        this.Disappearing += OnPageDisappearing;
        _tcs = new TaskCompletionSource<TResult>();

        await PagePresentationService.Instance.OpenModalPageAsync(this);

        return await _tcs.Task;
    }


    private void OnPageDisappearing(object? sender, EventArgs e)
    {
        this.Disappearing -= OnPageDisappearing;
        if (_tcs != null && !_tcs.Task.IsCompleted)
        {
            _tcs.TrySetResult(_closedWithResult!);
        }
    }

    protected async Task CloseWithResultAsync(TResult result)
    {
        _closedWithResult = result;
        await PagePresentationService.Instance.CloseModalPageAsync();
    }

    private async void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
    {
        if (CloseOnBackgroundTap)
        {
            await CloseWithResultAsync(default!);
        }
    }

    private void ContentContainerTapped(object? sender, TappedEventArgs e)
    {
        // Prevent tap from bubbling to background
    }
}