using SimulatorApp.Core.Enums;
using SimulatorApp.Emulator.Models;

namespace SimulatorApp.Emulator.Simulation;

public static class DeviceRegistry
{
    public static List<VirtualDevice> GetDevices() =>
    [
        new VirtualDevice
        {
            MAC = "F3D24C8D0EA5", Model = "Climate Sensor x1402abc", Location = "livingroom",
            Sensors = [
                new() { SensorType = SensorType.Temperature, Name = "Temperature Sensor", EmulatorMinRNG = 16, EmulatorMaxRNG = 22, UpdateInterval = TimeSpan.FromSeconds(15)},
                new() { SensorType = SensorType.Humidity, Name = "Humidity Sensor", EmulatorMinRNG = 40, EmulatorMaxRNG = 80, UpdateInterval = TimeSpan.FromSeconds(30)},
                new() { SensorType = SensorType.SoC, Name = "Battery Level Sensor", EmulatorMinRNG = 0, EmulatorMaxRNG = 100, UpdateInterval = TimeSpan.FromMinutes(1)}
            ]
        },
        new VirtualDevice
        {
            MAC = "56699D64012D", Model = "Motion Sensor xyz123", Location = "backyard",
            Sensors = [
                new() { SensorType = SensorType.Motion, Name = "Motion Sensor"}
            ]
        },
        new VirtualDevice
        {
            MAC = "3A2CE01FE222", Model = "Unusual Sensor t900", Location = "secret room",
            Sensors = [
                new() { SensorType = SensorType.Resistance, Name = "Resistance Sensor", EmulatorMinRNG = 1000, EmulatorMaxRNG = 10000, UpdateInterval = TimeSpan.FromSeconds(10)},
                new() { SensorType = SensorType.Pressure, Name = "Pressure Sensor", EmulatorMinRNG = 1000, EmulatorMaxRNG = 10000, UpdateInterval = TimeSpan.FromSeconds(13)},
                new() { SensorType = SensorType.CO2, Name = "CO2 Sensor", EmulatorMinRNG = 1000, EmulatorMaxRNG = 10000, UpdateInterval = TimeSpan.FromSeconds(17)}
            ]
        },
    ];
}
