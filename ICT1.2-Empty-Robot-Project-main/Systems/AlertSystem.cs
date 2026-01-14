using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class AlertSystem : IUpdatable
{
    private Led alertLed;
    private Button emergencyButton;
    private bool emergencyButtonWasPressed;
    private LCD16x2 display;
    private string PreviousMessage;
    public bool EmergencyStop { get; set; }

    public AlertSystem(Led led, Button button, LCD16x2 lcd)
    {
        Console.WriteLine("DEBUG: AlertSystem constructor called");

        display = lcd;
        emergencyButton = button;
        emergencyButtonWasPressed = emergencyButton.GetState().Equals("Pressed");
        alertLed = led;
        alertLed.SetOff();
    }
    public void AlertOn(string message)
    {
        if(PreviousMessage != message){
            display.SetText(message);
            PreviousMessage = message;
        }
        alertLed.SetOn();
        // Robot.PlayNotes("fd");
    }

    public void AlertOff()
    {
        display.SetText(""); // Clear display
        alertLed.SetOff();
        // Robot.PlayNotes("f>c");
    }
    public void Update()
    {
        // Check if the emergency stop button state has changed and act accordingly
        if (emergencyButton.GetState() == "Pressed" && !emergencyButtonWasPressed)
        {
            Console.WriteLine("DEBUG: Emergency stop button pressed");
            emergencyButtonWasPressed = true;
            EmergencyStop = !EmergencyStop;
        }
        else if (emergencyButton.GetState() == "Released" && emergencyButtonWasPressed)
        {
            Console.WriteLine("DEBUG: Emergency stop button released");
            emergencyButtonWasPressed = false;
        }
    }
}