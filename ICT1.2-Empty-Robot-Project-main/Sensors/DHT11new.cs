using Avans.StatisticalRobot;
using System.Device.Gpio;

public class DHT11new
{
    private readonly int _pin;

    /// <summary>
    /// Initializes a new instance of the DHT11 sensor.
    /// </summary>
    /// <param name="pin">Pin number on the board</param>
    public DHT11new(int pin)
    {
        Robot.SetDigitalPinMode(pin, PinMode.Output);
        _pin = pin;
    }

    public (int Temperature, int Humidity)? GetTemperatureAndHumidity()
    {
        int[] pulses = ReadPulses();
        if (pulses == null || pulses.Length < 40)
            return null;

        int[] data = DecodePulses(pulses);
        if (data[4] == ((data[0] + data[1] + data[2] + data[3]) & 0xFF))
        {
            return (data[2], data[0]);
        }

        return null;
    }

    private int[] ReadPulses()
    {
        const int pulseCount = 40;
        int[] pulses = new int[pulseCount];

        Robot.SetDigitalPinMode(_pin, PinMode.Output);
        Robot.WriteDigitalPin(_pin, PinValue.Low);
        Robot.Wait(18);
        Robot.WriteDigitalPin(_pin, PinValue.High);
        Robot.WaitUs(30);
        Robot.SetDigitalPinMode(_pin, PinMode.Input);

        if (Robot.PulseIn(_pin, PinValue.High, 100) > 80)
            for (int i = 0; i < pulseCount; i++)
            {
                pulses[i] = Robot.PulseIn(_pin, PinValue.High, 100);
                if (pulses[i] == 0)
                    return [];
            }
        return pulses;
    }

    private int[] DecodePulses(int[] pulses)
    {
        int[] data = new int[5];
        int threshold = 50;

        for (int i = 0; i < pulses.Length; i++)
        {
            int byteIndex = i / 8;
            int bitPosition = 7 - (i % 8);
            if (pulses[i] > threshold)
            {
                data[byteIndex] |= 1 << bitPosition;
            }
        }

        return data;
    }
}
