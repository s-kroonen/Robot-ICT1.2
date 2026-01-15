using RobotApp.Models;

namespace RobotApp.Repositories
{
    public interface IRobotRepository
    {
        Task<Robot> GetOrCreateAsync(string name);
        Task<Robot?> GetByNameAsync(string name);
    }
}
