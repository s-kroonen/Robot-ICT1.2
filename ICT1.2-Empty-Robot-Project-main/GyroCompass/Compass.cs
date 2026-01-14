using System.Device.I2c;

namespace GyroscopeCompass.Compass
{
    public enum CompassMode
    {
        PowerDown = 0x00,
        SingleMeasurement = 0x01,   
        Continuous10Hz = 0x02,
        Continuous20Hz = 0x04,
        Continuous50Hz = 0x06,
        Continuous100Hz = 0x08,
        SelfTest = 0x10
    }

    public enum CompassError
    {
        Ok,
        DataSkipped,
        NotReady,
        Timeout,
        SelfTestFailed,
        Overflow,
        WriteFailed,
        ReadFailed
    }

    /// <summary>
    /// Class to interact with the AK09918 Electronic compass.
    /// Provides methods for initialization, configuration, and data retrieval.
    /// </summary>
    public class Magnetometer(I2cDevice device)
    {
        private const byte DeviceAddress = 0x0C;

        private const byte RegisterST1 = 0x10;
        private const byte RegisterHXL = 0x11;
        private const byte RegisterCNTL2 = 0x31;
        private I2cDevice _i2cDevice = device;
        private CompassMode _currentMode = CompassMode.PowerDown;

        /// <summary>
        /// Initializes the compass with the specified mode.
        /// </summary>
        /// <param name="mode">The mode in which the compass gets initialized.</param>
        /// <returns>If the process was succesfull or what the error is.</returns>
        public CompassError Initialize(CompassMode mode)
        {
            if (SwitchMode(mode) != CompassError.Ok)
            {
                return CompassError.WriteFailed;
            }

            Thread.Sleep(100); // Wait for the sensor to be ready
            return CompassError.Ok;
        }

        /// <summary>
        /// Configures the mode of the sensor.
        /// </summary>
        /// <param name="mode">The mode to which the compass needs to be switched to.</param>
        /// <returns>If the process was succesfull or what the error is.</returns>
        public CompassError SwitchMode(CompassMode mode)
        {
            if (!WriteByte(RegisterCNTL2, (byte)mode))
            {
                return CompassError.WriteFailed;
            }

            _currentMode = mode;
            return CompassError.Ok;
        }

        /// <summary>
        /// Checks if the data is ready to be read.
        /// </summary>
        /// <returns>If the process was succesfull or what the error is.</returns>
        public CompassError IsDataReady()
        {
            if (!ReadByte(RegisterST1, out byte data))
            {
                return CompassError.ReadFailed;
            }

            return (data & 0x01) != 0 ? CompassError.Ok : CompassError.NotReady;
        }

        /// <summary>
        /// Reads the magnetometer data from the sensor.
        /// </summary>
        /// <param name="x">Returns the x-axis value scaled.</param>
        /// <param name="y">Returns the y-axis value scaled.</param>
        /// <param name="z">Returns the z-axis value scaled.</param>
        /// <returns>If the process was succesfull or what the error is.</returns>
        public CompassError GetMagnetData(out float x, out float y, out float z)
        {
            x = y = z = 0;

            // Wait for data to be ready
            for (int i = 0; i < 10; i++) // Retry 10 times
            {
                var status = IsDataReady();
                if (status == CompassError.Ok)
                {
                    break;
                }
                else if (i == 9) // Timeout
                {
                    return CompassError.Timeout;
                }
                Thread.Sleep(10); // Wait before retrying
            }

            // Read data
            if (!ReadBytes(RegisterHXL, out byte[] buffer, 8)) // Read 8 bytes starting from HXL
            {
                return CompassError.ReadFailed;
            }

            // Combine high and low bytes for each axis
            int rawX = (short)((buffer[1] << 8) | buffer[0]); // Combine HXH and HXL
            int rawY = (short)((buffer[3] << 8) | buffer[2]); // Combine HYH and HYL
            int rawZ = (short)((buffer[5] << 8) | buffer[4]); // Combine HZH and HZL

            // Scale raw data to µT and round to 2 decimal places
            x = (float)Math.Round(rawX * 0.15f, 2);
            y = (float)Math.Round(rawY * 0.15f, 2);
            z = (float)Math.Round(rawZ * 0.15f, 2);

            // Check for overflow
            if ((buffer[7] & 0x08) != 0) // HOFL bit
            {
                return CompassError.Overflow;
            }

            return CompassError.Ok;
        }


        private bool WriteByte(byte register, byte value)
        {
            try
            {
                _i2cDevice.Write(new byte[] { register, value });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I2C Write failed: {ex.Message}");
                return false;
            }
        }

        private bool ReadByte(byte register, out byte value)
        {
            try
            {
                _i2cDevice.WriteByte(register);
                value = _i2cDevice.ReadByte();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I2C Read failed: {ex.Message}");
                value = 0;
                return false;
            }
        }

        private bool ReadBytes(byte register, byte[] buffer, int offset, int length)
        {
            try
            {
                var readBuffer = new byte[length];
                _i2cDevice.WriteByte(register);
                _i2cDevice.Read(readBuffer);

                Array.Copy(readBuffer, 0, buffer, offset, length);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"I2C ReadBytes failed: {ex.Message}");
                return false;
            }
        }

        private bool ReadBytes(byte register, out byte[] buffer, int length)
        {
            buffer = new byte[length];
            return ReadBytes(register, buffer, 0, length);
        }
    }
}
