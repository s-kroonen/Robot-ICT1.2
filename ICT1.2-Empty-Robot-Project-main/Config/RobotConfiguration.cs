using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central configuration class for the wheeled robot
/// Defines all sensors and their configurations without hardcoding in systems
/// </summary>
public class RobotConfiguration
{
    /// <summary>
    /// Robot name for identification in MQTT and display
    /// </summary>
    public string RobotName { get; set; } = "DefaultRobot";

    /// <summary>
    /// Hardware pin for the alert LED
    /// </summary>
    public int AlertLedPin { get; set; }

    /// <summary>
    /// Hardware pin for the emergency stop button
    /// </summary>
    public int EmergencyStopButtonPin { get; set; }

    /// <summary>
    /// I2C address for the LCD display
    /// </summary>
    public byte LcdI2CAddress { get; set; }

    /// <summary>
    /// All sensor configurations
    /// </summary>
    public List<SensorConfiguration> Sensors { get; set; } = new();

    /// <summary>
    /// Motor configuration
    /// </summary>
    public MotorConfiguration MotorConfig { get; set; }

    /// <summary>
    /// Drive system configuration
    /// </summary>
    public DriveConfiguration DriveConfig { get; set; }

    /// <summary>
    /// Communication settings
    /// </summary>
    public CommunicationConfiguration CommunicationConfig { get; set; }

    public RobotConfiguration()
    {
        MotorConfig = new MotorConfiguration();
        DriveConfig = new DriveConfiguration();
        CommunicationConfig = new CommunicationConfiguration();
    }

    /// <summary>
    /// Get all ultrasonic sensors configured
    /// </summary>
    public List<SensorConfiguration> GetUltrasonicSensors()
    {
        return Sensors.Where(s => 
            s.Type == SensorType.Ultrasonic2Pin || s.Type == SensorType.Ultrasonic1Pin)
            .ToList();
    }

    /// <summary>
    /// Get ultrasonic sensors by specific direction
    /// </summary>
    public List<SensorConfiguration> GetUltrasonicSensorsByDirection(SensorDirection direction)
    {
        return GetUltrasonicSensors()
            .Where(s => s.Direction == direction)
            .ToList();
    }

    /// <summary>
    /// Get IR line sensors
    /// </summary>
    public List<SensorConfiguration> GetLineSensors()
    {
        return Sensors.Where(s => s.Type == SensorType.InfraredReflective)
            .ToList();
    }

    /// <summary>
    /// Get a sensor by ID
    /// </summary>
    public SensorConfiguration? GetSensor(string sensorId)
    {
        return Sensors.FirstOrDefault(s => s.Id == sensorId);
    }

    /// <summary>
    /// Add a sensor configuration
    /// </summary>
    public void AddSensor(SensorConfiguration sensor)
    {
        if (Sensors.Any(s => s.Id == sensor.Id))
            throw new InvalidOperationException($"Sensor with ID '{sensor.Id}' already exists");
        
        Sensors.Add(sensor);
    }

    /// <summary>
    /// Creates the default robot configuration (John's setup)
    /// </summary>
    public static RobotConfiguration CreateDefaultConfiguration()
    {
        var config = new RobotConfiguration
        {
            RobotName = "John",
            AlertLedPin = 22,
            EmergencyStopButtonPin = 23,
            LcdI2CAddress = 0x3E
        };

        // Add ultrasonic sensors for obstacle detection (8 directions)
        config.AddSensor(new SensorConfiguration("ultrasonic_forward", SensorType.Ultrasonic1Pin, 5)
            .WithSecondaryPin(6)
            .WithDirection(SensorDirection.Forward));

        config.AddSensor(new SensorConfiguration("ultrasonic_reverse_left", SensorType.Ultrasonic1Pin, 18)
            .WithSecondaryPin(14)
            .WithDirection(SensorDirection.ReverseLeft));

        config.AddSensor(new SensorConfiguration("ultrasonic_reverse_right", SensorType.Ultrasonic1Pin, 16)
            .WithSecondaryPin(20)
            .WithDirection(SensorDirection.ReverseRight));

        // Add environmental sensors
        config.AddSensor(new SensorConfiguration("dht11", SensorType.DHT11, 26));

        // config.AddSensor(new SensorConfiguration("rgb_color", SensorType.RGBColor, 0)
        //     .WithI2CAddress(0x29));

        return config;
    }
}

/// <summary>
/// Motor configuration
/// </summary>
public class MotorConfiguration
{
    public double MaxSpeed { get; set; } = 0.2;
    public double MaxTurning { get; set; } = 0.2;
    public double StartingTurn { get; set; } = 0.2;
}

/// <summary>
/// Drive system configuration
/// </summary>
public class DriveConfiguration
{
    public double MaxSpeed { get; set; } = 0.2;
    public double Sensitivity { get; set; } = 1.0;
    public double MaxTurning { get; set; } = 0.2;
    public double StartingTurn { get; set; } = 0.2;
    public double SpeedStep { get; set; } = 0.05;
}

/// <summary>
/// Communication configuration for MQTT
/// </summary>
public class CommunicationConfiguration
{
    public string BaseTopic { get; set; } = "avansict";
    public int DataPublishIntervalMs { get; set; } = 300;  // How often to publish sensor data
    public int StatusPublishIntervalMs { get; set; } = 1000; // How often to publish robot state
}
 