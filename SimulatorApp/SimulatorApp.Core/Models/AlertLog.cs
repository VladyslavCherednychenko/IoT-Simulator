namespace SimulatorApp.Core.Models;

public class AlertLog
{
    public long AlertLogId { get; set; }

    public long AlertRuleId { get; set; }

    public long SensorId { get; set; }

    public double? TriggerValue { get; set; }

    public bool IsRead { get; set; }

    public DateTime Timestamp { get; set; }

    public Sensor Sensor { get; set; } = default!;

    public AlertRule AlertRule { get; set; } = default!;
}
