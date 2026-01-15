using RobotApp.Data;
using RobotApp.Models.Measurements;

namespace RobotApp.Repositories;

public class MeasurementRepository : IMeasurementRepository
{
    private readonly RobotDbContext _db;

    public MeasurementRepository(RobotDbContext db)
    {
        _db = db;
    }

    public async Task SaveStateAsync(StateSnapshot snapshot)
    {
        _db.StateSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
    }

    public async Task SaveTemperatureAsync(TemperatureMeasurement measurement)
    {
        _db.TemperatureMeasurements.Add(measurement);
        await _db.SaveChangesAsync();
    }

    public async Task SaveHumidityAsync(HumidityMeasurement measurement)
    {
        _db.HumidityMeasurements.Add(measurement);
        await _db.SaveChangesAsync();
    }
}
