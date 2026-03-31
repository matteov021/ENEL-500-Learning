using NModbus.IO;
using System.IO;
using System.IO.Ports;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Adapts a SerialPort to the IStreamResource interface required by NModbus.
    /// 
    /// The SerialPortStreamAdapter provides a stream-like abstraction over a SerialPort,
    /// enabling it to be used with Modbus communication libraries. It supports synchronous
    /// and asynchronous read/write operations, buffer management, timeout configuration,
    /// and safe resource disposal.
    /// 
    /// The adapter includes error handling to ensure consistent exception behavior and
    /// helps maintain reliable serial communication with external devices.
    /// </summary>
    public class SerialPortStreamAdapter : IStreamResource
    {
        // Wraps a SerialPort to provide a stream-like interface for reading and writing data.
        private readonly SerialPort _serialPort;

        // Initializes the adapter with the specified SerialPort instance.
        public SerialPortStreamAdapter(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        // Represents an infinite timeout value for read/write operations.
        public int InfiniteTimeout => Timeout.Infinite;

        // Discards any data in the input buffer of the serial port.
        public void DiscardInBuffer()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.DiscardInBuffer();
            }
            catch { }
        }

        // Discards any data in the output buffer of the serial port
        public void DiscardOutBuffer()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.DiscardOutBuffer();
            }
            catch { }
        }

        // Gets or sets the read timeout for the serial port. A value of InfiniteTimeout means no timeout.
        public int ReadTimeout
        {
            get => _serialPort.ReadTimeout;
            set => _serialPort.ReadTimeout = value;
        }

        // Gets or sets the write timeout for the serial port. A value of InfiniteTimeout means no timeout.
        public int WriteTimeout
        {
            get => _serialPort.WriteTimeout;
            set => _serialPort.WriteTimeout = value;
        }

        // Disposes the serial port resource, ensuring that it is properly closed and any resources are released.
        public void Dispose()
        {
            try
            {
                _serialPort?.Dispose();
            }
            catch { }
        }

        // Reads data from the serial port into the provided buffer.
        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (!_serialPort.IsOpen)
                    throw new IOException("Serial port closed.");

                return _serialPort.Read(buffer, offset, count);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is InvalidOperationException ||
                ex is ObjectDisposedException)
            {
                throw new IOException("Serial port read failure.", ex);
            }
        }

        // Writes data to the serial port from the provided buffer.
        public void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                if (!_serialPort.IsOpen)
                    throw new IOException("Serial port closed.");

                _serialPort.Write(buffer, offset, count);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is InvalidOperationException ||
                ex is ObjectDisposedException)
            {
                throw new IOException("Serial port write failure.", ex);
            }
        }

        // Asynchronously reads data from the serial port into the provided buffer.
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                if (!_serialPort.IsOpen)
                    throw new IOException("Serial port closed.");

                return await _serialPort.BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new IOException("Serial async read failure.", ex);
            }
        }

        // Asynchronously writes data to the serial port from the provided buffer.
        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                if (!_serialPort.IsOpen)
                    throw new IOException("Serial port closed.");

                await _serialPort.BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new IOException("Serial async write failure.", ex);
            }
        }

        // Flushes the output buffer of the serial port, ensuring that all data is sent to the device
        public void Flush()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.BaseStream.Flush();
            }
            catch { }
        }

        // Closes the serial port connection, ensuring that it is properly closed and any resources are released
        public void Close()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch { }
        }

        // Gets a value indicating whether the serial port is currently open and available for communication
        public bool IsOpen => _serialPort?.IsOpen ?? false;
    }
}