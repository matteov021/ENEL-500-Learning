using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using PowerBarConfigurator.Core;
using PowerBarConfigurator.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PowerBarConfigurator.Pages
{
    /// <summary>
    /// Represents the Graphs page of the PowerBarConfigurator application.
    /// 
    /// The GraphsPage provides real-time visualization of power usage for the connected device.
    /// Users can select a specific outlet, and the page will continuously
    /// update the graph and summary cards with the latest RMS current, voltage, and calculated power.
    /// 
    /// The page runs an asynchronous loop that fetches device data at regular intervals (100ms),
    /// updates the graph and cards safely via the Dispatcher, and logs values to the DataLoggingService.
    /// It also handles validation for device connectivity and ensures that UI updates do not block the main thread.
    /// </summary>
    public partial class GraphsPage : UserControl
    {
        // Observable collection to hold the current or power values for the graph.
        private ObservableCollection<double> _values = new();
        private int _selectedOutlet = 0;
        private int _selectedGraphType = 0;

        // Constructor
        public GraphsPage()
        {
            InitializeComponent();

            // Set the initial selection of the outlet ComboBox to "Outlet 1" and apply the outlet selection.
            OutletComboBox.SelectedIndex = 0;
            GraphComboBox.SelectedIndex = 0;
            ApplyOutlet();

            // Set up the graph with an initial line series that will be updated with real-time data.
            CurrentChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _values,
                    GeometrySize = 0,
                    LineSmoothness = 0.4
                }
            };

            // Start the loop that continuously updates the graph with new data from the device.
            StartGraphLoop();
        }

        // Event handler for the "Apply Outlet" button click.
        private void ApplyOutlet_Click(object sender, RoutedEventArgs e)
        {
            var connection = AppServices.Connection;

            // Check if the device is connected before applying the outlet selection.
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

            // Apply the selected outlet and clear the graph values to start fresh with the new selection.
            ApplyOutlet();

            // Update the selected graph type based on the current selection in the GraphComboBox.
            _selectedGraphType = GraphComboBox.SelectedIndex;

            // Log the outlet selection change indicating which outlet(s) are now being displayed on the graph.
            string outletText = $"Outlet {_selectedOutlet + 1}";

            AppServices.ActivityLog.Add($"Graph outlet selection changed to {outletText}.");
        }

        // Applies the selected outlet from the ComboBox.
        private void ApplyOutlet()
        {
            _selectedOutlet = OutletComboBox.SelectedIndex;
            _values.Clear();
        }

        // Starts an asynchronous loop that continuously updates the graph.
        private async void StartGraphLoop()
        {
            while (true)
            {
                // Wait for a short delay before fetching new data
                await Task.Delay(100);

                // Check if the device is connected before attempting to fetch data
                if (AppServices.Connection.CurrentState != ConnectionState.Connected)
                    continue;

                // Use the Dispatcher to ensure that UI updates are performed on the main thread
                Dispatcher.Invoke(() =>
                {
                    double current = 0;
                    var outletStates = AppServices.Power.OutletStates;
                    
                    // If a specific outlet is selected, check if it is active and get its current.
                    // If the outlet index is out of range or the outlet is not active, the current will remain 0.
                    if (_selectedOutlet < AppServices.Graphs.RMSCurrents.Length 
                        && _selectedOutlet < outletStates.Length 
                        && outletStates[_selectedOutlet])
                    {
                        current = AppServices.Graphs.RMSCurrents[_selectedOutlet];
                    }

                    // Parse the voltage from the settings, calculate the power, and log the current and voltage values.
                    int voltage = ParseVoltage(Properties.Settings.Default.Voltage);
                    double power = current * voltage;

                    // Add the current or power value to the graph values collection based on the user's selection.
                    if (_selectedGraphType == 0)
                        _values.Add(current);
                    else
                        _values.Add(power);

                    // If there are more than 200 values, remove the oldest one.
                    if (_values.Count > 200)
                        _values.RemoveAt(0);

                    // Update the summary cards with the latest values.
                    UpdateCards(current, voltage, power);
                    UpdateBufferMinMax();
                });
            }
        }

        // Updates the current, voltage, and power cards with the latest values formatted for display.
        private void UpdateCards(double current, int voltage, double power)
        {
            CurrentCard.Value = $"{current:F2} A";
            VoltageCard.Value = $"{voltage} V";
            PowerCard.Value = $"{power:F2} W";
        }

        // Parses the voltage from the settings string, extracting the numeric value.
        private int ParseVoltage(string voltageString)
        {
            string digits = new string(voltageString.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int result) ? result : 120;
        }

        // Updates the buffer minimum and maximum values displayed on the UI based on the selected outlet.
        private void UpdateBufferMinMax()
        {
            // Get the minimum and maximum ADC values from the buffer for the selected outlet and update UI accordingly.
            var result = AppServices.Graphs.GetBufferMinMax(_selectedOutlet);
            if (result.HasValue)
                BufferMinMaxText.Text = $"Min: {result.Value.min}, Max: {result.Value.max}";
            else
                BufferMinMaxText.Text = "Min: - , Max: -";
        }
    }
}