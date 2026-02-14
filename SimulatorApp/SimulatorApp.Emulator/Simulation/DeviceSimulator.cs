using SimulatorApp.Core.Enums;
using SimulatorApp.Emulator.Models;
using SimulatorApp.Emulator.Mqtt;
using SimulatorApp.Emulator.Services;

namespace SimulatorApp.Emulator.Simulation;

public class DeviceSimulator(ILogger<DeviceSimulator> logger, IMqttService mqttService)
{
    public IEnumerable<Task> StartAll(IEnumerable<VirtualDevice> devices, CancellationToken ct)
    {
        return devices.SelectMany(d => d.Sensors.Select(s => RunSensorLoopAsync(d, s, ct)));
    }

    public async Task SetAllStatusesAsync(IEnumerable<VirtualDevice> devices, string status, CancellationToken ct)
    {
        foreach (var device in devices)
        {
            foreach (var sensor in device.Sensors)
            {
                await mqttService.PublishSensorStatusAsync(device, sensor, status, ct);
            }
        }
    }

    private async Task RunSensorLoopAsync(VirtualDevice device, VirtualSensor sensor, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (mqttService.IsConnected)
            {
                try
                {
                    var payload = TelemetryGenerator.GenerateTelemetryPayload(sensor);
                    logger.LogInformation("Publishing {Type} for {Mac}", sensor.SensorType, device.MAC);

                    // Logic for "Motion" (Only if state changed)
                    if (sensor.SensorType == SensorType.Motion)
                    {
                        if (payload != (string?)sensor.LastValue)
                        {
                            await mqttService.PublishSensorTelemetryAsync(device, sensor, payload, true, ct);
                            sensor.LastValue = payload;
                        }
                    }
                    else
                    {
                        await mqttService.PublishSensorTelemetryAsync(device, sensor, payload, false, ct);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Sensor {Name} failed", sensor.Name);
                }
            }

            var delay = sensor.UpdateInterval ?? TimeSpan.FromSeconds(1);
            await Task.Delay(delay, ct);
        }
    }
}
