using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MQTTnet;
using MQTTnet.Client;
using SimulatorApp.Core.Enums;
using SimulatorApp.Core.Models;
using SimulatorApp.Infrastructure.Data;
using SimulatorApp.Web.Helpers;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SimulatorApp.Web.Services;

public class MqttListenerService : BackgroundService
{
    private readonly ILogger<MqttListenerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly DashboardStateService _dashboardState;
    private readonly AlertEvaluatorService _alertEvaluator;

    // Pending records carry raw values + MAC context
    // EF entities are only created during flush once SensorId is resolved
    private sealed record PendingTelemetry(string MAC, SensorType SensorType, Metric Metric, double Value, DateTime Timestamp);
    private sealed record PendingStatus(string MAC, SensorType SensorType, bool IsOnline, DateTime Timestamp);
    private sealed record PendingState(string MAC, SensorType SensorType, bool IsTriggered, DateTime Timestamp);

    private readonly ConcurrentQueue<PendingTelemetry> _telemetryQueue = new();
    private readonly ConcurrentQueue<PendingStatus> _statusQueue = new();
    private readonly ConcurrentQueue<PendingState> _stateQueue = new();
    private readonly ConcurrentQueue<AlertLog> _alertLogQueue = new();

    // Cache (MAC + SensorType) -> LastStatus
    // to avoid repeated DB lookups per flush and store only status transitions
    private readonly Dictionary<(string MAC, SensorType), bool> _lastStatusCache = [];

    private const string TopicFilter = "#";
    private const string HomePrefix = "home/";
    private const string SimulatorStatusTopic = "simulator/status";
    private const int FlushIntervalMs = 10_000;

    public MqttListenerService(
        ILogger<MqttListenerService> logger,
        IConfiguration config,
        IServiceScopeFactory scopeFactory,
        DashboardStateService dashboardState,
        AlertEvaluatorService alertEvaluator)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _dashboardState = dashboardState;
        _alertEvaluator = alertEvaluator;

        var host = config["Mqtt:Host"] ?? "localhost";
        var port = int.Parse(config["Mqtt:Port"] ?? "1883");

        _client = new MqttFactory().CreateMqttClient();
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId("observer")
            .Build();

        _client.DisconnectedAsync += args =>
        {
            _logger.LogWarning("MQTT disconnected: {Reason}", args.Reason);
            return Task.CompletedTask;
        };

        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    // Main loop
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectAndSubscribeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    _logger.LogInformation("Attempting MQTT reconnect...");
                    await ConnectAndSubscribeAsync(stoppingToken);
                }

                await FlushAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in listener loop.");
            }

            // Flush every 10s
            await Task.Delay(FlushIntervalMs, stoppingToken);
        }

        if (_client.IsConnected)
        {
            await _client.DisconnectAsync(cancellationToken: stoppingToken);
        }
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken ct)
    {
        await _client.ConnectAsync(_options, ct);
        await _client.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic(TopicFilter)
            .Build(), ct);
        _logger.LogInformation("Connected and subscribed to MQTT broker.");
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = args.ApplicationMessage.ConvertPayloadToString();

        try
        {
            if (topic == SimulatorStatusTopic)
            {
                _logger.LogInformation("Simulator status: {Payload}", payload);
                HandleSimulatorStatus(payload);
                return Task.CompletedTask; ;
            }

            if (!topic.StartsWith(HomePrefix))
            {
                return Task.CompletedTask; ;
            }

            var parsed = TopicParser.TryParse(topic, _logger);
            if (parsed is null)
            {
                return Task.CompletedTask;
            }

            switch (parsed.MessageType)
            {
                case "telemetry":
                    HandleTelemetry(parsed.MAC, parsed.Location, parsed.SensorType, payload);
                    break;
                case "status":
                    HandleSensorStatus(parsed.MAC, parsed.Location, parsed.SensorType, payload);
                    break;
                default:
                    _logger.LogWarning("Unknown message type: {Type}", parsed.MessageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message on topic {Topic}", topic);
        }

        return Task.CompletedTask;
    }

    private void HandleTelemetry(string mac, string location, SensorType sensorType, string payload)
    {
        if (sensorType == SensorType.Motion)
        {
            var triggered = PayloadParser.TryParseMotion(payload);
            if (triggered is not null)
            {
                _dashboardState.UpdateTrigger(mac, location, sensorType, triggered.Value);
                _stateQueue.Enqueue(new PendingState(mac, sensorType, triggered.Value, DateTime.UtcNow));
            }
            return;
        }

        var telemetry = PayloadParser.TryParseTelemetry(sensorType, payload, _logger);
        if (telemetry is not null)
        {
            _dashboardState.UpdateTelemetry(mac, location, sensorType, telemetry.Value.Metric, telemetry.Value.Value);
            _telemetryQueue.Enqueue(new PendingTelemetry(mac, sensorType, telemetry.Value.Metric, telemetry.Value.Value, DateTime.UtcNow));
        }
    }

    private void HandleSensorStatus(string mac, string location, SensorType sensorType, string payload)
    {
        var isOnline = PayloadParser.TryParseStatus(payload);

        if (isOnline is null)
        {
            return;
        }

        var key = (mac, sensorType);
        if (_lastStatusCache.TryGetValue(key, out var last) && last == isOnline.Value)
        {
            return;
        }

        _lastStatusCache[key] = isOnline.Value;
        _dashboardState.UpdateSensorStatus(mac, location, sensorType, isOnline.Value);
        _statusQueue.Enqueue(new PendingStatus(mac, sensorType, isOnline.Value, DateTime.UtcNow));
    }

    private void HandleSimulatorStatus(string payload)
    {
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);
        if (json?.TryGetValue("status", out var el) == true)
        {
            _dashboardState.UpdateSimulatorStatus(el.GetString() == "online");
        }
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        if (_telemetryQueue.IsEmpty && _statusQueue.IsEmpty && _stateQueue.IsEmpty)
        {
            return;
        }

        var telemetryBatch = ConcurrentQueueHelper.DrainQueue(_telemetryQueue);
        var statusBatch = ConcurrentQueueHelper.DrainQueue(_statusQueue);
        var stateBatch = ConcurrentQueueHelper.DrainQueue(_stateQueue);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await FlushTelemetryAsync(db, telemetryBatch, ct);
            await FlushStatusAsync(db, statusBatch, ct);
            await FlushStateAsync(db, stateBatch, ct);
            await FlushAlertsAsync(db, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flush failed - re-enqueuing batch for retry.");

            // Re-enqueue entire batch (retry next cycle)
            // Note: alerts are NOT re-enqueued since they are derived from telemetry/status/state
            foreach (var item in telemetryBatch) _telemetryQueue.Enqueue(item);
            foreach (var item in statusBatch) _statusQueue.Enqueue(item);
            foreach (var item in stateBatch) _stateQueue.Enqueue(item);
        }
    }

    private async Task FlushTelemetryAsync(AppDbContext db, List<PendingTelemetry> batch, CancellationToken ct)
    {
        foreach (var pending in batch)
        {
            var sensorId = await SensorIdResolver.ResolveSensorIdAsync(db, pending.MAC, pending.SensorType, _logger, ct);

            // Skip if sensor is not registered yet
            if (sensorId is null)
            {
                continue;
            }

            await db.TelemetryLogs.AddAsync(new TelemetryLog
            {
                SensorId = sensorId.Value,
                Metric = pending.Metric,
                Value = pending.Value,
                Timestamp = pending.Timestamp
            }, ct);

            var alertLogs = _alertEvaluator.EvaluateTelemetry(sensorId.Value, pending.Value);
            foreach (var log in alertLogs)
            {
                _alertLogQueue.Enqueue(log);
            }
        }
    }

    private async Task FlushStatusAsync(AppDbContext db, List<PendingStatus> batch, CancellationToken ct)
    {
        foreach (var pending in batch)
        {
            var sensorId = await SensorIdResolver.ResolveSensorIdAsync(db, pending.MAC, pending.SensorType, _logger, ct);

            // Skip if sensor is not registered yet
            if (sensorId is null)
            {
                continue;
            }

            await db.StatusChangeLogs.AddAsync(new StatusChangeLog
            {
                SensorId = sensorId.Value,
                IsOnline = pending.IsOnline,
                Timestamp = pending.Timestamp
            }, ct);

            var alertLogs = _alertEvaluator.EvaluateOffline(sensorId.Value, pending.IsOnline);
            foreach (var log in alertLogs)
            {
                _alertLogQueue.Enqueue(log);
            }
        }
    }

    private async Task FlushStateAsync(AppDbContext db, List<PendingState> batch, CancellationToken ct)
    {
        foreach (var pending in batch)
        {
            var sensorId = await SensorIdResolver.ResolveSensorIdAsync(db, pending.MAC, pending.SensorType, _logger, ct);

            // Skip if sensor is not registered yet
            if (sensorId is null)
            {
                continue;
            }

            await db.StateChangeLogs.AddAsync(new StateChangeLog
            {
                SensorId = sensorId.Value,
                IsTriggered = pending.IsTriggered,
                Timestamp = pending.Timestamp
            }, ct);

            var alertLogs = _alertEvaluator.EvaluateTrigger(sensorId.Value, pending.IsTriggered);
            foreach (var log in alertLogs)
            {
                _alertLogQueue.Enqueue(log);
            }
        }
    }

    private async Task FlushAlertsAsync(AppDbContext db, CancellationToken ct)
    {

        var batch = ConcurrentQueueHelper.DrainQueue(_alertLogQueue);

        if (batch.Count == 0)
        {
            return;
        }

        await db.AlertLogs.AddRangeAsync(batch, ct);
    }
}
