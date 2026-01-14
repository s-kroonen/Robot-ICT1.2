using System;
using System.Device.Gpio;

namespace Avans.StatisticalRobot;

public class InfraredReflectiveAnalog
{
    private readonly int _pin;
    private DateTime syncTime;
    private int state;

    public InfraredReflectiveAnalog(int pin)
    {
        Robot.SetDigitalPinMode(pin, PinMode.Input);
        _pin = pin;
    }

    private int Update()
    {
        if (DateTime.Now - syncTime > TimeSpan.FromMilliseconds(50.0))
        {
            state = Robot.AnalogRead(Convert.ToByte(_pin));
            syncTime = DateTime.Now;
            return state;
        }

        return -1;
    }

    public bool Watch()
    {
        int sensorValue = Update();

        if (sensorValue == -1)
        {
            return false;
        }
        return sensorValue > 900;
    }
}
