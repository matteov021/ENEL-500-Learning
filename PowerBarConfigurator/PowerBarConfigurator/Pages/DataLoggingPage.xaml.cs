using Microsoft.Win32;
using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the Data Logging page of the PowerBarConfigurator application.
    ///
    /// The DataLoggingPage provides an interface for managing real-time logging of power usage
    /// data from the connected device. Users can start, stop, clear, and export logged data
    /// for analysis. The page also allows selecting a specific outlet to log.
    ///
    /// The page interacts with the DataLoggingService and logs actions to the central ActivityLog.
    /// It ensures that user actions are only performed when the device is connected and provides
    /// informative messages through MessageBox dialogs.
    /// </summary>
    public partial class DataLoggingPage : UserControl
    {
        // Constructor
        public DataLoggingPage()
        {
            InitializeComponent();

            // Populate the outlet selection combo box
            OutletComboBox.SelectedIndex = 0;

            // Bind the logging list to the data logger's UI entries
            LoggingList.ItemsSource = AppServices.DataLogger.UiEntries;

            // Scroll to the latest entry when a new log entry is added
            AppServices.DataLogger.UiEntries.CollectionChanged += (_, __) =>
            {
                if (LoggingList.Items.Count > 0)
                    LoggingList.ScrollIntoView(LoggingList.Items[^1]);
            };
        }

        // Event handlers for buttons
        private void StartLogging_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Check if the device is connected before starting logging
            if (connection.CurrentState != ConnectionState.Connected)
            {
                MessageBox.Show(
                    "Device is not connected.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                AppServices.ActivityLog.Add("Error: Cannot start data logging. Device is not connected.");
                return;
            }

            // Set the selected outlet for logging
            AppServices.DataLogger.SelectedOutlet = OutletComboBox.SelectedIndex;
            AppServices.DataLogger.Start();

            // Disable the outlet selection while logging is active
            OutletComboBox.IsEnabled = false;
            StartLoggingButton.IsEnabled = false;
            StopLoggingButton.IsEnabled = true;

            // Log the start of data logging
            AppServices.ActivityLog.Add("Data logging started.");
            AppServices.DataLogger.AddSystemMessage("Data logging started.");

            MessageBox.Show(
                "Data logging started.",
                "Data Logging",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Start the loop that continuously logs data from the device while logging is active.
            StartLoggingLoop();
        }

        // Stop logging and re-enable outlet selection
        private void StopLogging_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Check if the device is connected before stopping logging
            if (connection.CurrentState != ConnectionState.Connected)
            {
                MessageBox.Show(
                    "Device is not connected.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                AppServices.ActivityLog.Add("Error: Cannot stop data logging. Device is not connected.");
                return;
            }

            // Stop the data logger
            AppServices.DataLogger.Stop();

            // Re-enable the outlet selection
            OutletComboBox.IsEnabled = true;
            StartLoggingButton.IsEnabled = true;
            StopLoggingButton.IsEnabled = false;

            // Log the stop of data logging
            AppServices.ActivityLog.Add("Data logging stopped.");
            AppServices.DataLogger.AddSystemMessage("Data logging stopped.");

            MessageBox.Show(
                "Data logging stopped.",
                "Data Logging",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Clear the log entries
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            AppServices.DataLogger.Clear();
            AppServices.DataLogger.AddSystemMessage("Data log cleared.");
        }

        // Export the log entries to a CSV file
        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            // Open a save file dialog to choose the location for the CSV export
            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"data_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            // If the user selects a file and confirms, export the data to CSV
            if (dialog.ShowDialog() == true)
            {
                AppServices.DataLogger.ExportCsv(dialog.FileName);
                AppServices.ActivityLog.Add($"Data log exported to CSV: {dialog.FileName}");

                MessageBox.Show(
                    "Data log exported successfully.",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // Parses the voltage from the settings string, extracting the numeric value.
        private int ParseVoltage(string voltageString)
        {
            string digits = new string(voltageString.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int result) ? result : 120;
        }

        // Parses the voltage setting from the application settings, ensuring it is a valid integer.
        private async void StartLoggingLoop()
        {
            // Continuously log data while the data logger is active
            while (AppServices.DataLogger.IsLogging)
            {
                await Task.Delay(1000);

                // Check if the device is still connected before attempting to log data
                if (AppServices.Connection.CurrentState != ConnectionState.Connected)
                    continue;

                Dispatcher.Invoke(() =>
                {
                    // Get the selected outlet, current readings, outlet states, and voltage setting
                    int selected = AppServices.DataLogger.SelectedOutlet;
                    var currents = AppServices.Graphs.RMSCurrents;
                    var states = AppServices.Power.OutletStates;

                    // Parse the voltage from the settings
                    int voltage = ParseVoltage(Properties.Settings.Default.Voltage);

                    // If a specific outlet is selected
                    if (selected < currents.Length && selected < states.Length)
                    {
                        // Only log the current if the selected outlet is active; otherwise, log 0
                        double current = states[selected] ? currents[selected] : 0;
                        AppServices.DataLogger.AddSample(selected, current, voltage);
                    }
                });
            }
        }
    }
}