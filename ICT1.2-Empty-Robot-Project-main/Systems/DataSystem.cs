using System.Device.Gpio;
using System.Threading.Tasks;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

/// <summary>
/// Data system for environmental sensors (temperature, humidity)
/// Publishes sensor data to MQTT at configured intervals
/// </summary>
public class DataSystem : IUpdatable
{
    private readonly DHT11? dHT11;
    private readonly CommunicationSystem communicationSystem;
    private readonly RobotConfiguration config;
    private PeriodTimer scanIntervalTimer;
    private DateTime _lastRead = DateTime.MinValue;
    private const int MinIntervalMs = 1500;

    public DataSystem(RobotConfiguration config, CommunicationSystem communicationSystem)
    {
        this.config = config;
        this.communicationSystem = communicationSystem;

        // Initialize DHT11 sensor from configuration
        var dhtConfig = config.GetSensor("dht11");
        if (dhtConfig != null)
        {
            dHT11 = new DHT11(dhtConfig.Pin);
            Console.WriteLine($"DEBUG: DHT11 sensor initialized at pin {dhtConfig.Pin}");
        }
        else
        {
            Console.WriteLine("ERROR: DHT11 sensor configuration not found");
            dHT11 = null;
        }

        scanIntervalTimer = new PeriodTimer(config.CommunicationConfig.DataPublishIntervalMs);
    }

    public void Update()
    {
        if (scanIntervalTimer.Check() && dHT11 != null)
        {
            if ((DateTime.Now - _lastRead).TotalMilliseconds < MinIntervalMs)
                return;
            try
            {
                var data = dHT11.GetTemperatureAndHumidity();
                if (data != null && data[4] > 0)
                {
                    // Publish via communication system
                    // Fire-and-forget async operations
                    _ = communicationSystem.PublishTemperature(data[2].ToString() + "." + data[3].ToString());
                    _ = communicationSystem.PublishHumidity(data[0].ToString() + "." + data[1].ToString());
                    _lastRead = DateTime.Now;
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to read DHT11 sensor: {ex.Message}");
            }
        }
    }
}