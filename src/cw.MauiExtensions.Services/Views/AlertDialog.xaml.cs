using cw.MauiExtensions.Services.Helpers;

namespace cw.MauiExtensions.Services.Views;

public enum ContentDialogResult
{
    None,
    Primary,
    Secondary
}


public partial class AlertDialog : ContentDialog<ContentDialogResult>
{
    public AlertDialog(string title, string text, string primaryBttnText, string? secondaryBttnText = null)
    {
        InitializeComponent();

        // Apply configured or default styles
        DialogBorder.Style = ResourcesHelper.GetAlertDialogBorderStyle();
        PrimaryBttn.Style = ResourcesHelper.GetAlertDialogButtonStyle();
        SecondaryBttn.Style = ResourcesHelper.GetAlertDialogButtonStyle();

        this.TitleLabel.Text = title;
        this.TextLabel.Text = text;
        this.PrimaryBttn.Text = primaryBttnText;
        if (string.IsNullOrEmpty(secondaryBttnText))
        {
            this.SecondaryBttn.IsVisible = false;
        }
        else
        {
            this.SecondaryBttn.Text = secondaryBttnText;
        }
    }

    private async void PrimaryBttn_Clicked(Object sender, EventArgs e)
    {
        await CloseWithResultAsync(ContentDialogResult.Primary);
    }

    private async void SecondaryBttn_Clicked(Object sender, EventArgs e)
    {
        await CloseWithResultAsync(ContentDialogResult.Secondary);
    }
}
