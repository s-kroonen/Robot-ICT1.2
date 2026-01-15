using Microsoft.EntityFrameworkCore;
using RobotApp.Data;
using RobotApp.Models;

namespace RobotApp.Repositories;

public class RobotRepository : IRobotRepository
{
    private readonly RobotDbContext _db;

    public RobotRepository(RobotDbContext db)
    {
        _db = db;
    }

    public async Task<Robot?> GetByNameAsync(string name)
    {
        return await _db.Robots.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Robot> GetOrCreateAsync(string name)
    {
        var robot = await GetByNameAsync(name);
        if (robot != null)
            return robot;

        robot = new Robot { Name = name };
        _db.Robots.Add(robot);
        await _db.SaveChangesAsync();

        return robot;
    }
}
