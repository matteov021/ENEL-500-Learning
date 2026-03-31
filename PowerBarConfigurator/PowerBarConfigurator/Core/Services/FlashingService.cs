using SharpCubeProgrammer;
using SharpCubeProgrammer.Enum;
using SharpCubeProgrammer.Struct;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Provides functionality for flashing firmware to STM32 devices using the CubeProgrammer API.
    /// 
    /// The FlashingService handles detection of DFU (Device Firmware Upgrade) devices,
    /// establishes a connection to the device bootloader, and performs firmware uploads.
    /// It includes progress reporting through callbacks and logs all key steps and errors
    /// to the application's activity log.
    /// 
    /// This service is designed to support reliable firmware updates with feedback for
    /// both success and failure scenarios.
    /// </summary>
    public class FlashingService
    {
        // Asynchronously flashes firmware to a connected DFU device.
        public async Task FlashFirmwareAsync(string firmwarePath)
        {
            await Task.Run(() => FlashInternal(firmwarePath));
        }

        // Internal method that performs the actual flashing logic using the CubeProgrammer API.
        private void FlashInternal(string firmwarePath)
        {
            var cubeProgrammerApi = new CubeProgrammerApi();

            // Set up callbacks to log flashing progress
            var callbacks = new DisplayCallBacks
            {
                LoadBar = (current, total) =>
                {
                    double percent = (double)current / total * 100;
                    AppServices.ActivityLog.Add($"Firmware flashing progress: {percent:0}%");
                }
            };

            // Register the callbacks with the CubeProgrammer API
            cubeProgrammerApi.SetDisplayCallbacks(callbacks);

            try
            {
                AppServices.ActivityLog.Add("Searching for DFU device...");

                // Get the list of connected DFU devices with the specified vendor and product IDs
                var usbList = cubeProgrammerApi.GetDfuDeviceList(0xdf11, 0x0483);

                // Check if any DFU devices were detected
                if (!usbList.Any())
                {
                    AppServices.ActivityLog.Add("Error: DFU device not detected.");
                    throw new Exception("No DFU device detected.");
                }

                // Use the first detected DFU device for flashing
                var dfuDevice = usbList.First();
                string usbIndex = dfuDevice.UsbIndex;

                AppServices.ActivityLog.Add($"DFU device detected (USB Index: {usbIndex}).");

                // Attempt to connect to the DFU bootloader on the detected device
                var connectFlag = cubeProgrammerApi.ConnectDfuBootloader(usbIndex);

                // Check if the connection was successful
                if (connectFlag != CubeProgrammerError.CubeprogrammerNoError)
                {
                    cubeProgrammerApi.Disconnect();
                    throw new Exception("Failed to connect to DFU bootloader.");
                }

                AppServices.ActivityLog.Add("Connected to DFU bootloader.");

                // Set flashing parameters
                uint verify = 1;
                uint skipErase = 0;
                string startAddress = "0x08000000";

                AppServices.ActivityLog.Add($"Flashing firmware: {firmwarePath}");

                // Attempt to download the firmware to the device
                var downloadFlag = cubeProgrammerApi.DownloadFile(
                    firmwarePath,
                    startAddress,
                    skipErase,
                    verify,
                    ""
                );

                // Check if the firmware download was successful
                if (downloadFlag != 0)
                {
                    cubeProgrammerApi.Disconnect();
                    cubeProgrammerApi.DeleteInterfaceList();

                    AppServices.ActivityLog.Add("Error: Firmware flashing failed.");
                    throw new Exception("Firmware flashing failed.");
                }

                // Disconnect and clean up resources after flashing
                cubeProgrammerApi.Disconnect();
                cubeProgrammerApi.DeleteInterfaceList();

                AppServices.ActivityLog.Add("Firmware flashing completed successfully.");
            }
            catch (Exception ex)
            {
                AppServices.ActivityLog.Add($"Error: Unexpected flashing error. {ex.Message}");
                throw;
            }
        }
    }
}