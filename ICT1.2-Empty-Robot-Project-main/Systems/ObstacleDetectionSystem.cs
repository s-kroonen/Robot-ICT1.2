using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class ObstacleDetectionSystem : IUpdatable
{
    const int forward_pin = 5, right_pin = 16, left_pin = 18;
    const int ScanIntervalMilliseconds = 200;
    public Ultrasonic_2pin Ultrasonic_foward, Ultrasonic_right, Ultrasonic_left; 
    private PeriodTimer scanIntervalTimer;
    public int ObstacleDistance { get; private set; }

    public ObstacleDetectionSystem()
    {
        Console.WriteLine("DEBUG: ObstacleDetectionSystem constructor called");
        Ultrasonic_right = new Ultrasonic_2pin(right_pin);
        Ultrasonic_left = new Ultrasonic_2pin(left_pin);
        Ultrasonic_foward = new Ultrasonic_2pin(forward_pin);
        scanIntervalTimer = new PeriodTimer(ScanIntervalMilliseconds);
    }
    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            ObstacleDistance = Math.Min(Ultrasonic_right.GetUltrasoneDistance(),Math.Min(Ultrasonic_left.GetUltrasoneDistance(),Ultrasonic_foward.GetUltrasoneDistance()));
        }
    }
}