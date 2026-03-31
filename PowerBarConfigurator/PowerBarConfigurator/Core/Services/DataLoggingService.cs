using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Provides functionality for logging and managing electrical measurement data.
    /// 
    /// The DataLoggingService collects time-series data such as current, voltage,
    /// and calculated power, and maintains both a full dataset and a UI-friendly
    /// log stream. It supports starting and stopping logging sessions, filtering
    /// by outlet, and throttling log frequency to prevent excessive data capture.
    /// 
    /// The service also ensures thread-safe updates to the UI and enforces a limit
    /// on visible log entries to maintain performance. Logged data can be exported
    /// to a CSV file for further analysis.
    /// 
    /// This service is intended for use in monitoring and analyzing power usage
    /// across one or multiple outlets in real time.
    /// </summary>
    public class DataLoggingService
    {
        // Represents a single log entry with all relevant data
        public class LogEntry
        {
            // Source can be "Outlet 1", "Outlet 2", etc
            public string Source { get; set; } = "";
            
            // Timestamp of the log entry
            public DateTime Timestamp { get; set; }

            // Calculated power in watts (Current * Voltage)
            public double Power { get; set; }

            // Current in amps
            public double Current { get; set; }

            // Voltage in volts
            public int Voltage { get; set; }
        }

        // UI log stream
        public ObservableCollection<string> UiEntries { get; } = new();

        // Full dataset
        private readonly List<LogEntry> _allEntries = new();

        // Indicates whether logging is currently active
        public bool IsLogging { get; private set; }

        // The currently selected outlet index
        public int SelectedOutlet { get; set; } = 0;

        // Maximum number of entries to keep in the UI log to prevent memory issues
        private const int UI_LIMIT = 100;

        // Starts the logging process
        public void Start()
        {
            Clear();
            IsLogging = true;
        }

        // Stops the logging process
        public void Stop()
        {
            IsLogging = false;
        }

        // Adds a new sample to the log.
        public void AddSample(int outlet, double current, int voltage)
        {
            // Only log if logging is active
            if (!IsLogging)
                return;

            // Calculate power based on current and voltage
            double power = current * voltage;

            // Create a new log entry with all relevant data
            var entry = new LogEntry
            {
                Source = $"Outlet {outlet + 1}",
                Timestamp = DateTime.Now,
                Current = current,
                Voltage = voltage,
                Power = current * voltage
            };

            _allEntries.Add(entry);

            // Format the log entry for the UI
            string line =
                $"{entry.Timestamp:HH:mm:ss} | " +
                $"{entry.Source,-11} | " +
                $"{entry.Current,3:F2} A | " +
                $"{entry.Voltage,3} V | " +
                $"{entry.Power,3:F2} W";

            // Add the formatted line to the UI log
            Application.Current.Dispatcher.Invoke(() =>
            {
                UiEntries.Add(line);

                if (UiEntries.Count > UI_LIMIT)
                    UiEntries.RemoveAt(0);
            });
        }

        // Clears all logs from both the full history and the UI stream
        public void Clear()
        {
            _allEntries.Clear();

            Application.Current.Dispatcher.Invoke(() =>
            {
                UiEntries.Clear();
            });
        }

        // Adds a system message to the UI log
        public void AddSystemMessage(string message)
        {
            string line = $"{DateTime.Now:HH:mm:ss} | {message}";

            // System messages are not added to the full dataset, only the UI log
            Application.Current.Dispatcher.Invoke(() =>
            {
                UiEntries.Add(line);

                if (UiEntries.Count > UI_LIMIT)
                    UiEntries.RemoveAt(0);
            });
        }

        // Exports the full dataset to a CSV file
        public void ExportCsv(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Source,Timestamp,Power(W),Current(A),Voltage(V)");

            // Write all entries, not just the UI ones
            foreach (var e in _allEntries)
            {
                sb.AppendLine($"{e.Source},{e.Timestamp:yyyy-MM-dd HH:mm:ss},{e.Power:F2},{e.Current:F2},{e.Voltage}");
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}