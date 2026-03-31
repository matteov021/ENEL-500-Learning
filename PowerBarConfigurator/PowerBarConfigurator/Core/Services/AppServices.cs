using PowerBarConfigurator.Core.Services;

namespace PowerBarConfigurator.Core
{
    /// <summary>
    /// Provides a centralized access point for core application services.
    /// 
    /// The AppServices class exposes singleton instances of key services used
    /// throughout the application, including connection management, power control,
    /// activity logging, data visualization, data logging, and firmware flashing.
    /// 
    /// This static service locator simplifies dependency access across the application,
    /// allowing different components to share and interact with common services.
    /// </summary>
    public static class AppServices
    {
        // The ConnectionService is responsible for managing connections to the device.
        public static ConnectionService Connection { get; } = new ConnectionService();

        // The PowerService is responsible for managing power-related operations.
        public static PowerService Power { get; } = new PowerService();

        // The ActivityLogService provides a logging mechanism for application activities.
        public static ActivityLogService ActivityLog { get; } = new ActivityLogService();

        // The GraphService manages graph-related functionalities.
        public static GraphService Graphs { get; } = new GraphService();

        // The DataLoggingService handles the logging of data points.
        public static DataLoggingService DataLogger { get; } = new();

        // The FlashingService manages firmware flashing operations.
        public static FlashingService Flashing { get; } = new();
    }
}