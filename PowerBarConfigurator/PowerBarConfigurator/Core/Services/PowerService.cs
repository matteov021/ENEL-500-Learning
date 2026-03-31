using PowerBarConfigurator.Core.Models;

namespace PowerBarConfigurator.Core.Services 
{
    /// <summary>
    /// Manages and tracks the power state of multiple outlets within the system.
    /// 
    /// The PowerService maintains the on/off state of each outlet and determines
    /// the overall system power state (Idle or Active) based on whether any outlet
    /// is currently drawing power. It updates its state from incoming register data
    /// and notifies subscribers when the overall power state changes.
    /// 
    /// This service is intended to provide a centralized representation of outlet
    /// activity for use in UI updates, logic control, and system monitoring.
    /// </summary>
    public class PowerService
    {
        // Represents the current power state of the system, which can be either Idle or Active.
        public PowerState CurrentState { get; private set; } = PowerState.Idle;

        // An array representing the on/off state of each outlet.
        public bool[] OutletStates { get; private set; } = new bool[10];

        // An event that is triggered whenever the power state changes.
        public event Action<PowerState>? PowerStateChanged;

        // Updates the outlet states based on the provided register values.
        public void UpdateFromRegisters(ushort[] registers)
        {
            // Update the outlet states based on the register values.
            for (int i = 0; i < 10 && i < registers.Length; i++)
            {
                OutletStates[i] = registers[i] > 0;
            }

            // Determine the new power state based on the outlet states.
            bool anyActive = OutletStates.Any(o => o);
            var newState = anyActive ? PowerState.Active : PowerState.Idle;

            // If the new state is different from the current state.
            if (newState != CurrentState)
            {
                CurrentState = newState;
                PowerStateChanged?.Invoke(CurrentState);
            }
        }
    }
}