using RobotApp.Models.Measurements;

namespace RobotApp.Repositories;

public interface IMeasurementRepository
{
    Task SaveStateAsync(StateSnapshot snapshot);
    Task SaveTemperatureAsync(TemperatureMeasurement measurement);
    Task SaveHumidityAsync(HumidityMeasurement measurement);
}
