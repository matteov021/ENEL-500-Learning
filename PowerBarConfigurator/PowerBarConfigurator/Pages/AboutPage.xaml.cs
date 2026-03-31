using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the About page of the PowerBarConfigurator application.
    /// 
    /// The AboutPage displays application information and provides hyperlinks to external resources.
    /// It handles hyperlink clicks by opening the target URLs in the user's default web browser.
    /// 
    /// This page is intended to provide users with version information, credits, and relevant links.
    /// </summary>
    public partial class AboutPage : UserControl
    {
        // Constructor
        public AboutPage()
        {
            InitializeComponent();
        }

        // Hyperlink click handler to open the URL in the default browser
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Open the hyperlink URL in the default web browser
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
    }
}