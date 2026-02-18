using SimulatorApp.Core.Enums;

namespace SimulatorApp.Core.Models;

public class AlertRule
{
    public long AlertRuleId { get; set; }

    public long SensorId { get; set; }

    public AlertType AlertType { get; set; }

    public double? RangeMin { get; set; }

    public double? RangeMax { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public Sensor Sensor { get; set; } = default!;
}
