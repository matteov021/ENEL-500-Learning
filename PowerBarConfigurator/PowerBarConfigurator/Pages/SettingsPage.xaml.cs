using Microsoft.Win32;
using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using PowerBarConfigurator.Core.Services;
using System.Windows;
using System.Windows.Controls;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the Settings page of the PowerBarConfigurator application.
    /// 
    /// The SettingsPage allows users to configure application preferences and device settings,
    /// including selecting firmware files for flashing, entering DFU mode, updating the application
    /// theme (Light or Dark), and changing the system voltage setting. It interacts with the
    /// ConnectionService, FlashingService, and ActivityLogService to perform device operations
    /// and log relevant actions.
    /// 
    /// This page handles user input, validates actions, confirms critical operations,
    /// and provides real-time feedback to the user through the UI and activity log.
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        // Store the selected firmware path for flashing
        private string? _firmwarePath;

        // Modbus slave ID for sending commands to the device
        private const byte SlaveId = 1;

        // Constructor
        public SettingsPage()
        {
            InitializeComponent();

            // Load saved voltage and theme settings from application settings and update the UI accordingly
            var savedVoltage = Properties.Settings.Default.Voltage;
            VoltageSelector.SelectedIndex = savedVoltage == "120 V" ? 0 : 1;

            // Load saved theme preference and update the theme selector ComboBox
            var savedTheme = Properties.Settings.Default.Theme;
            ThemeSelector.SelectedIndex = savedTheme == "Dark" ? 0 : 1;

            AppServices.Connection.DataReceived += OnDataReceived;
        }

        // Event handler for receiving data from the device, extracts firmware version and updates UI
        private void OnDataReceived(string data)
        {
            // The data is expected to be a comma-separated string of register values
            var parts = data.Split(',');
            var registers = parts.Select(ushort.Parse).ToArray();

            // Ensure we have enough registers to read the firmware version (registers 41 and 42)
            if (registers.Length < 43)
                return;

            // Combine registers 41 and 42 to get the firmware version as a float
            float firmwareRev = ConnectionService.CombineFloat(registers[42], registers[41]);

            // Update the firmware version text on the UI thread
            Dispatcher.Invoke(() =>
            {
                FirmwareVersionText.Text = $"Current Version: v{firmwareRev:F2}";
            });
        }

        // Event handler for the "Select Firmware" button, opens a file dialog to choose a .bin firmware file
        private void SelectFirmware_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select a firmware file
            var dialog = new OpenFileDialog
            {
                Filter = "Firmware Files (*.bin)|*.bin",
                Title = "Select Firmware File"
            };

            // If the user selects a file and clicks "OK", store the path and enable the DFU button
            if (dialog.ShowDialog() == true)
            {
                _firmwarePath = dialog.FileName;
                FirmwarePathText.Text = dialog.FileName;
                EnterDfuButton.IsEnabled = true;

                AppServices.ActivityLog.Add($"Firmware file selected: {dialog.FileName}");
            }
        }

        // Event handler for the "Enter DFU Mode" button, sends a command to the device to reboot into DFU mode
        private async void EnterDfu_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Check if the device is currently connected before attempting to send the DFU command
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

            // Confirm with the user before sending the command to enter DFU mode
            var confirm = MessageBox.Show(
                "Are you sure you want to enter DFU Mode?",
                "Enter DFU Mode", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Warning);

            // If the user does not confirm, do not proceed with sending the command
            if (confirm != MessageBoxResult.OK) 
                return;

            try
            {
                bool success = connection.TryWriteSingleRegister(SlaveId, 20, 1);

                if (!success)
                {
                    MessageBox.Show(
                        "Failed to send reboot command to the device.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    AppServices.ActivityLog.Add("Error: Failed to send DFU reboot command.");
                    return;
                }

                // Log the successful sending of the DFU reboot command
                AppServices.ActivityLog.Add("DFU reboot command sent.");

                await Task.Delay(200);

                // Disconnect from the device to allow it to reboot into DFU mode
                connection.Disconnect();

                AppServices.ActivityLog.Add("Device rebooting into DFU mode...");

                await Task.Delay(5000);

                // After waiting for the device to reboot, re-enable the flash button to allow firmware flashing
                FlashButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AppServices.ActivityLog.Add($"Error: Unexpected DFU reboot error. {ex.Message}");

                MessageBox.Show(
                    $"Unexpected error:\n{ex.Message}",
                    "DFU Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Event handler for the "Flash Firmware" button, initiates the firmware flashing process
        private async void FlashFirmware_Click(object sender, RoutedEventArgs e)
        {
            if (_firmwarePath == null)
            {
                MessageBox.Show(
                    "Please select a firmware file before flashing.",
                    "Firmware File Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Confirm with the user before starting the firmware flashing process.
            var result = MessageBox.Show(
                "Firmware flashing will start.\n" +
                "Do not disconnect the device during this process.",
                "Flash Firmware",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            // If the user does not confirm, do not proceed with flashing.
            if (result != MessageBoxResult.OK)
                return;

            try
            {
                await AppServices.Flashing.FlashFirmwareAsync(_firmwarePath);

                MessageBox.Show(
                    "Firmware flashing completed successfully.\n\n" +
                    "Disconnect all power from the device, then reconnect it.",
                    "Firmware Update Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Firmware flashing failed.\n\n{ex.Message}",
                    "Firmware Flash Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Event handler for the theme selector ComboBox, updates the application theme and saves the preference.
        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) 
                return;

            // Get the selected theme from the ComboBox.
            var selected = (ThemeSelector.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (selected == null) 
                return;

            // Update the application theme using the App class method.
            ((App)Application.Current).SetTheme(selected);

            // Save the selected theme to application settings for persistence.
            Properties.Settings.Default.Theme = selected;
            Properties.Settings.Default.Save();

            AppServices.ActivityLog.Add($"Application theme changed to {selected}.");
        }

        // Event handler for the voltage selector ComboBox.
        private void VoltageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) 
                return;

            // Get the selected voltage from the ComboBox.
            var selected = (VoltageSelector.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (selected == null) 
                return;

            // If the selected voltage is different from the current setting.
            if (selected != Properties.Settings.Default.Voltage)
            {
                var result = MessageBox.Show(
                    $"Changing voltage from {Properties.Settings.Default.Voltage} to {selected} may affect device behavior.\n\nAre you sure you want to continue?",
                    "Confirm Voltage Change",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                // If the user does not confirm the change, revert the selection back to the previous value
                if (result != MessageBoxResult.Yes)
                {
                    // Revert the selection back to the previous value without triggering the event handler again
                    VoltageSelector.SelectionChanged -= VoltageSelector_SelectionChanged;
                    VoltageSelector.SelectedItem = selected == "120 V" ? VoltageSelector.Items[1] : VoltageSelector.Items[0];
                    VoltageSelector.SelectionChanged += VoltageSelector_SelectionChanged;

                    return;
                }
            }

            // Save the selected voltage to application settings for persistence
            Properties.Settings.Default.Voltage = selected;
            Properties.Settings.Default.Save();

            AppServices.ActivityLog.Add($"System voltage setting changed to {selected}.");
        }
    }
}