namespace RobotApp.Models.Measurements
{
    public class HumidityMeasurement
    {
        public int Id { get; set; }
        public string RobotName { get; set; } = default!;
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
