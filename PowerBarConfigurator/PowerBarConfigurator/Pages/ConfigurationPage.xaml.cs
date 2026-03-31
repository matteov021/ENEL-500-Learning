using PowerBarConfigurator.Controls;
using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using PowerBarConfigurator.Core.Services;
using System.Windows;
using System.Windows.Controls;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the Configuration page of the PowerBarConfigurator application.
    ///
    /// The ConfigurationPage allows the user to view and adjust outlet-specific settings,
    /// particularly the amperage or wattage limits for the power bar outlets. Users can select
    /// a specific outlet or apply changes to all outlets at once.
    ///
    /// The page interacts with the ConnectionService for Modbus communication and ensures that
    /// user actions are only applied when the device is connected. It provides informative
    /// MessageBox prompts for errors, invalid input, and confirmation of successful operations.
    /// </summary>
    public partial class ConfigurationPage : UserControl
    {
        // Maximum of 20 amps for the power bar
        private const int MaxAmps = 20;

        // Modbus slave ID for the power bar
        private const byte SlaveId = 1;

        // Constructor
        public ConfigurationPage()
        {
            InitializeComponent();

            // Initialize outlet selector with "All Outlets" option
            OutletSelector.SelectedIndex = 0;
            LimitTypeSelector.SelectedIndex = 0;

            // Subscribe to data received event from the connection service
            AppServices.Connection.DataReceived += OnDataReceived;
        }

        // Event handler for receiving data from the power bar
        private void OnDataReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                // Parse the received data into an array of ushort values
                ushort[] registers = data.Split(',').Select(ushort.Parse).ToArray();

                // Parse voltage from settings
                int voltage = ParseVoltage(Properties.Settings.Default.Voltage);

                // Update each info card based on the received register values
                foreach (var card in InfoCardsPanel.Children.OfType<MultiValueInfoCard>())
                {
                    int outlet = card.RegisterIndex;

                    // Update the status color based on the outlet state (on/off)
                    if (outlet < 10)
                        card.UpdateStatusColor(registers[outlet] > 0);

                    // Calculate amperage, voltage, and wattage for the outlet
                    int baseIndex = 21 + (outlet * 2);

                    // Ensure we have enough registers to read the amperage value
                    if (baseIndex + 1 < registers.Length)
                    {
                        // Combine the high and low registers to get the amperage value
                        ushort high = registers[baseIndex];
                        ushort low = registers[baseIndex + 1];

                        // Convert the combined value to a float representing the amperage
                        float amps = ConnectionService.CombineFloat(high, low);
                        double watts = amps * voltage;

                        // If the amperage is zero, it indicates that no limit is set
                        if (amps == 0)
                        {
                            // Update the info card with blanks for no limit set
                            card.Amperage = $"-";
                            card.Voltage = $"-";
                            card.Wattage = $"-";
                        }
                        else
                        {
                            // Update the info card with the new values
                            card.Amperage = $"{amps:F2} A";
                            card.Voltage = $"{voltage} V";
                            card.Wattage = $"{watts:F2} W";
                        }
                    }
                }
            });
        }

        // Helper method to parse voltage from settings, extracting digits and converting to an integer
        private int ParseVoltage(string voltageString)
        {
            string digits = new string(voltageString.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int result) ? result : 120;
        }

        // Event handler for the Save Config button click
        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            // Validate the input value from the LimitValueBox
            if (!double.TryParse(LimitValueBox.Text, out double inputValue))
            {
                MessageBox.Show(
                    "Invalid input. Please enter a valid number.",
                    "Invalid Input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var connection = AppServices.Connection;

            // Check if the device is connected before attempting to apply the limit
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

            // Determine if the user is setting a wattage limit or an amperage limit based on the selected option
            bool isWattMode = ((ComboBoxItem)LimitTypeSelector.SelectedItem).Content.ToString() == "Wattage";
            int voltage = ParseVoltage(Properties.Settings.Default.Voltage);
            double amps;

            // Validate the input value based on the selected limit type and calculate the corresponding amperage if wattage mode is selected
            if (isWattMode)
            {
                double maxWatts = MaxAmps * voltage;

                // Validate that the input wattage is within the acceptable range for the given voltage
                if (inputValue <= 0 || inputValue > maxWatts)
                {
                    MessageBox.Show(
                        $"Wattage must be between 0 W (Exclusive) and {maxWatts} W for a {voltage} V system.",
                        "Invalid Wattage",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                amps = inputValue / voltage;
            }
            else
            {
                // Validate that the input amperage is within the acceptable range
                if (inputValue <= 0 || inputValue > MaxAmps)
                {
                    MessageBox.Show(
                        $"Amperage must be between 0 A (Exclusive) and {MaxAmps} A.",
                        "Invalid Amperage",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // If in amperage mode, use the input value directly as the amperage
                amps = inputValue;
            }

            // Apply the calculated amperage limit to the selected outlet(s)
            ApplyAmperageLimit(amps);
        }

        // Method to apply the amperage limit to the selected outlet(s)
        private void ApplyAmperageLimit(double amps)
        {
            // Round the amperage value to 2 decimal places for precision and convert it to a float
            float ampValue = (float)Math.Round(amps, 2);

            // Split the float value into high and low ushort values for Modbus register writing
            var (high, low) = ConnectionService.SplitFloat(ampValue);

            var connection = AppServices.Connection;

            // Check if the device is connected before attempting to write the new limit values
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
            
            // Create an array of ushort values to write to the Modbus registers, containing the high and low parts of the amperage value
            ushort[] values = new ushort[] { high, low };

            // Determine whether to apply the limit to all outlets or just a specific outlet
            if (OutletSelector.SelectedIndex == 0)
            {
                // Loop through all 10 outlets and write the same amperage limit to each outlet's corresponding Modbus registers
                for (int i = 0; i < 10; i++)
                {
                    // Calculate the base address for the current outlet's amperage limit registers
                    ushort baseAddress = (ushort)(21 + (i * 2));

                    // Write the high and low values to the Modbus registers for each outlet
                    bool success = connection.TryWriteMultipleRegisters(SlaveId, baseAddress, values);

                    if (!success)
                    {
                        AppServices.ActivityLog.Add("Error: Failed to set current limit for all outlets.");
                        return;
                    }
                }

                AppServices.ActivityLog.Add($"All outlet current limits set to {ampValue:F2} A.");
            }
            else
            {
                // Base address for that outlet's amperage limit registers and write the high and low register values
                int outlet = OutletSelector.SelectedIndex - 1;
                ushort baseAddress = (ushort)(21 + (outlet * 2));

                // Write the high and low values to the Modbus registers for the selected outlet
                bool success = connection.TryWriteMultipleRegisters(SlaveId, baseAddress, values);

                if (!success)
                {
                    AppServices.ActivityLog.Add($"Error: Failed to set current limit for outlet {outlet + 1}.");
                    return;
                }

                AppServices.ActivityLog.Add($"Outlet {outlet + 1} current limit set to {ampValue:F2} A.");
            }
        }
    }
}