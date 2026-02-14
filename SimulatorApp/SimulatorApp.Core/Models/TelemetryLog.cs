using SimulatorApp.Core.Enums;

namespace SimulatorApp.Core.Models;

public class TelemetryLog
{
    public long TelemetryLogId { get; set; }

    public long SensorId { get; set; }

    public Metric Metric { get; set; }

    public double Value { get; set; }

    public DateTime Timestamp { get; set; }

    public Sensor Sensor { get; set; } = default!;
}
