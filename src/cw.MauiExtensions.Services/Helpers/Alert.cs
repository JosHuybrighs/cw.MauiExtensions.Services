using cw.MauiExtensions.Services.Views;

namespace cw.MauiExtensions.Services.Helpers
{
    public class Alert
    {
        public static async Task<ContentDialogResult> ShowAsync(string title, string text, string primaryBttnText, string? secondaryBttnText = null)
        {
            AlertDialog dialog = new AlertDialog(title, text, primaryBttnText, secondaryBttnText);
            var result = await dialog.ShowAsync();
            return result;
        }
    }
}
