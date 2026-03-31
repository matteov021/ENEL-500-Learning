using PowerBarConfigurator.Pages;
using PowerBarConfigurator.Styles;
using System.Windows;
using System.Windows.Controls;

namespace PowerBarConfigurator
{
    /// <summary>
    /// Represents the main window of the PowerBarConfigurator application.
    /// 
    /// The MainWindow hosts the primary navigation structure and content areas for the application,
    /// pre-instantiating pages for Home, Graphs, Data Logging, Configuration, Settings, and About
    /// for faster navigation. It manages the sidebar state, handles page navigation via button clicks,
    /// and provides animated toggling of the sidebar's width for a responsive UI.
    /// 
    /// This window serves as the central entry point and container for all major UI components
    /// in the application.
    /// </summary>
    public partial class MainWindow : Window
    {

        // Pre-instantiate pages for faster navigation
        private readonly HomePage _homePage = new HomePage();
        private readonly GraphsPage _graphsPage = new GraphsPage();
        private readonly DataLoggingPage _dataLoggingPage = new DataLoggingPage();
        private readonly ConfigurationPage _configurationPage = new ConfigurationPage();
        private readonly SettingsPage _settingsPage = new SettingsPage();
        private readonly AboutPage _aboutPage = new AboutPage();

        // Sidebar state
        private bool _isSidebarCollapsed = false;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            ContentHost.Content = _homePage;
        }

        // Navigation button click handler
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag.ToString();

            // Switch content based on the button's tag
            ContentHost.Content = tag switch
            {
                "Home" => _homePage,
                "Graphs" => _graphsPage,
                "DataLogging" => _dataLoggingPage,
                "Configuration" => _configurationPage,
                "Settings" => _settingsPage,
                "About" => _aboutPage,
                _ => null
            };
        }

        // Toggle sidebar button click handler
        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            // Determine the target width based on the current state
            double from = SidebarColumn.Width.Value;
            double to = _isSidebarCollapsed ? 240 : 105;

            // Create and start the animation
            var animation = new GridLengthAnimation
            {
                From = new GridLength(from),
                To = new GridLength(to),
                Duration = TimeSpan.FromMilliseconds(220)
            };

            // Apply the animation to the sidebar column
            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, animation);
            _isSidebarCollapsed = !_isSidebarCollapsed;
        }
    }
}