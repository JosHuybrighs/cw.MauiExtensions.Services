namespace cw.MauiExtensions.Services.Exceptions
{
    public class MissingResourceException : Exception
    {
        public string ResourceKey { get; }
        public MissingResourceException(string resourceKey)
            : base($"The required resource with key '{resourceKey}' is missing from the application resources. You must add it to your App.xaml or Colors.xaml.")
        {
            ResourceKey = resourceKey;
        }
    }
}
