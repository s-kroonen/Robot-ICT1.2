using RobotApp.Models.Measurements;

namespace RobotApp.Repositories;

public interface IMeasurementRepository
{
    Task SaveStateAsync(StateSnapshot snapshot);
    Task SaveTemperatureAsync(TemperatureMeasurement measurement);
    Task SaveHumidityAsync(HumidityMeasurement measurement);
    Task<IReadOnlyList<TemperatureMeasurement>> GetTemperatureAsync(
        string robot, DateTime from, DateTime to);

    Task<IReadOnlyList<HumidityMeasurement>> GetHumidityAsync(
        string robot, DateTime from, DateTime to);

    Task<IReadOnlyList<StateSnapshot>> GetStatesAsync(
        string robot, DateTime from, DateTime to);
}
