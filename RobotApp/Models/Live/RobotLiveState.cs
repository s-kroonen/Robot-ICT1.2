namespace RobotApp.Models.Live
{
    public class RobotLiveState
    {
        public string Robot { get; set; } = default!;

        public string State { get; set; } = "unknown";
        public string DisplayState { get; set; } = "";
        public string Message { get; set; } = "";

        public double? Temperature { get; set; }
        public double? Humidity { get; set; }

        public bool AlertActive { get; set; }
        public string AlertMessage { get; set; } = "";

        public int? BatteryPercentage { get; set; } // if available later

        public DateTime LastUpdate { get; set; }
    }

}
