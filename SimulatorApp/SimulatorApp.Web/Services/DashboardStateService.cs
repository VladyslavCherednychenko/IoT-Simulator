using SimulatorApp.Core.Enums;
using SimulatorApp.Web.Models;

namespace SimulatorApp.Web.Services;

public class DashboardStateService
{
    // In-memory snapshot of the latest known state for every device
    private readonly Dictionary<string, DeviceState> _devices = [];
    private readonly Lock _lock = new();

    public event Action? OnChange;

    public IReadOnlyDictionary<string, DeviceState> Devices
    {
        get {
            lock (_lock) return _devices.ToDictionary();
        }
    }

    public DeviceState? GetDevice(string mac)
    {
        lock (_lock) return _devices.GetValueOrDefault(mac);
    }

    public void UpdateTelemetry(string mac, string location, SensorType sensorType, Metric metric, double value)
    {
        lock (_lock)
        {
            var device = GetOrCreateDevice(mac, location);
            var sensor = GetOrCreateSensor(device, sensorType);
            sensor.LastValue = value;
            sensor.LastMetric = metric;
            sensor.LastSeen = DateTime.UtcNow;
            device.LastSeen = DateTime.UtcNow;
            sensor.IsOnline = true;
            device.IsOnline = true;
        }
        OnChange?.Invoke();
    }

    public void UpdateTrigger(string mac, string location, SensorType sensorType, bool isTriggered)
    {
        lock (_lock)
        {
            var device = GetOrCreateDevice(mac, location);
            var sensor = GetOrCreateSensor(device, sensorType);
            sensor.LastTriggered = isTriggered;
            sensor.LastSeen = DateTime.UtcNow;
            device.LastSeen = DateTime.UtcNow;
            sensor.IsOnline = true;
            device.IsOnline = true;
        }
        OnChange?.Invoke();
    }

    public void UpdateSensorStatus(string mac, string location, SensorType sensorType, bool isOnline)
    {
        lock (_lock)
        {
            var device = GetOrCreateDevice(mac, location);
            var sensor = GetOrCreateSensor(device, sensorType);
            sensor.IsOnline = isOnline;
            sensor.LastSeen = DateTime.UtcNow;

            // Device is online if any sensor is online
            device.IsOnline = device.Sensors.Values.Any(s => s.IsOnline);
            device.LastSeen = DateTime.UtcNow;
        }
        OnChange?.Invoke();
    }

    public void UpdateSimulatorStatus(bool isOnline)
    {
        lock (_lock)
        {
            foreach (var device in _devices.Values)
            {
                device.IsOnline = isOnline;
                foreach (var sensor in device.Sensors.Values)
                {
                    sensor.IsOnline = isOnline;
                }
            }
        }
        OnChange?.Invoke();
    }

    // --- Helpers ---
    private DeviceState GetOrCreateDevice(string mac, string location)
    {
        if (!_devices.TryGetValue(mac, out var device))
        {
            device = new DeviceState { MAC = mac, Model = mac, Location = location };
            _devices[mac] = device;
        }
        return device;
    }

    private static SensorState GetOrCreateSensor(DeviceState device, SensorType sensorType)
    {
        if (!device.Sensors.TryGetValue(sensorType, out var sensor))
        {
            sensor = new SensorState { SensorType = sensorType };
            device.Sensors[sensorType] = sensor;
        }
        return sensor;
    }
}
