using GyroscopeCompass.Gyroscope;
using GyroscopeCompass.Compass;
using Avans.StatisticalRobot;
using System.Device.I2c;

// TODO Write documentation
namespace GyroscopeCompass.GyroscopeCompass
{
    public enum PerformanceMode
    {
        Sleep,
        Normal,
        Fast
    }

    public enum Range
    {
        Low,
        Medium,
        High
    }
    public class GyroCompass
    {
        private Gyro gyro;
        private Magnetometer compass;

        /// <summary>
        /// Initializes a new instance of the <see cref="GyroCompass"/> class.
        /// This class is used to interact with the ICM20600 6-axis IMU sensor and the AK09918 Electronic compass.
        /// Initializes in high range and fast mode.
        /// </summary>
        public GyroCompass()
        {
            gyro = new Gyroscope.Gyro(OccupiedId(0x68) ? Robot.CreateI2cDevice(0x68) : Robot.CreateI2cDevice(0x69));
            compass = new Magnetometer(Robot.CreateI2cDevice(0x0c));
            gyro.Initialize();
            compass.Initialize(CompassMode.Continuous100Hz);
        }

        /// <summary>
        /// Gets the acceleration data from the gyroscope.
        /// </summary>
        /// <param name="x">Returns the scaled acceleration value for the X-axis in mg (miligravity)</param>
        /// <param name="y">Returns the scaled acceleration value for the y-axis in mg (miligravity</param>
        /// <param name="z">Returns the scaled acceleration value for the z-axis in mg (miligravity</param>
        public void GetGyroAcceleration(out float x, out float y, out float z)
        {
            x = gyro.GetAccelerationX();
            y = gyro.GetAccelerationY();
            z = gyro.GetAccelerationZ();
        }

        /// <summary>
        /// Gets the angular velocity data from the gyroscope.
        /// </summary>
        /// <param name="x">Returns the scaled angular velocity value for the X-axis in degrees per second (dps).</param>
        /// <param name="y">Returns the scaled angular velocity value for the Y-axis in degrees per second (dps).</param>
        /// <param name="z">Returns the scaled angular velocity value for the Z-axis in degrees per second (dps).</param>
        public void GetGyroAngularVelocity(out float x, out float y, out float z)
        {
            x = gyro.GetGyroscopeX();
            y = gyro.GetGyroscopeY();
            z = gyro.GetGyroscopeZ();
        }

        /// <summary>
        /// Gets the temperature of the gyroscope.
        /// </summary>
        /// <returns>Temperature in Celsius.</returns>
        public int GetTemperature()
        {
            return gyro.GetTemperature();
        }

        /// <summary>
        /// Sets the performance mode of the gyroscope.
        /// </summary>
        /// <param name="mode"> Sleep, Normal or Fast</param>
        public void SetGyroMode(PerformanceMode mode)
        {
            switch (mode)
            {
                case PerformanceMode.Sleep:
                    gyro.SetPowerMode(GyroConstants.PowerModes.Sleep);
                    break;
                case PerformanceMode.Normal:
                    gyro.SetPowerMode(GyroConstants.PowerModes.LowPower6Axis);
                    break;
                case PerformanceMode.Fast:
                    gyro.SetPowerMode(GyroConstants.PowerModes.LowNoise6Axis);
                    break;
            }
        }

        /// <summary>
        /// Sets the range of the gyroscope.
        /// </summary>
        /// <param name="range">Low, Medium or High range</param>
        public void SetGyroRange(Range range)
        {
            switch (range)
            {
                case Range.Low:
                    gyro.SetAccelScaleRange(GyroConstants.AccelRange.RANGE_4G);
                    gyro.SetGyroScaleRange(GyroConstants.GyroRange.RANGE_500_DPS);
                    break;
                case Range.Medium:
                    gyro.SetAccelScaleRange(GyroConstants.AccelRange.RANGE_8G);
                    gyro.SetGyroScaleRange(GyroConstants.GyroRange.RANGE_1000_DPS);
                    break;
                case Range.High:
                    gyro.SetAccelScaleRange(GyroConstants.AccelRange.RANGE_16G);
                    gyro.SetGyroScaleRange(GyroConstants.GyroRange.RANGE_2000_DPS);
                    break;
            }
        }

        /// <summary>Gets the magnetometer data from the compass.</summary>
        /// <param name="x">Returns the scaled magnetometer value for the X-axis in microtesla (uT).</param>
        /// <param name="y">Returns the scaled magnetometer value for the Y-axis in microtesla (uT).</param>
        /// <param name="z">Returns the scaled magnetometer value for the Z-axis in microtesla (uT).</param>
        public void GetMagnetData(out float x, out float y, out float z)
        {
            compass.GetMagnetData(out x, out y, out z);
        }

        /// <summary>
        /// Sets the performance mode of the compass.
        /// </summary>
        /// <param name="mode">Sleep, Normal or Fast</param>
        public void SetCompassMode(PerformanceMode mode)
        {
            switch (mode)
            {
                case PerformanceMode.Sleep:
                    compass.SwitchMode(CompassMode.PowerDown);
                    break;
                case PerformanceMode.Normal:
                    compass.SwitchMode(CompassMode.Continuous20Hz);
                    break;
                case PerformanceMode.Fast:
                    compass.SwitchMode(CompassMode.Continuous100Hz);
                    break;
            }

        }

        /// <summary>
        ///  Scans the I2C bus for occupied addresses.
        /// </summary>
        /// <param name="id">ID to be checked</param>
        /// <returns>A boolean if the specified address is occupied (true if occupied)</returns>
        private static bool OccupiedId(int id)
        {
            int busId = 1;
            try
            {
                var settings = new I2cConnectionSettings(busId, id);
                using var device = I2cDevice.Create(settings);
                device.WriteByte(0x00);
                Console.WriteLine($"Device found at address: {id}. Using alternate ID");
                return true;
            }
            catch
            {
                Console.WriteLine($"No device found at address: {id}. Mounting..");
                return false;
            }
        }
    }
}