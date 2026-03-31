using Microsoft.Win32;
using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the Home page of the PowerBarConfigurator application.
    /// 
    /// The HomePage provides a central dashboard for monitoring and controlling the connected power bar device.
    /// It displays real-time connection status, power outlet states, and system activity logs.
    /// 
    /// The page subscribes to device data and activity log events, ensuring that the UI is updated
    /// in real-time while throttling log updates to prevent flooding. It also handles user interactions,
    /// validates actions, and logs all important events to the ActivityLogService.
    /// </summary>
    public partial class HomePage : UserControl
    {
        // To prevent log flooding, we will only log received data every 10 seconds at most
        private DateTime _lastRxLogTime = DateTime.MinValue;
        private readonly TimeSpan _rxLogInterval = TimeSpan.FromSeconds(10);

        // Modbus slave ID for sending commands to the device
        private const byte SlaveId = 1;

        // Constructor
        public HomePage()
        {
            InitializeComponent();

            // Subscribe to connection events
            AppServices.Connection.ConnectionStateChanged += state => Dispatcher.Invoke(RefreshUI);
            AppServices.Connection.DataReceived += OnDeviceDataReceived;

            // Bind the activity log list to the activity log's UI logs
            ActivityList.ItemsSource = AppServices.ActivityLog.UiLogs;

            // Scroll to the latest log entry when a new log entry is added
            AppServices.ActivityLog.UiLogs.CollectionChanged += (_, __) =>
            {
                if (ActivityList.Items.Count > 0)
                    ActivityList.ScrollIntoView(ActivityList.Items[^1]);
            };

            RefreshUI();
        }

        // Event handler for Connect/Disconnect button
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // If currently disconnected, attempt to connect with voltage confirmation
            if (AppServices.Connection.CurrentState == ConnectionState.Disconnected)
            {
                string currentVoltage = Properties.Settings.Default.Voltage;

                // Show a confirmation dialog to the user about the current voltage setting
                var confirm = MessageBox.Show(
                    $"The voltage is currently set to {currentVoltage}.\n" +
                    $"Please ensure this matches your region and device configuration.\n\n" +
                    $"Do you want to continue connecting?",
                    "Confirm Voltage Before Connecting",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                // If the user clicks "No", cancel the connection attempt and log the cancellation
                if (confirm != MessageBoxResult.Yes)
                {
                    AppServices.ActivityLog.Add($"Connection cancelled by user during voltage confirmation ({currentVoltage}).");
                    return;
                }

                try
                {
                    AppServices.ActivityLog.Add("Attempting to connect to device...");
                    await AppServices.Connection.ConnectAsync();
                    AppServices.ActivityLog.Add("Device connected successfully.");
                }
                catch (Exception ex)
                {
                    AppServices.ActivityLog.Add($"Error: Connection failed. {ex.Message}");
                }
            }
            else
            {
                AppServices.Connection.Disconnect();
                AppServices.ActivityLog.Add("Device disconnected.");
            }

            // UI will be refreshed via the ConnectionStateChanged event
            RefreshUI();
        }

        // Event handler for Power ON/OFF button
        private async void PowerButton_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Ensure we are connected before trying to write registers
            if (connection.CurrentState != ConnectionState.Connected)
            {
                MessageBox.Show(
                    "Device is not connected.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                AppServices.ActivityLog.Add("Error: Device is not connected.");
                return;
            }

            try
            {
                // If currently active, we will turn everything off by writing 0 to all registers
                if (AppServices.Power.CurrentState == PowerState.Active)
                {
                    ushort[] zeros = new ushort[10];

                    // Write zeros to all 10 registers to turn everything off
                    bool success = connection.TryWriteMultipleRegisters(SlaveId, 0, zeros);
                    if (!success) 
                        return;

                    // Read back the registers to verify they were set to 0
                    success = connection.TryReadHoldingRegisters(SlaveId, 0, 10, out ushort[] verifyRegisters);
                    if (!success) 
                        return;

                    // Check if all registers are 0 to confirm everything is off
                    if (verifyRegisters.All(r => r == 0))
                        AppServices.ActivityLog.Add("All outlets turned OFF.");
                    else
                        AppServices.ActivityLog.Add("Error: Failed to turn OFF all outlets. Verification mismatch.");
                }
                else
                {
                    ushort[] ones = Enumerable.Repeat((ushort)1, 10).ToArray();

                    // Write ones to all 10 registers to turn everything on
                    bool success = connection.TryWriteMultipleRegisters(SlaveId, 0, ones);
                    if (!success) 
                        return;

                    // Read back the registers to verify they were set to 1
                    success = connection.TryReadHoldingRegisters(SlaveId, 0, 10, out ushort[] verifyRegisters);
                    if (!success) 
                        return;

                    // Check if all registers are 1 to confirm everything is on
                    if (verifyRegisters.All(r => r == 1))
                        AppServices.ActivityLog.Add("All outlets turned ON.");
                    else
                        AppServices.ActivityLog.Add("Error: Failed to turn ON all outlets. Check current limits.");
                }
            }
            catch (Exception ex)
            {
                AppServices.ActivityLog.Add($"Error: Failed to update outlet registers. {ex.Message}");
            }
        }

        // Event handler for Clear Log button
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            AppServices.ActivityLog.Clear();
            AppServices.ActivityLog.Add("Activity log cleared.");
        }

        // Event handler for Export Log button
        private void ExportLogButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a SaveFileDialog to let the user choose where to save the CSV file
            var dialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"activity_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            // If the user selects a file and clicks "Save", export the log to CSV
            if (dialog.ShowDialog() == true)
            {
                AppServices.ActivityLog.ExportCsv(dialog.FileName);
                AppServices.ActivityLog.Add($"Activity log exported to CSV: {dialog.FileName}");

                MessageBox.Show(
                    "Activity log exported successfully.",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // Event handler for receiving data from the device
        private void OnDeviceDataReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Parse the received data into an array of ushort registers
                    ushort[] registers = data.Split(',').Select(r => ushort.Parse(r.Trim())).ToArray();

                    // Log the received register states, but only if enough time has passed
                    if (DateTime.Now - _lastRxLogTime > _rxLogInterval)
                    {
                        string firstTen = string.Join(",", registers.Take(10));
                        AppServices.ActivityLog.Add($"RX outlet states: {firstTen}");
                        _lastRxLogTime = DateTime.Now;
                    }

                    AppServices.Power.UpdateFromRegisters(registers);

                    RefreshUI();
                }
                catch (Exception ex)
                {
                    AppServices.ActivityLog.Add($"Error: Failed to parse device registers. {ex.Message}");
                }
            });
        }

        // Method to refresh the UI elements based on the current connection and power states
        private void RefreshUI()
        {
            UpdateConnectionCard();
            UpdateStatusCard();

            var connection = AppServices.Connection;

            // Update the Connect/Disconnect button text based on the current connection state
            if (connection.CurrentState == ConnectionState.Connected)
            {
                ConnectButtonText.Text = "Disconnect";
            }
            else
            {
                ConnectButtonText.Text = "Connect";
            }
        }

        // Method to update the connection status card based on the current connection state
        private void UpdateConnectionCard()
        {
            // Update the connection status card's text and color based on the current connection state
            switch (AppServices.Connection.CurrentState)
            {
                case ConnectionState.Disconnected:
                    ConnectionCard.Value = "Disconnected";
                    ConnectionCard.StatusColor = Brushes.Red;
                    break;

                case ConnectionState.Connecting:
                    ConnectionCard.Value = "Connecting";
                    ConnectionCard.StatusColor = Brushes.Orange;
                    break;

                case ConnectionState.Connected:
                    ConnectionCard.Value = "Connected";
                    ConnectionCard.StatusColor = Brushes.LimeGreen;
                    break;
            }
        }

        // Method to update the power status card based on the current power state
        private void UpdateStatusCard()
        {
            // Update the power status card's text and color based on the current power state
            switch (AppServices.Power.CurrentState)
            {
                case PowerState.Idle:
                    StatusCard.Value = "Idle";
                    StatusCard.StatusColor = Brushes.Orange;
                    break;

                case PowerState.Active:
                    StatusCard.Value = "Active";
                    StatusCard.StatusColor = Brushes.LimeGreen;
                    break;
            }
        }
    }
}