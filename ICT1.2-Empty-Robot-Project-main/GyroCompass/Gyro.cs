using System.Device.I2c;
namespace GyroscopeCompass.Gyroscope
{

    public static class GyroConstants
    {
        public const byte CONFIG = 0x1A;
        public const byte FIFO_EN = 0x23;
        public const byte WHO_AM_I = 0x75;
        public const byte PWR_MGMT_1 = 0x6B;
        public const byte PWR_MGMT_2 = 0x6C;
        public const byte ACCEL_CONFIG = 0x1C;
        public const byte ACCEL_CONFIG2 = 0x1D;
        public const byte GYRO_CONFIG = 0x1B;
        public const byte GYRO_LP_MODE_CFG = 0x1E;
        public const byte SMPLRT_DIV = 0x19;
        // public const byte ACCEL_XOUT_H = 0x3B; THESE ARE THE ORIGINAL VALUES AND SHOULD NOT BE ALTERED
        // public const byte ACCEL_YOUT_H = 0x3D;
        // public const byte ACCEL_ZOUT_H = 0x3F;
        // public const byte GYRO_XOUT_H = 0x43;
        // public const byte GYRO_YOUT_H = 0x45;
        // public const byte GYRO_ZOUT_H = 0x47;
        public const byte ACCEL_XOUT_H = 0x43; // There seems to be either a documentation error or a hardware error where the registers are not correct
        public const byte ACCEL_YOUT_H = 0x45;
        public const byte ACCEL_ZOUT_H = 0x47;
        public const byte GYRO_XOUT_H = 0x3B;
        public const byte GYRO_YOUT_H = 0x3D;
        public const byte GYRO_ZOUT_H = 0x4F;

        //Preserve V
        public const byte TEMP_OUT_H = 0x41;

        public enum PowerModes
        {
            Sleep,
            Standby,
            LowPowerAccelerometer,
            LowNoiseAccelerometer,
            LowPowerGyroscope,
            LowNoiseGyroscope,
            LowPower6Axis,
            LowNoise6Axis
        }

        public enum GyroRange
        {
            RANGE_250_DPS,
            RANGE_500_DPS,
            RANGE_1000_DPS,
            RANGE_2000_DPS
        }

        public enum AccelRange
        {
            RANGE_2G,
            RANGE_4G,
            RANGE_8G,
            RANGE_16G
        }
    }

    /// <summary>
    /// Class to interact with the ICM20600 6-axis IMU sensor.
    /// Provides methods for initialization, configuration, and data retrieval.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ICM20600 class.
    /// </remarks>
    /// <param name="device">The I2C device for communication.</param>
    /// <param name="useAlternateAddress">Use alternate I2C address (default: false).</param>
    public class Gyro(I2cDevice device, bool useAlternateAddress = false)
    {
        private readonly int _address = useAlternateAddress ? 0x69 : 0x68;
        private readonly I2cDevice _device = device;
        private int _accScale;
        private int _gyroScale;

        /// <summary>
        /// Initializes the sensor with default configurations.
        /// </summary>
        public void Initialize()
        {
            // Set default configurations for the device
            WriteByte(GyroConstants.CONFIG, 0x00);
            WriteByte(GyroConstants.FIFO_EN, 0x00);

            // Set default power mode and sensitivity ranges
            SetPowerMode(GyroConstants.PowerModes.LowNoise6Axis);
            SetGyroScaleRange(GyroConstants.GyroRange.RANGE_2000_DPS);
            SetAccelScaleRange(GyroConstants.AccelRange.RANGE_16G);
        }

        /// <summary>
        /// Reads the device ID from the WHO_AM_I register.
        /// </summary>
        /// <returns>Device ID as a byte.</returns>
        public byte GetDeviceID()
        {
            return ReadByte(GyroConstants.WHO_AM_I); // WHO_AM_I register contains the device ID
        }

        /// <summary>
        /// Configures the power mode of the sensor.
        /// </summary>
        /// <param name="mode">The power mode to set.</param>
        public void SetPowerMode(GyroConstants.PowerModes mode)
        {
            byte power1 = ReadByte(GyroConstants.PWR_MGMT_1);
            byte power2 = 0x00; // Secondary power management register
            power1 &= 0x8F; // Clear the mode bits (bits 4–6)

            switch (mode)
            {
                case GyroConstants.PowerModes.Sleep:
                    power1 |= 0x40; // Set sleep mode
                    break;
                case GyroConstants.PowerModes.Standby:
                    power1 |= 0x10; // Set standby mode
                    power2 = 0x38; // Disable accelerometer
                    break;
                case GyroConstants.PowerModes.LowPowerAccelerometer:
                    power1 |= 0x20; // Low-power accelerometer mode
                    power2 = 0x07; // Disable gyroscope
                    break;
                case GyroConstants.PowerModes.LowNoiseAccelerometer:
                    power1 |= 0x00; // Low-noise accelerometer mode
                    power2 = 0x07; // Disable gyroscope
                    break;
                default:
                    break;
            }

            // Write the configuration to the registers
            WriteByte(GyroConstants.PWR_MGMT_1, power1);
            WriteByte(GyroConstants.PWR_MGMT_2, power2);
        }

        /// <summary>
        /// Sets the gyroscope sensitivity range.
        /// </summary>
        /// <param name="range">The sensitivity range to set.</param>
        public void SetGyroScaleRange(GyroConstants.GyroRange range)
        {
            byte data = ReadByte(GyroConstants.GYRO_CONFIG);
            data &= 0xE7; // Clear the sensitivity bits (bits 3–4)

            switch (range)
            {
                case GyroConstants.GyroRange.RANGE_250_DPS:
                    _gyroScale = 500; // Sensitivity scaling factor
                    break;
                case GyroConstants.GyroRange.RANGE_500_DPS:
                    data |= 0x08;
                    _gyroScale = 1000;
                    break;
                case GyroConstants.GyroRange.RANGE_1000_DPS:
                    data |= 0x10;
                    _gyroScale = 2000;
                    break;
                case GyroConstants.GyroRange.RANGE_2000_DPS:
                    data |= 0x18;
                    _gyroScale = 4000;
                    break;
            }

            WriteByte(GyroConstants.GYRO_CONFIG, data);
        }

        /// <summary>
        /// Sets the accelerometer sensitivity range.
        /// </summary>
        /// <param name="range">The sensitivity range to set.</param>
        public void SetAccelScaleRange(GyroConstants.AccelRange range)
        {
            byte data = ReadByte(GyroConstants.ACCEL_CONFIG);
            data &= 0xE7; // Clear the sensitivity bits (bits 3–4)

            switch (range)
            {
                case GyroConstants.AccelRange.RANGE_2G:
                    _accScale = 4000;
                    break;
                case GyroConstants.AccelRange.RANGE_4G:
                    data |= 0x08;
                    _accScale = 8000;
                    break;
                case GyroConstants.AccelRange.RANGE_8G:
                    data |= 0x10;
                    _accScale = 16000;
                    break;
                case GyroConstants.AccelRange.RANGE_16G:
                    data |= 0x18;
                    _accScale = 32000;
                    break;
            }

            WriteByte(GyroConstants.ACCEL_CONFIG, data);
        }

        /// <summary>
        /// Reads the scaled acceleration value for the X-axis.
        /// </summary>
        /// <returns>Acceleration in mg (milligravity).</returns>
        public int GetAccelerationX() => ScaleAcceleration(GetRawAccelerationX());

        /// <summary>
        /// Reads the scaled acceleration value for the Y-axis.
        /// </summary>
        /// <returns>Acceleration in mg (milligravity).</returns>
        public int GetAccelerationY() => ScaleAcceleration(GetRawAccelerationY());

        /// <summary>
        /// Reads the scaled acceleration value for the Z-axis.
        /// </summary>
        /// <returns>Acceleration in mg (milligravity).</returns>
        public int GetAccelerationZ() => ScaleAcceleration(GetRawAccelerationZ());

        private int GetRawAccelerationX() => ReadWord(GyroConstants.ACCEL_XOUT_H);
        private int GetRawAccelerationY() => ReadWord(GyroConstants.ACCEL_YOUT_H);
        private int GetRawAccelerationZ() => ReadWord(GyroConstants.ACCEL_ZOUT_H);

        private int ScaleAcceleration(int rawData)
        {
            // Scale raw data to milligravity using sensitivity scaling
            return (rawData * _accScale) >> 16;
        }
        /// <summary>
        /// Reads the scaled gyroscope value for the X-axis.
        /// </summary>
        /// <returns>Angular velocity in degrees per second (dps).</returns>
        public int GetGyroscopeX() => ScaleGyroscope(GetRawGyroscopeX());

        /// <summary>
        /// Reads the scaled gyroscope value for the Y-axis.
        /// </summary>
        /// <returns>Angular velocity in degrees per second (dps).</returns>
        public int GetGyroscopeY() => ScaleGyroscope(GetRawGyroscopeY());

        /// <summary>
        /// Reads the scaled gyroscope value for the Z-axis.
        /// </summary>
        /// <returns>Angular velocity in degrees per second (dps).</returns>
        public int GetGyroscopeZ() => ScaleGyroscope(GetRawGyroscopeZ());
        private int GetRawGyroscopeX() => ReadWord(GyroConstants.GYRO_XOUT_H);
        private int GetRawGyroscopeY() => ReadWord(GyroConstants.GYRO_YOUT_H);
        private int GetRawGyroscopeZ() => ReadWord(GyroConstants.GYRO_ZOUT_H);

        /// <summary>
        /// Scales the raw gyroscope data to degrees per second (dps) based on the configured sensitivity range.
        /// </summary>
        /// <param name="rawData">Raw gyroscope data read from the sensor.</param>
        /// <returns>Scaled angular velocity in degrees per second (dps).</returns>
        private int ScaleGyroscope(int rawData)
        {
            // Scale raw gyroscope data based on the configured sensitivity range.
            // The scaling factor (_gyroScale) is set in the SetGyroScaleRange method.
            return (rawData * _gyroScale) >> 16;
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <returns>Temperature in degrees Celsius.</returns>
        public int GetTemperature()
        {
            int rawTemp = ReadWord(GyroConstants.TEMP_OUT_H);
            return rawTemp / 327 + 25; // Convert to Celsius
        }

        private byte ReadByte(byte register)
        {
            Span<byte> buffer = stackalloc byte[1];
            _device.WriteRead(new[] { register }, buffer);
            return buffer[0]; // Return the single byte read
        }

        private int ReadWord(byte highRegister)
        {
            Span<byte> buffer = stackalloc byte[2];
            _device.WriteRead(new[] { highRegister }, buffer);

            // Combine high and low bytes into a signed 16-bit integer
            return (short)((buffer[0] << 8) | buffer[1]);
        }

        private void WriteByte(byte register, byte value)
        {
            _device.Write(new[] { register, value }); // Write register and value to the device
        }
    }
}