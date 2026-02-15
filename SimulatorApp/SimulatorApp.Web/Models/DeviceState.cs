using SimulatorApp.Core.Enums;

namespace SimulatorApp.Web.Models;

public class DeviceState
{
    public string MAC { get; set; } = default!;
    public string Model { get; set; } = default!;
    public string Location { get; set; } = default!;
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public Dictionary<SensorType, SensorState> Sensors { get; set; } = [];
}
