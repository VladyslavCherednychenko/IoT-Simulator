namespace SimulatorApp.Core.Models;

public class StateChangeLog
{
    public long StateChangeLogId { get; set; }

    public long SensorId { get; set; }

    public bool IsTriggered { get; set; }

    public DateTime Timestamp { get; set; }

    public Sensor Sensor { get; set; } = default!;
}
