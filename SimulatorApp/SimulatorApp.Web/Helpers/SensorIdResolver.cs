using Microsoft.EntityFrameworkCore;
using SimulatorApp.Core.Enums;
using SimulatorApp.Infrastructure.Data;

namespace SimulatorApp.Web.Helpers;

public static class SensorIdResolver
{
    // Cache (MAC + SensorType) -> SensorId
    // to avoid repeated DB lookups per flush
    private static readonly Dictionary<(string MAC, SensorType), long> _sensorIdCache = [];

    public static async Task<long?> ResolveSensorIdAsync(AppDbContext db, string mac, SensorType sensorType, ILogger logger, CancellationToken ct)
    {
        var key = (mac, sensorType);

        // Try to retrieve Id from in-memory cache first
        if (_sensorIdCache.TryGetValue(key, out var cachedId))
        {
            return cachedId;
        }

        var sensor = await db.Sensors
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.Device.MAC == mac && s.SensorType == sensorType, ct);

        if (sensor is null)
        {
            logger.LogDebug("Sensor not registered - MAC={MAC}, Type={Type}", mac, sensorType);
            return null;
        }

        // Chache Id on successful DB lookup to reuse it for all future flushes
        _sensorIdCache[key] = sensor.SensorId;
        return sensor.SensorId;
    }
}
