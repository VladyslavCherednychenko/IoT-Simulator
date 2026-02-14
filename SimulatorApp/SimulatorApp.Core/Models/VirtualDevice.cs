namespace SimulatorApp.Core.Models;

public class VirtualDevice
{
    public string MAC { get; set; } = default!;

    public string Model { get; set; } = default!;

    public string Location { get; set; } = default!;

    public List<VirtualSensor> Sensors { get; set; } = [];
}
