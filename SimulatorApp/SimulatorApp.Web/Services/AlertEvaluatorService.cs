using Microsoft.EntityFrameworkCore;
using SimulatorApp.Core.Enums;
using SimulatorApp.Core.Models;
using SimulatorApp.Infrastructure.Data;

namespace SimulatorApp.Web.Services;

public sealed class AlertEvaluatorService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private Dictionary<long, List<AlertRule>> _rulesBySensor = [];

    private readonly Lock _lock = new();

    public AlertEvaluatorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task InitializeAsync()
    {
        await LoadRulesAsync();
    }

    private async Task LoadRulesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var rules = await db.AlertRules.Where(r => r.IsEnabled).ToListAsync();

        lock (_lock)
        {
            _rulesBySensor = rules.GroupBy(r => r.SensorId).ToDictionary(g => g.Key, g => g.ToList());
        }
    }

    public async Task ReloadRulesAsync() => await LoadRulesAsync();

    public List<AlertLog> EvaluateTelemetry(long sensorId, double value)
    {
        List<AlertRule>? rules;
        lock (_lock)
        {
            if (!_rulesBySensor.TryGetValue(sensorId, out rules))
            {
                return [];
            }
        }

        var logs = new List<AlertLog>();
        foreach (var rule in rules)
        {
            bool shouldTrigger = false;
            if ((rule.RangeMin.HasValue && value < rule.RangeMin.Value) || (rule.RangeMax.HasValue && value > rule.RangeMax.Value))
            {
                shouldTrigger = true;
            }

            if (shouldTrigger)
            {
                logs.Add(new AlertLog
                {
                    AlertRuleId = rule.AlertRuleId,
                    SensorId = sensorId,
                    TriggerValue = value,
                    IsRead = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        return logs;
    }

    public List<AlertLog> EvaluateTrigger(long sensorId, bool isTriggered)
    {
        List<AlertRule>? rules;
        lock (_lock)
        {
            if (!_rulesBySensor.TryGetValue(sensorId, out rules))
            {
                return [];
            }
        }

        var logs = new List<AlertLog>();
        foreach (var rule in rules.Where(e => e.AlertType == AlertType.Trigger))
        {
            if (isTriggered)
            {
                logs.Add(new AlertLog
                {
                    AlertRuleId = rule.AlertRuleId,
                    SensorId = sensorId,
                    TriggerValue = null,
                    IsRead = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        return logs;
    }

    public List<AlertLog> EvaluateOffline(long sensorId, bool isOnline)
    {
        List<AlertRule>? rules;
        lock (_lock)
        {
            if (!_rulesBySensor.TryGetValue(sensorId, out rules))
            {
                return [];
            }
        }

        var logs = new List<AlertLog>();
        foreach (var rule in rules.Where(r => r.AlertType == AlertType.Offline))
        {
            if (!isOnline)
            {
                logs.Add(new AlertLog
                {
                    AlertRuleId = rule.AlertRuleId,
                    SensorId = sensorId,
                    TriggerValue = null,
                    IsRead = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        return logs;
    }
}
