using System.Windows;

namespace PowerBarConfigurator
{
    /// <summary>
    /// Represents the entry point for the PowerBarConfigurator application.
    /// 
    /// The App class manages application-level resources, including the theme (Light or Dark),
    /// and handles startup initialization. It provides functionality to dynamically set and
    /// switch themes by updating the merged resource dictionaries and ensures the selected
    /// theme is applied when the application starts.
    /// 
    /// This class serves as the central point for application-wide configuration and resource management.
    /// </summary>
    public partial class App : Application
    {

        // Set the application theme (Light or Dark)
        public void SetTheme(string theme)
        {
            var dictionaries = Resources.MergedDictionaries;

            // Find the existing theme dictionary
            var oldTheme = dictionaries
                .FirstOrDefault(d => d.Source != null &&
                                     d.Source.OriginalString.Contains("Theme"));

            // Remove the old theme if it exists
            if (oldTheme != null)
                dictionaries.Remove(oldTheme);

            var newTheme = new ResourceDictionary();

            // Load the appropriate theme based on the input parameter
            newTheme.Source = theme == "Dark"
                ? new Uri("Themes/Dark.xaml", UriKind.Relative)
                : new Uri("Themes/Light.xaml", UriKind.Relative);

            dictionaries.Add(newTheme);
        }

        // Save the selected theme to application settings on exit
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load the saved theme from settings and apply it
            var savedTheme = PowerBarConfigurator.Properties.Settings.Default.Theme;

            SetTheme(savedTheme);
        }
    }
}