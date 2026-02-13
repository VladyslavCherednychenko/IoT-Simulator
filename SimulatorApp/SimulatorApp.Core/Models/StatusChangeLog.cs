namespace SimulatorApp.Core.Models;

public class StatusChangeLog
{
    public long StatusChangeLogId { get; set; }

    public long SensorId { get; set; }

    public bool IsOnline { get; set; }

    public DateTime Timestamp { get; set; }

    public Sensor Sensor { get; set; } = default!;
}
