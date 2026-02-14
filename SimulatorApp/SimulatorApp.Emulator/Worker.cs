using SimulatorApp.Emulator.Models;
using SimulatorApp.Emulator.Mqtt;
using SimulatorApp.Emulator.Simulation;

namespace SimulatorApp.Emulator;

public class Worker(ILogger<Worker> logger, IMqttService mqttService, DeviceSimulator deviceSimulator) : BackgroundService
{
    private readonly List<VirtualDevice> _devices = DeviceRegistry.GetDevices();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await mqttService.ConnectAsync(stoppingToken);
            await deviceSimulator.SetAllStatusesAsync(_devices, "online", stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to MQTT broker on startup.");
        }

        var sensorTasks = deviceSimulator.StartAll(_devices, stoppingToken).ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await mqttService.ReconnectIfNeededAsync(stoppingToken);

                if (mqttService.IsConnected)
                {
                    await deviceSimulator.SetAllStatusesAsync(_devices, "online", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reconnection attempt failed.");
            }

            await Task.Delay(10000, stoppingToken);
        }

        await deviceSimulator.SetAllStatusesAsync(_devices, "offline", stoppingToken);
        await mqttService.DisconnectAsync(stoppingToken);
        await Task.WhenAll(sensorTasks);
    }
}
