using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PowerBarConfigurator.Controls
{
    /// <summary>
    /// A WPF UserControl that represents a multi-value informational card for a power outlet.
    /// 
    /// The MultiValueInfoCard displays key electrical metrics including amperage, voltage,
    /// and wattage, along with a title and visual status indicator. It also provides
    /// functionality to toggle the power state of a specific outlet via a Modbus connection.
    /// 
    /// Each card is associated with a register index corresponding to a physical outlet
    /// on the power bar device, enabling individual control and monitoring.
    /// 
    /// This control is designed for use in power management dashboards where real-time
    /// outlet data and interaction are required.
    /// </summary>
    public partial class MultiValueInfoCard : UserControl
    {
        // Modbus slave ID for the power bar device
        private const byte SlaveId = 1;

        // Constructor
        public MultiValueInfoCard()
        {
            InitializeComponent();
        }

        // Title Dependency Property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MultiValueInfoCard), new PropertyMetadata("Outlet"));

        // Register Index Dependency Property (0-based index for the outlet)
        public static readonly DependencyProperty RegisterIndexProperty =
            DependencyProperty.Register(nameof(RegisterIndex), typeof(int), typeof(MultiValueInfoCard), new PropertyMetadata(0));

        // Status Color Dependency Property
        public static readonly DependencyProperty StatusColorProperty =
            DependencyProperty.Register(nameof(StatusColor), typeof(Brush), typeof(MultiValueInfoCard), new PropertyMetadata(Brushes.Gray));

        // Amperage Dependency Property
        public static readonly DependencyProperty AmperageProperty =
            DependencyProperty.Register(nameof(Amperage), typeof(string), typeof(MultiValueInfoCard), new PropertyMetadata("-"));

        // Voltage Dependency Property
        public static readonly DependencyProperty VoltageProperty =
            DependencyProperty.Register(nameof(Voltage), typeof(string), typeof(MultiValueInfoCard), new PropertyMetadata("-"));

        // Wattage Dependency Property
        public static readonly DependencyProperty WattageProperty =
            DependencyProperty.Register(nameof(Wattage), typeof(string), typeof(MultiValueInfoCard), new PropertyMetadata("-"));

        // Title Getter and Setter
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Status Color Getter and Setter
        public Brush StatusColor
        {
            get => (Brush)GetValue(StatusColorProperty);
            set => SetValue(StatusColorProperty, value);
        }

        // Amperage Getter and Setter
        public string Amperage
        {
            get => (string)GetValue(AmperageProperty);
            set => SetValue(AmperageProperty, value);
        }

        // Voltage Getter and Setter
        public string Voltage
        {
            get => (string)GetValue(VoltageProperty);
            set => SetValue(VoltageProperty, value);
        }

        // Wattage Getter and Setter
        public string Wattage
        {
            get => (string)GetValue(WattageProperty);
            set => SetValue(WattageProperty, value);
        }

        // Register Index Getter and Setter
        public int RegisterIndex
        {
            get => (int)GetValue(RegisterIndexProperty);
            set => SetValue(RegisterIndexProperty, value);
        }

        // Power button click handler
        private void PowerButton_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Check if the device is connected before attempting to toggle the outlet
            if (connection.CurrentState != ConnectionState.Connected)
            {
                AppServices.ActivityLog.Add($"Error: Cannot toggle outlet {RegisterIndex + 1}. Device is not connected.");

                MessageBox.Show(
                    "Device is not connected.",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            try
            {
                bool success = connection.TryReadHoldingRegisters(SlaveId, (ushort)RegisterIndex, 1, out ushort[] registers);

                // If reading the current state fails, log the error and exit
                if (!success)
                {
                    AppServices.ActivityLog.Add($"Error: Failed to read state for outlet {RegisterIndex + 1}.");
                    return;
                }

                // Toggle the outlet state (assuming 0 = OFF, 1 = ON)
                ushort currentValue = registers[0];
                ushort newValue = (currentValue > 0) ? (ushort)0 : (ushort)1;

                success = connection.TryWriteSingleRegister(SlaveId, (ushort)RegisterIndex, newValue);

                // If writing the new state fails, log the error and exit
                if (!success)
                {
                    AppServices.ActivityLog.Add($"Error: Failed to toggle outlet {RegisterIndex + 1}.");
                    return;
                }

                // Update the status color based on the new state
                UpdateStatusColor(newValue > 0);

                AppServices.ActivityLog.Add($"Outlet {RegisterIndex + 1} turned {(newValue > 0 ? "ON" : "OFF")}.");
            }
            catch (Exception ex)
            {
                AppServices.ActivityLog.Add($"Error: Failed to toggle outlet {RegisterIndex + 1}. {ex.Message}");

                MessageBox.Show(
                    "An unexpected error occurred while toggling the outlet.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Method to update status color based on outlet state
        public void UpdateStatusColor(bool isOn)
        {
            StatusColor = isOn ? Brushes.LimeGreen : Brushes.Gray;
        }
    }
}