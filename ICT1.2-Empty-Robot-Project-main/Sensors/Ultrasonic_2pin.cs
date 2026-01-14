using System.Device.Gpio;

namespace Avans.StatisticalRobot
{
    public class Ultrasonic_2pin
    {
        private readonly int _pin_trigger;
        private readonly int _pin_echo;

        /// <summary>
        /// This is a digital device
        /// 3.3V/5V
        /// Detecting range: 0-4m
        /// Resolution: 1cm
        /// </summary>
        /// <param name="pin">Pin number on grove board</param>
        public Ultrasonic_2pin(int pin)
        {
            Robot.SetDigitalPinMode(pin, PinMode.Output);
            Robot.SetDigitalPinMode(pin+1, PinMode.Input);
            _pin_trigger = pin;
            _pin_echo = pin+1;
        }


        public int GetUltrasoneDistance()
        {
            Robot.WriteDigitalPin(_pin_trigger, PinValue.Low);
            Robot.Wait(1);
            Robot.WriteDigitalPin(_pin_trigger, PinValue.High);
            Robot.WaitUs(10);
            Robot.WriteDigitalPin(_pin_trigger, PinValue.Low);

            // int pulseIn = BoeBot.pulseIn(this.echoPin, true, this.timeout);
            // if (pulseIn > 0) {
            //     double afstand = (pulseIn/1000000.0)*343;
            //     this.afstand = afstand;
            // }
            int pulse = Robot.PulseIn(_pin_echo, PinValue.High, 200);
            // Console.WriteLine(pulse);
            return pulse / 29 / 2;
        }
    }
}