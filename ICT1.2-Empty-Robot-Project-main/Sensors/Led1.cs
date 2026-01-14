using System.Device.Gpio;

namespace Avans.StatisticalRobot;

public class Led1
{
    private readonly int _pin;
    public Led1(int pin)
    {
        Robot.SetDigitalPinMode(pin, PinMode.Output);
        _pin = pin;
    }

    public void SetOn()
    {
        Robot.WriteDigitalPin(_pin, PinValue.High);
    }
    public void SetOff()
    {
        Robot.WriteDigitalPin(_pin, PinValue.Low);
    }
    public void SetState(bool state)
    {
        Robot.WriteDigitalPin(_pin, state ? PinValue.Low : PinValue.High);
    }
}