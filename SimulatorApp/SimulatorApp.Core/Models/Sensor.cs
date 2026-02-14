namespace SimulatorApp.Core.Models;

public class Sensor
{
    public long SensorId { get; set; }

    public long DeviceId { get; set; }

    public SensorType SensorType { get; set; }

    public string Name { get; set; } = default!;

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; }

    public Device Device { get; set; } = default!;

    public List<TelemetryLog> Telemetries { get; set; } = new();

    public List<StateChangeLog> StateChanges { get; set; } = new();

    public List<StatusChangeLog> StatusChanges { get; set; } = new();
}
