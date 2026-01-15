using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class AlertSystem : IUpdatable
{
    private readonly Led alertLed;
    private readonly Button emergencyButton;
    private readonly LCD16x2 display;
    private readonly RobotConfiguration config;
    private bool emergencyButtonWasPressed;
    private string previousMessage = string.Empty;
    
    /// <summary>
    /// Triggered when emergency stop state changes (both manual button and MQTT)
    /// </summary>
    public event EventHandler<bool>? EmergencyStopChanged;

    /// <summary>
    /// Current emergency stop state
    /// </summary>
    public bool EmergencyStop { get; private set; }

    /// <summary>
    /// Alert is currently active
    /// </summary>
    public bool AlertActive { get; private set; }

    /// <summary>
    /// Current alert message
    /// </summary>
    public string CurrentAlertMessage => previousMessage;

    public AlertSystem(RobotConfiguration config, Led led, Button button, LCD16x2 lcd)
    {
        Console.WriteLine("DEBUG: AlertSystem constructor called");
        this.config = config;
        this.display = lcd;
        this.emergencyButton = button;
        this.alertLed = led;
        
        emergencyButtonWasPressed = emergencyButton.GetState().Equals("Pressed");
        alertLed.SetOff();
        AlertActive = false;
    }

    /// <summary>
    /// Turn on alert with message (display on LCD and LED)
    /// </summary>
    public void AlertOn(string message)
    {
        if (previousMessage != message)
        {
            display.SetText(message);
            previousMessage = message;
            AlertActive = true;
        }
        
        alertLed.SetOn();
        // Console.WriteLine($"DEBUG: Alert ON - {message}");
    }

    /// <summary>
    /// Turn off alert
    /// </summary>
    public void AlertOff()
    {
        display.SetText(""); // Clear display
        alertLed.SetOff();
        previousMessage = string.Empty;
        AlertActive = false;
        // Console.WriteLine("DEBUG: Alert OFF");
    }

    /// <summary>
    /// Set emergency stop state (can be called from MQTT or manual button)
    /// </summary>
    public void SetEmergencyStop(bool state, string reason = "")
    {
        if (EmergencyStop != state)
        {
            EmergencyStop = state;
            EmergencyStopChanged?.Invoke(this, state);

            if (state)
            {
                string message = string.IsNullOrEmpty(reason) ? "Emergency Stop" : reason;
                AlertOn(message);
                Console.WriteLine($"DEBUG: Emergency stop activated - {reason}");
            }
            else
            {
                AlertOff();
                Console.WriteLine("DEBUG: Emergency stop deactivated");
            }
        }
    }

    /// <summary>
    /// Toggle emergency stop state (for button handling)
    /// </summary>
    public void ToggleEmergencyStop()
    {
        SetEmergencyStop(!EmergencyStop, "Manual Emergency Stop");
    }

    /// <summary>
    /// Display a temporary message on LCD (doesn't trigger alert LED)
    /// </summary>
    public void DisplayMessage(string message)
    {
        if (previousMessage != message)
        {
            display.SetText(message);
            previousMessage = message;
            Console.WriteLine($"DEBUG: Display message - {message}");
        }
    }

    public void Update()
    {
        // Check if the emergency stop button state has changed and act accordingly
        if (emergencyButton.GetState() == "Pressed" && !emergencyButtonWasPressed)
        {
            Console.WriteLine("DEBUG: Emergency stop button pressed");
            emergencyButtonWasPressed = true;
            ToggleEmergencyStop();
        }
        else if (emergencyButton.GetState() == "Released" && emergencyButtonWasPressed)
        {
            Console.WriteLine("DEBUG: Emergency stop button released");
            emergencyButtonWasPressed = false;
        }
    }
}