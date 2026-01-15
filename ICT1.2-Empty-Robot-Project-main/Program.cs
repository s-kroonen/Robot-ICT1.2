using System;
using System.Diagnostics;
using System.Reflection;
using System.Device.Gpio;
using System.Device.I2c;
using GyroscopeCompass;
using Avans.StatisticalRobot;
using GyroscopeCompass.GyroscopeCompass;

try
{
    var wheeledRobot = new WheeledRobot();
    
    // Initialize the robot asynchronously
    await wheeledRobot.Init();
    
    // Main robot loop
    while (true)
    {
        wheeledRobot.Update();
        Robot.Wait(1);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}