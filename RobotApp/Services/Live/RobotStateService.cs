using RobotApp.Models.Live;

public class RobotStateService
{
    private readonly Dictionary<string, RobotLiveState> _robots = new();

    public event Action? OnChange;

    public IReadOnlyCollection<string> Robots => _robots.Keys;

    public RobotLiveState Get(string robot)
    {
        if (!_robots.ContainsKey(robot))
        {
            _robots[robot] = new RobotLiveState { Robot = robot };
            Notify();
        }

        return _robots[robot];
    }

    public void Update(string robot, Action<RobotLiveState> update)
    {
        var state = Get(robot);
        update(state);
        state.LastUpdate = DateTime.UtcNow;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}
