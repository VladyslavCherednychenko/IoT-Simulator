namespace SimulatorApp.Core.Models;

public class Device
{
    public long DeviceId { get; set; }

    public string Model { get; set; } = default!;

    public string Location { get; set; } = default!;

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; }

    public List<Sensor> Sensors { get; set; } = new();
}
