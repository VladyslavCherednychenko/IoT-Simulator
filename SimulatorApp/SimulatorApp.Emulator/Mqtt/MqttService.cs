using MQTTnet;
using MQTTnet.Client;
using SimulatorApp.Core.Extensions;
using SimulatorApp.Emulator.Models;
using System.Text.Json;

namespace SimulatorApp.Emulator.Mqtt;

public class MqttService : IMqttService, IAsyncDisposable
{
    private readonly ILogger<MqttService> _logger;
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public bool IsConnected => _client.IsConnected;

    public MqttService(ILogger<MqttService> logger, IConfiguration config)
    {
        _logger = logger;

        var host = config["Mqtt:Host"] ?? "localhost";
        var port = int.Parse(config["Mqtt:Port"] ?? "1883");

        _client = new MqttFactory().CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId("simulator")
            .WithWillTopic("simulator/status")
            .WithWillPayload(JsonSerializer.Serialize(new { status = "offline" }))
            .WithWillRetain(true)
            .Build();

        _client.DisconnectedAsync += args =>
        {
            _logger.LogWarning("MQTT disconnected: {Reason}", args.Reason);
            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync(CancellationToken ct)
    {
        await _client.ConnectAsync(_options, ct);
        _logger.LogInformation("Connected to MQTT broker.");
    }

    public async Task ReconnectIfNeededAsync(CancellationToken ct)
    {
        if (_client.IsConnected) return;

        _logger.LogInformation("Attempting MQTT reconnect...");
        await _client.ConnectAsync(_options, ct);
        _logger.LogInformation("Reconnected to MQTT broker.");
    }

    public async Task DisconnectAsync(CancellationToken ct)
    {
        if (_client.IsConnected)
            await _client.DisconnectAsync(cancellationToken: ct);
    }

    public async Task PublishSensorTelemetryAsync(VirtualDevice device, VirtualSensor sensor, string payload, bool retain, CancellationToken ct)
    {
        var topic = $"home/{device.Location.ToSlugString()}/{device.MAC}/{sensor.SensorType}/telemetry";
        await PublishAsync(topic, payload, retain, ct);
    }

    public async Task PublishSensorStatusAsync(VirtualDevice device, VirtualSensor sensor, string status, CancellationToken ct)
    {
        var topic = $"home/{device.Location.ToSlugString()}/{device.MAC}/{sensor.SensorType}/status";
        await PublishAsync(topic, JsonSerializer.Serialize(new { status }), retain: true, ct);
    }

    private async Task PublishAsync(string topic, string payload, bool retain, CancellationToken ct)
    {
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .Build();
        await _client.PublishAsync(msg, ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }

        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
