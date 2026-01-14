using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class DriveSystem : IUpdatable
{
    private double maxSpeed = 0.2;
    public double sensitivity = 1;
    public double maxTurning = 0.2;
    public double starting = 0.2;

    public double speedStep = 0.05,
    targetSpeed, actualSpeed;

    private RobotState currentState = RobotState.Forward;

    public bool DriveActive { get; set; } = true;
    private double time, steer;
    public bool manualControl;


    private enum RobotState
    {
        Forward,
        TurnLeft,
        TurnRight
    }

    public DriveSystem()
    {
        targetSpeed = 0.0;
        actualSpeed = 0.0;
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

    }

    private void ControlRobotMotorSpeeds()
    {
        if (DriveActive)
        {
            Console.WriteLine(currentState + "." + actualSpeed);
            Robot.Motors(
                ToRobotSpeedValue(actualSpeed * -1),
                ToRobotSpeedValue(actualSpeed * -1));

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
        // Set the robot's state based on the line sensor input
        if (forward && left && right)
        {
            // currentState = RobotState.Forward;
        }
        else if (left)
        {
            // if (currentState != RobotState.TurnLeft)
            // {
            //     actualSpeed = 0;
            // }
            currentState = RobotState.TurnLeft;
        }
        else if (right)
        {
            // if (currentState != RobotState.TurnRight)
            // {
            //     actualSpeed = 0;
            // }
            currentState = RobotState.TurnRight;
        }
        else if (forward)
        {
            // if (currentState != RobotState.Forward)
            // {
            //     actualSpeed = 0;
            // }
            currentState = RobotState.Forward;
        }
    }

    private void TurnRelative(double speed, bool steerInvert)
    {
        if (DriveActive)
        {
            Console.WriteLine(currentState + "," + actualSpeed + "," + speed + "," + steerInvert);
            if (steerInvert)
            {
                Robot.Motors(
                        ToRobotSpeedValue((actualSpeed - speed) * -1),
                        ToRobotSpeedValue(((actualSpeed + speed) * -1) + 0.1));
            }
            else
            {
                Robot.Motors(
                        ToRobotSpeedValue((actualSpeed + speed) * -1),
                        ToRobotSpeedValue((actualSpeed - speed) * -1));
            }
        }
    }

}
