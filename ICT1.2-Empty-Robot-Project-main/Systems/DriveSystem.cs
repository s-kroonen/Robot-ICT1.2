using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

/// <summary>
/// Drive system for robot motor control
/// Uses configuration for motor parameters
/// </summary>
public class DriveSystem : IUpdatable
{
    private readonly RobotConfiguration config;
    private double maxSpeed;
    public double sensitivity;
    public double maxTurning;
    public double starting;
    public double speedStep;

    public double targetSpeed;
    public double actualSpeed;

    private enum RobotState
    {
        Forward,
        TurnLeft,
        TurnRight,
        Reverse
    }

    private RobotState currentState = RobotState.Forward;
    public bool DriveActive { get; set; } = true;
    private double time, steer;
    public bool manualControl;

    public DriveSystem(RobotConfiguration config)
    {
        this.config = config;

        // Load from configuration
        maxSpeed = config.DriveConfig.MaxSpeed;
        sensitivity = config.DriveConfig.Sensitivity;
        maxTurning = config.DriveConfig.MaxTurning;
        starting = config.DriveConfig.StartingTurn;
        speedStep = config.DriveConfig.SpeedStep;

        targetSpeed = 0.0;
        actualSpeed = 0.0;

        Console.WriteLine("DEBUG: DriveSystem initialized with configuration");
    }

    private short ToRobotSpeedValue(double speed)
    {
        if (speed < maxSpeed * -1)
        {
            speed = maxSpeed * -1;
        }
        else if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }
        return (short)Math.Round(speed * 300.0);
    }

    public void manual(int right, int left)
    {
        // Placeholder for manual control
    }

    private void ControlRobotMotorSpeeds()
    {
        if (DriveActive)
        {
            Robot.Motors(
                ToRobotSpeedValue(actualSpeed),
                ToRobotSpeedValue(actualSpeed));
        }
    }

    public void EmergencyStop()
    {
        targetSpeed = 0.0;
        actualSpeed = 0.0;
        ControlRobotMotorSpeeds();
    }

    public void Update()
    {
        // Update actual speed towards target speed
        if (actualSpeed < targetSpeed)
        {
            actualSpeed += speedStep;
            if (actualSpeed > targetSpeed) actualSpeed = targetSpeed;
        }
        else if (actualSpeed > targetSpeed)
        {
            actualSpeed -= speedStep;
            if (actualSpeed < targetSpeed) actualSpeed = targetSpeed;
        }

        bool steerInvert = false;
        if (currentState == RobotState.TurnRight)
        {
            if (time > maxTurning)
            {
                time += 1;
                steerInvert = true;
            }
        }
        else if (currentState == RobotState.TurnLeft)
        {
            if (time < maxTurning)
            {
                time += 1;
                steerInvert = false;
            }
        }
        else if (currentState == RobotState.Forward)
        {
            time = 0;
            steer = 0;
        }
        if (time > 0)
            steer = starting + Math.Pow(sensitivity, time);
        else
            steer = 0;
        if (steer >= maxTurning)
        {
            steer = maxTurning;
        }
        TurnRelative(steer, steerInvert);
    }

    public void LineInput(bool left, bool forward, bool right)
    {
        // Placeholder for line following input
    }

    private void TurnRelative(double speed, bool steerInvert)
    {
        if (DriveActive)
        {
            if (steerInvert)
            {
                Robot.Motors(
                        ToRobotSpeedValue((actualSpeed - speed)),
                        ToRobotSpeedValue(((actualSpeed + speed)) + 0.1));
            }
            else
            {
                Robot.Motors(
                        ToRobotSpeedValue((actualSpeed + speed)),
                        ToRobotSpeedValue((actualSpeed - speed)));
            }
        }
    }
}
