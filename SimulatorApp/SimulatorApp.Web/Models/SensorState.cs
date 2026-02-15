using SimulatorApp.Core.Enums;

namespace SimulatorApp.Web.Models;

public class SensorState
{
    public SensorType SensorType { get; set; }
    public string Name { get; set; } = default!;
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }

    // Latest telemetry value (null if not yet received)
    public double? LastValue { get; set; }
    public Metric? LastMetric { get; set; }

    // Latest motion trigger state
    public bool? LastTriggered { get; set; }
}
