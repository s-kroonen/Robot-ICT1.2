using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

/// <summary>
/// Color detection system using RGB sensor
/// Detects color and publishes to MQTT
/// </summary>
public class ColorSystem : IUpdatable
{
    private PeriodTimer scanIntervalTimer = new PeriodTimer(100);
    private RGBSensor? rGBSensor;
    private ushort r, g, b, c;
    public string color { get; private set; } = "Unknown";
    private readonly RobotConfiguration config;

    public ColorSystem(RobotConfiguration config)
    {
        this.config = config;
        InitializeSensor();
    }

    /// <summary>
    /// Initialize RGB sensor from configuration
    /// </summary>
    private void InitializeSensor()
    {
        try
        {
            var rgbConfig = config.GetSensor("rgb_color");
            if (rgbConfig?.I2CAddress.HasValue ?? false)
            {
                rGBSensor = new RGBSensor(
                    rgbConfig.I2CAddress.Value,
                    RGBSensor.IntegrationTime.INTEGRATION_TIME_700MS,
                    RGBSensor.Gain.GAIN_4X);
                rGBSensor.Begin();
                Console.WriteLine($"DEBUG: RGB Color sensor initialized at I2C address 0x{rgbConfig.I2CAddress:X}");
            }
            else
            {
                // Fallback to default configuration
                rGBSensor = new RGBSensor(0x29,
                    RGBSensor.IntegrationTime.INTEGRATION_TIME_700MS,
                    RGBSensor.Gain.GAIN_4X);
                rGBSensor.Begin();
                Console.WriteLine("DEBUG: RGB Color sensor initialized with default I2C address");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to initialize RGB sensor: {ex.Message}");
        }
    }

    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            try
            {
                if (rGBSensor != null)
                {
                    rGBSensor.GetRawData(out r, out g, out b, out c);
                    color = DetectColor(r, g, b);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to read RGB sensor: {ex.Message}");
                color = "Error";
            }
        }
    }

    /// <summary>
    /// Detect color from RGB values
    /// </summary>
    public static string DetectColor(int red, int green, int blue)
    {
        // Normalize the RGB values to the range 0-1
        double total = red + green + blue;
        if (total == 0)
            return "Black"; // No light detected

        double normalizedRed = red / total;
        double normalizedGreen = green / total;
        double normalizedBlue = blue / total;

        // Define thresholds for color detection
        if (normalizedRed > 0.4 && normalizedGreen < 0.3 && normalizedBlue < 0.3)
            return "Red";
        else if (normalizedGreen > 0.35 && normalizedRed < 0.35 && normalizedBlue < 0.35)
            return "Green";
        else if (normalizedBlue > 0.3 && normalizedRed < 0.4 && normalizedGreen < 0.4)
            return "Blue";
        else if (normalizedRed > 0.3 && normalizedGreen > 0.4 && normalizedBlue < 0.3)
            return "Yellow";
        else if (normalizedRed > 0.4 && normalizedBlue > 0.5 && normalizedGreen < 0.4)
            return "Magenta";
        else if (normalizedGreen > 0.4 && normalizedBlue > 0.5 && normalizedRed < 0.4)
            return "Cyan";
        else if (normalizedRed > 0.3 && normalizedGreen > 0.3 && normalizedBlue > 0.3)
            return "White";
        else
            return "Uncertain";
    }
}