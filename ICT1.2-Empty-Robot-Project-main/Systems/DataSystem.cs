using System.Device.Gpio;
using System.Threading.Tasks;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class DataSystem : IUpdatable
{
    private DHT11new dHT11 = new DHT11new(26);

    private CommunicationSystem communicationSystem;
    private PeriodTimer scanIntervalTimer = new PeriodTimer(300);
    public DataSystem(CommunicationSystem communicationSystem)
    {
        this.communicationSystem = communicationSystem;
    }

    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            var data = dHT11.GetTemperatureAndHumidity();
            if (data != null)
            {
                communicationSystem.SendTempMeasurment(data.Value.Temperature.ToString());
                communicationSystem.SendHumMeasurment(data.Value.Humidity.ToString());
            }
        }
    }
}