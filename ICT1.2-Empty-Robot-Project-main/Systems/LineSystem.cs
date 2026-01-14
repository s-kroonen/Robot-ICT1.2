using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class LineSystem : IUpdatable
{

    private PeriodTimer scanIntervalTimer = new PeriodTimer(10);
    private InfraredReflectiveAnalog forward, left, right;
    private bool ForwardState, RightState, LeftState;
    public LineCallback callback;

    public LineSystem(LineCallback callback)
    {
        this.callback = callback;
        forward = new InfraredReflectiveAnalog(6);
        right = new InfraredReflectiveAnalog(0);
        left = new InfraredReflectiveAnalog(4);
    }


    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            ForwardState = forward.Watch();
            RightState = right.Watch();
            LeftState = left.Watch();
            callback.lineCallback(LeftState, ForwardState, RightState);
        }

    }
}