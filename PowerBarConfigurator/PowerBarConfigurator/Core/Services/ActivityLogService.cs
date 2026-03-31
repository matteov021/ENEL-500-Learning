using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Provides a centralized service for managing application activity logs.
    /// 
    /// The ActivityLogService supports adding timestamped log entries, clearing logs,
    /// and exporting logs to a CSV file. It also exposes events to notify subscribers
    /// when logs are added or cleared, enabling real-time UI updates.
    /// 
    /// This service is intended for use across the application to track user actions,
    /// system events, and errors in a consistent and accessible manner.
    /// </summary>
    public class ActivityLogService
    {
        // UI log stream
        public ObservableCollection<string> UiLogs { get; } = new();

        // Full log history
        private readonly List<string> _allLogs = new();

        // Maximum number of log entries to keep in the UI to prevent memory issues
        private const int UI_LIMIT = 100;

        // Adds a log entry with a timestamp
        public void Add(string message)
        {
            // Create a timestamped log entry
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] {message}";

            _allLogs.Add(entry);

            // Update the UI log stream on the main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                UiLogs.Add(entry);

                if (UiLogs.Count > UI_LIMIT)
                    UiLogs.RemoveAt(0);
            });
        }

        // Clears all logs from both the full history and the UI stream
        public void Clear()
        {
            _allLogs.Clear();

            Application.Current.Dispatcher.Invoke(() =>
            {
                UiLogs.Clear();
            });
        }

        // Exports logs to a CSV file
        public void ExportCsv(string filePath)
        {
            // Use StringBuilder for efficient string concatenation
            var sb = new StringBuilder();
            sb.AppendLine("ActivityLogEntry");

            // Escape double quotes in log entries for CSV format
            foreach (var log in _allLogs)
            {
                string safe = log.Replace("\"", "\"\"");
                sb.AppendLine($"\"{safe}\"");
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}