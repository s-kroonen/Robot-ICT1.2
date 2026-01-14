using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

public class ColorSystem : IUpdatable
{

    private PeriodTimer scanIntervalTimer = new PeriodTimer(100);
    private RGBSensor rGBSensor = new RGBSensor(0x29,RGBSensor.IntegrationTime.INTEGRATION_TIME_700MS,RGBSensor.Gain.GAIN_4X);
    private ushort r, g, b, c;
    public string color {get; private set;}
    public ColorSystem()
    {
        rGBSensor.Begin();
    }


    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            rGBSensor.GetRawData(out r, out g, out b, out c);
            color = DetectColor(r,g,b);
        }

    }

    public static string DetectColor(int red, int green, int blue)
    {
        // Normalize the RGB values to the range 0-1
        double total = red + green + blue;
        if (total == 0)
            return "Black"; // No light detected

        double normalizedRed = red / total;
        double normalizedGreen = green / total;
        double normalizedBlue = blue / total;
        // Console.WriteLine(normalizedRed.ToString() + "." + normalizedGreen.ToString() + "." + normalizedBlue.ToString() + "." + total.ToString());

        // Define thresholds for color detection
        if (normalizedRed > 0.4 && normalizedGreen < 0.3 && normalizedBlue < 0.3)
            return "Red";
        else if (normalizedGreen > 0.35 && normalizedRed < 0.35 && normalizedBlue < 0.35)
            return "Green";
        else if (normalizedBlue > 0.3 && normalizedRed < 0.4 && normalizedGreen < 0.4)
            return "Blue";
        else if (normalizedRed > 0.3 && normalizedGreen > 0.4 && normalizedBlue < 0.3)
            return "Yellow";
        else if (normalizedRed > 0.4 && normalizedBlue > 0.5 && normalizedGreen < 0.4)
            return "Magenta";
        else if (normalizedGreen > 0.4 && normalizedBlue > 0.5 && normalizedRed < 0.4)
            return "Cyan";
        else if (normalizedRed > 0.3 && normalizedGreen > 0.3 && normalizedBlue > 0.3)
            return "White";
        else
            return "Uncertain"; // Handle cases where the color is not clear
    }
}