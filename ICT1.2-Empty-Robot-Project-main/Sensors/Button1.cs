using System.Device.Gpio;

namespace Avans.StatisticalRobot;

public class Button1
{
    private readonly int _pin;
    private readonly bool _defHigh;
    public Button1(int pin, bool defHigh = false)
    {
        Robot.SetDigitalPinMode(pin, PinMode.Input);
        _pin = pin;
        _defHigh = defHigh;
    }

    public bool GetState()
    {
        return Robot.ReadDigitalPin(_pin) == PinValue.High;
    }
}