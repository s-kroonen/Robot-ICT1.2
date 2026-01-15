namespace RobotApp.Services.Mqtt
{
    public record TemperatureDto(
    string value,
    string unit,
    DateTime timestamp);

    public record HumidityDto(
        string value,
        string unit,
        DateTime timestamp);

    public record StateDto(
        string state,
        string displayState,
        string message,
        int stateDurationMs,
        DateTime timestamp);

    public record AlertDto(
        bool active,
        string message,
        DateTime timestamp);

}
