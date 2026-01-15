using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using System.Collections.Generic;

public class ObstacleDetectionSystem : IUpdatable
{
    private const int ScanIntervalMilliseconds = 200;
    private readonly RobotConfiguration config;
    private readonly Dictionary<string, Ultrasonic> ultrasonicSensors = new();
    private PeriodTimer scanIntervalTimer;

    /// <summary>
    /// Dictionary mapping sensor direction to closest obstacle distance
    /// </summary>
    public Dictionary<SensorDirection, int> DirectionalDistances { get; private set; } = new();

    /// <summary>
    /// The overall minimum obstacle distance (across all directions)
    /// </summary>
    public int ObstacleDistance { get; private set; }

    public ObstacleDetectionSystem(RobotConfiguration config)
    {
        Console.WriteLine("DEBUG: ObstacleDetectionSystem constructor called");
        this.config = config;
        scanIntervalTimer = new PeriodTimer(ScanIntervalMilliseconds);
        InitializeSensors();
    }

    /// <summary>
    /// Initialize all ultrasonic sensors from configuration
    /// </summary>
    private void InitializeSensors()
    {
        var ultrasonicConfigs = config.GetUltrasonicSensors();
        
        foreach (var sensorConfig in ultrasonicConfigs)
        {
            if (!sensorConfig.IsEnabled)
                continue;

            try
            {
                // if (sensorConfig.Type == SensorType.Ultrasonic2Pin && sensorConfig.SecondaryPin.HasValue)
                // {
                //     var sensor = new Ultrasonic_2pin(sensorConfig.Pin);
                //     ultrasonicSensors[sensorConfig.Id] = sensor;
                    
                //     if (sensorConfig.Direction.HasValue)
                //     {
                //         DirectionalDistances[sensorConfig.Direction.Value] = 0;
                //     }

                //     Console.WriteLine($"DEBUG: Initialized {sensorConfig.Type} sensor '{sensorConfig.Id}' at pin {sensorConfig.Pin}");
                // }
                if (sensorConfig.Type == SensorType.Ultrasonic1Pin)
                {
                    var sensor = new Ultrasonic(sensorConfig.Pin);
                    ultrasonicSensors[sensorConfig.Id] = sensor;
                    
                    if (sensorConfig.Direction.HasValue)
                    {
                        DirectionalDistances[sensorConfig.Direction.Value] = 0;
                    }

                    Console.WriteLine($"DEBUG: Initialized {sensorConfig.Type} sensor '{sensorConfig.Id}' at pin {sensorConfig.Pin}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to initialize sensor '{sensorConfig.Id}': {ex.Message}");
            }
        }
    }

    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            int minDistance = int.MaxValue;
            DirectionalDistances.Clear();

            foreach (var (sensorId, sensor) in ultrasonicSensors)
            {
                try
                {
                    int distance = sensor.GetUltrasoneDistance();
                    
                    var sensorConfig = config.GetSensor(sensorId);
                    if (sensorConfig?.Direction.HasValue ?? false)
                    {
                        DirectionalDistances[sensorConfig.Direction.Value] = distance;
                    }

                    minDistance = Math.Min(minDistance, distance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to read distance from sensor '{sensorId}': {ex.Message}");
                }
            }

            ObstacleDistance = minDistance == int.MaxValue ? 0 : minDistance;
        }
    }

    /// <summary>
    /// Get the closest obstacle distance in a specific direction
    /// </summary>
    public int GetDistanceInDirection(SensorDirection direction)
    {
        return DirectionalDistances.TryGetValue(direction, out var distance) ? distance : 0;
    }

    /// <summary>
    /// Get all available directional distances as formatted string for MQTT
    /// </summary>
    public string GetDirectionalDistancesJson()
    {
        var directions = DirectionalDistances.Select(kvp => 
            $"\"{kvp.Key}\":{kvp.Value}");
        return "{" + string.Join(",", directions) + "}";
    }
}