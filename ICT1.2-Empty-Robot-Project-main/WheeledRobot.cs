using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;

public class WheeledRobot : IUpdatable, LineCallback
{
    // Define local constants for hardware related details
    const int AlertLedPinNumber = 24;
    const int EmergencyStopButtonPinNumber = 25;
    const byte LcdAddress = 0x3E; // I2C address for the LCD grove module

    private DriveSystem driveSystem;
    private ObstacleDetectionSystem obstacleDetectionSystem;
    private AlertSystem alertSystem;
    private CommunicationSystem communicationSystem;
    private DataSystem dataSystem;
    private ColorSystem colorSystem;
    private LineSystem lineSystem;

    private Led alertLed;
    private Button emergencyStopButton;
    private LCD16x2 lcd;
    private bool stopped = false;
    private string name = "John";

    public WheeledRobot()
    {
        Console.WriteLine("DEBUG: WheeledRobot constructor called");

        driveSystem = new DriveSystem();

        obstacleDetectionSystem = new ObstacleDetectionSystem();

        alertLed = new Led(AlertLedPinNumber);

        emergencyStopButton = new Button(EmergencyStopButtonPinNumber);

        lcd = new LCD16x2(LcdAddress);

        alertSystem = new AlertSystem(alertLed, emergencyStopButton, lcd);

        communicationSystem = new CommunicationSystem(this, name);

        dataSystem = new DataSystem(communicationSystem);

        colorSystem = new ColorSystem();

        lineSystem = new LineSystem(this);
    }

    /// <summary>
    /// Initializes the WheeledRobot
    /// </summary>
    /// <returns></returns>
    public async Task Init()
    {
        Console.WriteLine("DEBUG: WheeledRobot Init() called");
        // Temporarily disable motors for quick testing during debugging
        driveSystem.DriveActive = false;

        // Configure the CommunicationSystem
        await communicationSystem.Init();
        await communicationSystem.SendAlertState("On");

        // Let the user know that we're up and running
        lcd.SetText("SimpleRobot");
        Console.WriteLine("DEBUG: WheeledRobot Init() finished");
    }

    public void HandleMessage(SimpleMqttMessage msg)
    {
        Console.WriteLine($"Message received (topic:msg) = {msg.Topic}:{msg.Message}");
        if(msg.Topic == "Drive"){
        }
    }

    public async void Update()
    {
        obstacleDetectionSystem.Update();
        driveSystem.Update();
        alertSystem.Update();
        dataSystem.Update();
        colorSystem.Update();
        lineSystem.Update();

        int distance = obstacleDetectionSystem.ObstacleDistance;
        await communicationSystem.SendDistanceMeasurement(distance);
        // Console.WriteLine($"DEBUG: Distance {distance} cm");

        if (distance < 3 && !stopped || alertSystem.EmergencyStop || driveSystem.manualControl)
        {
            stopped = true;
            driveSystem.EmergencyStop();
            driveSystem.DriveActive = false;
            alertSystem.AlertOn($"Emergency stop\nDistance {distance} cm");
            await communicationSystem.SendAlertState("On");
        }
        else if (distance >= 5 && stopped)
        {
            stopped = false;
            driveSystem.DriveActive = true;
            alertSystem.AlertOff();
            await communicationSystem.SendAlertState("Off");
        }

        if (distance >= 5 && distance < 15)
        {
            driveSystem.targetSpeed = 0.2;
        }
        else if (distance >= 15 && distance < 40)
        {
            driveSystem.targetSpeed = 0.2;
        }
        else if (distance >= 40)
        {
            driveSystem.targetSpeed = 0.2;
        }
    }

    public void lineCallback(bool left, bool forward, bool right)
    {
        Console.WriteLine(left.ToString() + "." + forward.ToString() + "." + right.ToString());
        driveSystem.LineInput(left,forward,right);
    }

}

