using System;
using System.Diagnostics;
using System.Reflection;
using System.Device.Gpio;
using System.Device.I2c;
using GyroscopeCompass;
using Avans.StatisticalRobot;
using GyroscopeCompass.GyroscopeCompass;

WheeledRobot wheeledRobot = new WheeledRobot();

while (true)
{
    wheeledRobot.Update();
    Robot.Wait(1);
}