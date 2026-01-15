namespace RobotApp.Services.Mqtt;

public class RobotCommandService
{
    private readonly MqttService _mqtt;

    public RobotCommandService(MqttService mqtt)
    {
        _mqtt = mqtt;
    }

    public Task Drive(string robot, string direction, double speed)
    {
        return _mqtt.SendCommand(robot, $"drive:{direction}:{speed}");
    }

    public Task EmergencyStop(string robot)
    {
        return _mqtt.SendCommand(robot, "emergency:stop");
    }

    public Task SendMessage(string robot, string message)
    {
        return _mqtt.SendCommand(robot, $"message:{message}");
    }
}
