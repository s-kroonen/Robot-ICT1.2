using System;

/// <summary>
/// Defines sensor types available in the robot
/// </summary>
public enum SensorType
{
    Ultrasonic2Pin,      // 2-pin ultrasonic sensor
    Ultrasonic1Pin,      // 1-pin ultrasonic sensor
    InfraredReflective,  // Infrared reflective sensor for line detection
    DHT11,               // Temperature and Humidity sensor
    RGBColor,            // RGB color sensor
    Button,              // Button sensor
    Led                  // LED output
}

/// <summary>
/// Defines the direction a sensor is facing
/// </summary>
public enum SensorDirection
{
    Forward,
    Reverse,
    Left,
    Right,
    ForwardLeft,
    ForwardRight,
    ReverseLeft,
    ReverseRight
}

/// <summary>
/// Configuration for a single sensor with its properties
/// </summary>
public class SensorConfiguration
{
    /// <summary>
    /// Unique identifier for the sensor
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Type of sensor
    /// </summary>
    public SensorType Type { get; set; }

    /// <summary>
    /// Primary pin number (or only pin for single-pin sensors)
    /// </summary>
    public int Pin { get; set; }

    /// <summary>
    /// Secondary pin number (for 2-pin sensors like ultrasonic)
    /// </summary>
    public int? SecondaryPin { get; set; }

    /// <summary>
    /// Direction the sensor is facing (relevant for ultrasonic and IR sensors)
    /// </summary>
    public SensorDirection? Direction { get; set; }

    /// <summary>
    /// I2C address (for sensors using I2C protocol)
    /// </summary>
    public byte? I2CAddress { get; set; }

    /// <summary>
    /// Enable/disable status
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Custom settings as key-value pairs for sensor-specific configurations
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; set; } = new();

    public SensorConfiguration(string id, SensorType type, int pin)
    {
        Id = id;
        Type = type;
        Pin = pin;
    }

    public SensorConfiguration WithSecondaryPin(int secondaryPin)
    {
        SecondaryPin = secondaryPin;
        return this;
    }

    public SensorConfiguration WithDirection(SensorDirection direction)
    {
        Direction = direction;
        return this;
    }

    public SensorConfiguration WithI2CAddress(byte address)
    {
        I2CAddress = address;
        return this;
    }

    public SensorConfiguration WithCustomSetting(string key, object value)
    {
        CustomSettings[key] = value;
        return this;
    }
}
