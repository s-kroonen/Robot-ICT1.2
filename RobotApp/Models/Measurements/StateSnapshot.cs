namespace RobotApp.Models.Measurements
{
    public class StateSnapshot
    {
        public int Id { get; set; }
        public string RobotName { get; set; } = default!;
        public string State { get; set; } = default!;
        public string DisplayState { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }

}
