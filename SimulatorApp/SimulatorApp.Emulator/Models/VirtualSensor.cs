using SimulatorApp.Core.Enums;

namespace SimulatorApp.Emulator.Models;

public class VirtualSensor
{
    public SensorType SensorType { get; set; } = default!;

    public string Name { get; set; } = default!;

    // Controls to Emulate Device
    public double EmulatorMinRNG { get; set; } = 0;

    public double EmulatorMaxRNG { get; set; } = 0;

    public TimeSpan? UpdateInterval { get; set; }

    public object? LastValue { get; set; } // For "state-change" logic
}
