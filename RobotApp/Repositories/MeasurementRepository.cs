using Microsoft.EntityFrameworkCore;
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
    public async Task<IReadOnlyList<TemperatureMeasurement>> GetTemperatureAsync(
        string robot, DateTime from, DateTime to)
    {
        return await _db.TemperatureMeasurements
            .Where(t => t.RobotName == robot &&
                        t.Timestamp >= from &&
                        t.Timestamp <= to)
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<HumidityMeasurement>> GetHumidityAsync(
        string robot, DateTime from, DateTime to)
    {
        return await _db.HumidityMeasurements
            .Where(h => h.RobotName == robot &&
                        h.Timestamp >= from &&
                        h.Timestamp <= to)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StateSnapshot>> GetStatesAsync(
        string robot, DateTime from, DateTime to)
    {
        return await _db.StateSnapshots
            .Where(s => s.RobotName == robot &&
                        s.Timestamp >= from &&
                        s.Timestamp <= to)
            .OrderByDescending(s => s.Timestamp)
            .ToListAsync();
    }
}
