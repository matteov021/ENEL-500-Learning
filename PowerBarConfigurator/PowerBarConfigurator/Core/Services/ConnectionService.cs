using PowerBarConfigurator.Core.Models;
using NModbus;
using System.IO.Ports;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Manages serial communication with a Modbus-enabled STM32 device.
    /// 
    /// The ConnectionService is responsible for discovering and connecting to the device
    /// over available serial ports, maintaining the Modbus RTU master, and handling
    /// thread-safe read/write operations. It also provides a continuous polling loop
    /// to retrieve real-time data from the device and propagate updates to subscribers.
    /// 
    /// The service exposes connection state changes, incoming data events, and includes
    /// error handling with automatic disconnection on communication failures to ensure
    /// system stability.
    /// 
    /// Additionally, it provides utility methods for converting between floating-point
    /// values and Modbus register representations.
    /// </summary>
    public class ConnectionService
    {
        // Internal fields for managing the serial port and Modbus master
        private SerialPort? _serialPort;
        private IModbusSerialMaster? _modbusMaster;

        // Public properties to expose the Modbus master and connection state
        public IModbusSerialMaster? ModbusMaster => _modbusMaster;

        // The current connection state of the service
        public ConnectionState CurrentState { get; private set; } = ConnectionState.Disconnected;

        // Events to notify subscribers of data reception and connection state changes
        public event Action<string>? DataReceived;
        public event Action<ConnectionState>? ConnectionStateChanged;

        // A lock object to ensure thread-safe access to Modbus operations
        private readonly object _modbusLock = new();

        // Cancellation token source and task for managing the polling loop
        private CancellationTokenSource? _pollCts;
        private Task? _pollTask;

        // Attempt to connect to the STM32 device over serial ports
        public async Task ConnectAsync(int baudRate = 48000, byte slaveId = 1)
        {
            // If already connected, do nothing
            if (CurrentState == ConnectionState.Connected)
                return;

            CurrentState = ConnectionState.Connecting;
            ConnectionStateChanged?.Invoke(CurrentState);

            await Task.Delay(1000);

            string[] ports = SerialPort.GetPortNames();

            // Try each available port to find the STM32 device
            foreach (var port in ports)
            {
                try
                {
                    var serial = new SerialPort(port)
                    {
                        BaudRate = baudRate,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        ReadTimeout = 1000,
                        WriteTimeout = 1000
                    };

                    serial.Open();

                    // Test communication by trying to read a register
                    var adapter = new SerialPortStreamAdapter(serial);
                    var factory = new ModbusFactory();
                    var master = factory.CreateRtuMaster(adapter);

                    master.Transport.ReadTimeout = 1000;

                    try
                    {
                        master.ReadHoldingRegisters(slaveId, 0, 1);
                    }
                    catch
                    {
                        serial.Close();
                        continue;
                    }

                    // If successful, store the serial port and Modbus master
                    _serialPort = serial;
                    _modbusMaster = master;

                    // Update state and start polling loop
                    CurrentState = ConnectionState.Connected;
                    ConnectionStateChanged?.Invoke(CurrentState);

                    // Stop any previous polling
                    _pollCts?.Cancel();

                    // Start a new polling loop to read data periodically
                    _pollCts = new CancellationTokenSource();
                    _pollTask = Task.Run(() => PollLoop(slaveId, _pollCts.Token));

                    return;
                }
                catch
                {
                    try 
                    { 
                        _serialPort?.Close(); 
                    } 
                    catch 
                    {
                        // Ignore close errors
                    }
                }
            }

            // If we reach here, no compatible device was found
            CurrentState = ConnectionState.Disconnected;
            ConnectionStateChanged?.Invoke(CurrentState);

            throw new Exception("No compatible STM32 Modbus device was found.");
        }

        // Disconnect and clean up resources
        public void Disconnect()
        {
            CurrentState = ConnectionState.Disconnected;

            try
            {
                // Stop polling
                _pollCts?.Cancel();

                try
                {
                    _pollTask?.Wait(500);
                }
                catch { }

                // Clean up polling resources
                _pollCts = null;
                _pollTask = null;

                // Close serial port
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                }

                _serialPort?.Dispose();
            }
            catch
            {
                // Ignore cleanup errors
            }

            // Clear references to allow GC
            _modbusMaster = null;
            ConnectionStateChanged?.Invoke(CurrentState);
        }

        // Thread-safe read
        public bool TryReadHoldingRegisters(byte slaveId, ushort address, ushort count, out ushort[] registers)
        {
            // Initialize output parameter
            registers = Array.Empty<ushort>();
            bool shouldDisconnect = false;

            // Lock to ensure thread-safe access to Modbus operations
            lock (_modbusLock)
            {
                if (_modbusMaster == null || _serialPort == null || !_serialPort.IsOpen)
                    return false;

                try
                {
                    registers = _modbusMaster.ReadHoldingRegisters(slaveId, address, count);
                    return true;
                }
                catch (Exception ex)
                {
                    AppServices.ActivityLog.Add($"Error: Modbus read failed. {ex.Message}");
                    shouldDisconnect = true;
                }
            }

            // If an error occurred during Modbus communication, disconnect to reset the state
            if (shouldDisconnect)
                Disconnect();

            return false;
        }

        // Thread-safe single register write
        public bool TryWriteSingleRegister(byte slaveId, ushort address, ushort value)
        {
            // Flag to determine if we should disconnect after an error
            bool shouldDisconnect = false;

            // Lock to ensure thread-safe access to Modbus operations
            lock (_modbusLock)
            {
                if (_modbusMaster == null || _serialPort == null || !_serialPort.IsOpen)
                    return false;

                try
                {
                    _modbusMaster.WriteSingleRegister(slaveId, address, value);
                    return true;
                }
                catch (Exception ex)
                {
                    AppServices.ActivityLog.Add($"Error: Modbus write failed. {ex.Message}");
                    shouldDisconnect = true;
                }
            }

            // If an error occurred during Modbus communication, disconnect to reset the state
            if (shouldDisconnect)
                Disconnect();

            return false;
        }

        // Thread-safe multiple register write
        public bool TryWriteMultipleRegisters(byte slaveId, ushort address, ushort[] values)
        {
            // Flag to determine if we should disconnect after an error
            bool shouldDisconnect = false;

            // Lock to ensure thread-safe access to Modbus operations
            lock (_modbusLock)
            {
                if (_modbusMaster == null || _serialPort == null || !_serialPort.IsOpen)
                    return false;

                try
                {
                    _modbusMaster.WriteMultipleRegisters(slaveId, address, values);
                    return true;
                }
                catch (Exception ex)
                {
                    AppServices.ActivityLog.Add($"Error: Modbus write failed. {ex.Message}");
                    shouldDisconnect = true;
                }
            }

            // If an error occurred during Modbus communication, disconnect to reset the state
            if (shouldDisconnect)
                Disconnect();

            return false;
        }

        // Utility methods to combine and split float values into two ushort registers
        public static float CombineFloat(ushort high, ushort low)
        {
            byte[] bytes = new byte[4];

            // Combine the high and low ushort values into a byte array
            bytes[0] = (byte)(high >> 8);
            bytes[1] = (byte)(high & 0xFF);
            bytes[2] = (byte)(low >> 8);
            bytes[3] = (byte)(low & 0xFF);

            return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
        }

        // Split a float value into two ushort registers (high and low)
        public static (ushort high, ushort low) SplitFloat(float value)
        {
            // Convert the float value to a byte array and reverse it for Modbus big-endian format
            byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            ushort high = (ushort)((bytes[0] << 8) | bytes[1]);
            ushort low = (ushort)((bytes[2] << 8) | bytes[3]);

            return (high, low);
        }

        // The main polling loop that continuously reads data from the device and updates the UI
        private async Task PollLoop(byte slaveId, CancellationToken token)
        {
            try
            {
                // Continuously read data until cancellation is requested
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1, token);

                    // Attempt to read holding registers from the device
                    if (!TryReadHoldingRegisters(slaveId, 0, 43, out ushort[] registers))
                        continue;

                    // Update graphs with the latest ADC values from the registers
                    for (int i = 0; i < 10; i++)
                    {
                        ushort adc = registers[10 + i];
                        AppServices.Graphs.AddSample(i, adc);
                    }

                    // Notify subscribers of the new data by invoking the DataReceived event.
                    DataReceived?.Invoke(string.Join(",", registers));
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on disconnect so ignore
            }
            catch (Exception ex)
            {
                AppServices.ActivityLog.Add($"PollLoop error: {ex.Message}");
            }
        }
    }
}