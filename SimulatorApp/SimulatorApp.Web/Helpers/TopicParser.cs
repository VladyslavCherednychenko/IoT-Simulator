using SimulatorApp.Core.Enums;

namespace SimulatorApp.Web.Helpers;

public static class TopicParser
{
    // Expected: home/{location}/{mac}/{sensorType}/telemetry|status
    public static ParsedTopic? TryParse(string topic, ILogger logger)
    {
        var parts = topic.Split('/');
        if (parts.Length != 5)
        {
            logger.LogWarning("Unexpected topic format: {Topic}", topic);
            return null;
        }

        if (!Enum.TryParse<SensorType>(parts[3], ignoreCase: true, out var sensorType))
        {
            logger.LogWarning("Unknown SensorType in topic: {Value}", parts[3]);
            return null;
        }

        return new ParsedTopic(MAC: parts[2], Location: parts[1], SensorType: sensorType, MessageType: parts[4]);
    }
}

public record ParsedTopic(string MAC, string Location, SensorType SensorType, string MessageType);
