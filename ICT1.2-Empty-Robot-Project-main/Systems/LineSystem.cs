using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using System.Collections.Generic;

public class LineSystem : IUpdatable
{
    private PeriodTimer scanIntervalTimer = new PeriodTimer(10);
    private readonly RobotConfiguration config;
    private readonly Dictionary<string, InfraredReflectiveAnalog?> irSensors = new();
    private readonly Dictionary<string, bool> sensorStates = new();
    public LineCallback callback;

    public LineSystem(RobotConfiguration config, LineCallback callback)
    {
        this.config = config;
        this.callback = callback;
        InitializeSensors();
    }

    /// <summary>
    /// Initialize all infrared line detection sensors from configuration
    /// </summary>
    private void InitializeSensors()
    {
        var lineSensors = config.GetLineSensors();
        
        foreach (var sensorConfig in lineSensors)
        {
            if (!sensorConfig.IsEnabled)
                continue;

            try
            {
                var sensor = new InfraredReflectiveAnalog(sensorConfig.Pin);
                irSensors[sensorConfig.Id] = sensor;
                sensorStates[sensorConfig.Id] = false;

                Console.WriteLine($"DEBUG: Initialized IR line sensor '{sensorConfig.Id}' at pin {sensorConfig.Pin}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to initialize line sensor '{sensorConfig.Id}': {ex.Message}");
            }
        }
    }

    public void Update()
    {
        if (scanIntervalTimer.Check() && irSensors.Count > 0)
        {
            // Read states from all IR sensors
            foreach (var (sensorId, sensor) in irSensors)
            {
                try
                {
                    sensorStates[sensorId] = sensor?.Watch() ?? false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to read line sensor '{sensorId}': {ex.Message}");
                }
            }

            // Determine directional states
            bool forwardState = GetSensorStateByDirection(SensorDirection.Forward);
            bool leftState = GetSensorStateByDirection(SensorDirection.Left);
            bool rightState = GetSensorStateByDirection(SensorDirection.Right);

            // Call callback with directional states
            callback.lineCallback(leftState, forwardState, rightState);
        }
    }

    /// <summary>
    /// Get sensor state by its direction
    /// </summary>
    private bool GetSensorStateByDirection(SensorDirection direction)
    {
        var sensor = config.GetLineSensors()
            .FirstOrDefault(s => s.Direction == direction);
        
        if (sensor != null && sensorStates.TryGetValue(sensor.Id, out var state))
        {
            return state;
        }
        
        return false;
    }

    /// <summary>
    /// Get raw state of a specific line sensor by ID
    /// </summary>
    public bool GetSensorState(string sensorId)
    {
        return sensorStates.TryGetValue(sensorId, out var state) ? state : false;
    }

    /// <summary>
    /// Get all line sensor states as a formatted string for MQTT
    /// </summary>
    public string? GetSensorStatesJson()
    {
        if (irSensors.Count == 0)
            return null;
        var states = irSensors.Keys.Select(sensorId =>
        {
            var sensorConfig = config.GetSensor(sensorId);
            var directionStr = sensorConfig?.Direction?.ToString() ?? "unknown";
            var stateStr = sensorStates.TryGetValue(sensorId, out var state) ? (state ? "detected" : "clear") : "unknown";
            return $"\"{directionStr}\":\"{stateStr}\"";
        });
        return "{" + string.Join(",", states) + "}";
    }
}