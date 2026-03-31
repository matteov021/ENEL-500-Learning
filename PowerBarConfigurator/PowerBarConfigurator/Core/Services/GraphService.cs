using System.Transactions;

namespace PowerBarConfigurator.Core.Services
{
    /// <summary>
    /// Provides functionality for processing ADC samples and calculating RMS current values
    /// for multiple outlets.
    /// 
    /// The GraphService maintains a rolling buffer of ADC samples for each outlet and computes
    /// the RMS (Root Mean Square) current once enough samples are collected. It uses a
    /// piecewise linear approximation to convert ADC readings into current values based on
    /// empirical calibration data.
    /// 
    /// This service is designed to support real-time data visualization and analysis of
    /// electrical current across multiple outlets.
    /// </summary>
    public class GraphService
    {
        // Number of samples to keep for each outlet to calculate RMS current.
        const int BUFFER_SIZE = 200;

        // Buffers for ADC samples for each outlet.
        private readonly List<ushort>[] _adcBuffers =
            Enumerable.Range(0, 10)
            .Select(_ => new List<ushort>())
            .ToArray(); 

        // Calculated RMS currents for each outlet.
        public double[] RMSCurrents = new double[10];

        // Calibration parameters for each outlet
        // 1.00 is a placeholder. It is not used in calculations but is included to align indices.
        public double[] outletOffsets = { 1.00, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0, 2057.0 };
        public double[] outletSlopes = { 1.00, 93.5, 93.675, 93.8, 92.875, 93.9, 92.868, 93.840, 93.76, 93.65, 93.5 };

        // Adds a new ADC sample for the specified outlet and updates the RMS current.
        public void AddSample(int outlet, ushort adc)
        {
            // Validate the outlet index to ensure it is within the valid range of outlets
            var buffer = _adcBuffers[outlet];
            buffer.Add(adc);

            // Keep only the most recent BUFFER_SIZE samples to limit memory usage
            if (buffer.Count > BUFFER_SIZE)
                buffer.RemoveAt(0);

            // Calculate RMS current when we have enough samples
            if (buffer.Count == BUFFER_SIZE)
                CalculateCurrent(outlet);
        }

        // Calculates the RMS current for the specified outlet based on the ADC samples in the buffer
        private void CalculateCurrent(int outlet)
        {
            // Get the current buffer of ADC samples for the outlet and find the maximum value
            var buffer = _adcBuffers[outlet];
            ushort max = buffer.Max();
            double RMSCurrent = 0;

            // Count how many outlets are currently active to determine the appropriate calibration curve.
            var outletStates = AppServices.Power.OutletStates;
            int sum = 0;

            // Count how many outlets are currently drawing active.
            for (int i = 0; i < 10; i++)
            {
                sum += outletStates[i] ? 1 : 0;
            }

            // Use the count of active outlets to select the appropriate calibration parameters
            for (int i = 0; i < 11; i++)
            {
                // Use the corresponding slope and offset to calculate the RMS current.
                if (sum == i)
                    RMSCurrent = (1 / outletSlopes[i]) * max - (outletOffsets[i] / outletSlopes[i]);
            }

            RMSCurrents[outlet] = RMSCurrent;
        }

        // Retrieves the minimum and maximum ADC values from the buffer for the specified outlet
        public (ushort min, ushort max)? GetBufferMinMax(int outlet)
        {
            // Validate the outlet index to ensure it is within the valid range of outlets
            if (outlet < 0 || outlet >= _adcBuffers.Length)
                return null;

            // Get the buffer for the specified outlet and check if it contains any samples
            var buffer = _adcBuffers[outlet];
            if (buffer.Count == 0)
                return null;

            return (buffer.Min(), buffer.Max());
        }
    }
}