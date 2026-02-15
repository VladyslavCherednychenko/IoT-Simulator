using SimulatorApp.Core.Enums;
using System.Text.Json;

namespace SimulatorApp.Web.Helpers;

public static class PayloadParser
{
    private static Dictionary<string, JsonElement>? TryDeserialize(string payload) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);

    public static (Metric Metric, double Value)? TryParseTelemetry(SensorType sensorType, string payload, ILogger logger)
    {
        var json = TryDeserialize(payload);
        if (json is null)
        {
            return null;
        }

        var key = sensorType.ToString();
        if (!json.TryGetValue(key, out var element))
        {
            return null;
        }

        if (!Enum.TryParse<Metric>(key, ignoreCase: true, out var metric))
        {
            logger.LogWarning("Cannot map SensorType {Key} to Metric enum", key);
            return null;
        }

        return (metric, element.GetDouble());
    }

    public static bool? TryParseMotion(string payload)
    {
        var json = TryDeserialize(payload);
        if (json is null)
        {
            return null;
        }

        return json.TryGetValue(nameof(SensorType.Motion), out var element) ? element.GetBoolean() : null;
    }

    public static bool? TryParseStatus(string payload)
    {
        var json = TryDeserialize(payload);
        if (json is null)
        {
            return null;
        }

        return json.TryGetValue("status", out var el) ? el.GetString() == "online" : null;
    }
}
