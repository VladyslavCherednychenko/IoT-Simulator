using SimulatorApp.Core.Enums;
using SimulatorApp.Emulator.Models;
using System.Text.Json;

namespace SimulatorApp.Emulator.Services;

public static class TelemetryGenerator
{
    private static readonly Random _rng = new();

    public static string GenerateTelemetryPayload(VirtualSensor sensor)
    {
        var data = new Dictionary<string, object>();

        if (sensor.SensorType is SensorType.Motion)
        {
            var state = _rng.Next(0, 101) < 7;
            data.Add(sensor.SensorType.ToString(), state);
        }
        else
        {
            var value = Math.Round((_rng.NextDouble() * (sensor.EmulatorMaxRNG - sensor.EmulatorMinRNG)) + sensor.EmulatorMinRNG, 1);
            data.Add(sensor.SensorType.ToString(), value);
        }

        return JsonSerializer.Serialize(data);
    }
}
