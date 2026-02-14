using SimulatorApp.Emulator.Models;

namespace SimulatorApp.Emulator.Mqtt;

public interface IMqttService
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken ct);

    Task ReconnectIfNeededAsync(CancellationToken ct);

    Task DisconnectAsync(CancellationToken ct);

    Task PublishSensorTelemetryAsync(VirtualDevice device, VirtualSensor sensor, string payload, bool retain, CancellationToken ct);

    Task PublishSensorStatusAsync(VirtualDevice device, VirtualSensor sensor, string status, CancellationToken ct);
}
