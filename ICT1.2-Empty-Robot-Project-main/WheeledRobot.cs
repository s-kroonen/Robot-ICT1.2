using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;

/// <summary>
/// Main WheeledRobot class
/// Orchestrates all robot systems and manages state transitions
/// </summary>
public class WheeledRobot : IUpdatable, LineCallback
{
    private readonly RobotConfiguration config;
    private readonly StateManager stateManager;

    // Systems
    private DriveSystem? driveSystem;
    private ObstacleDetectionSystem? obstacleDetectionSystem;
    private AlertSystem? alertSystem;
    private CommunicationSystem? communicationSystem;
    private DataSystem? dataSystem;
    private ColorSystem? colorSystem;
    private LineSystem? lineSystem;

    // Hardware
    private Led? alertLed;
    private Button? emergencyStopButton;
    private LCD16x2? lcd;

    // State tracking
    private bool obstacleStop = false;

    public WheeledRobot()
    {
        Console.WriteLine("DEBUG: WheeledRobot constructor called");

        // Load or create configuration
        config = RobotConfiguration.CreateDefaultConfiguration();
        stateManager = new StateManager();

        try
        {
            // Initialize hardware
            alertLed = new Led(config.AlertLedPin);
            emergencyStopButton = new Button(config.EmergencyStopButtonPin);
            lcd = new LCD16x2(config.LcdI2CAddress);

            // Initialize systems with configuration
            driveSystem = new DriveSystem(config);
            obstacleDetectionSystem = new ObstacleDetectionSystem(config);
            alertSystem = new AlertSystem(config, alertLed, emergencyStopButton, lcd);
            communicationSystem = new CommunicationSystem(this, config);
            dataSystem = new DataSystem(config, communicationSystem);
            colorSystem = new ColorSystem(config);
            lineSystem = new LineSystem(config, this);

            // Wire up event handlers
            alertSystem.EmergencyStopChanged += OnEmergencyStopChanged;
            communicationSystem.CommandReceived += OnCommandReceived;
            communicationSystem.EmergencyStopCommandReceived += OnEmergencyStopCommand;
            communicationSystem.AlertCommandReceived += OnAlertCommand;

            stateManager.SetState(RobotStateEnum.Initializing, "Starting up");
            Console.WriteLine("DEBUG: WheeledRobot systems initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to initialize WheeledRobot: {ex.Message}");
            stateManager.SetState(RobotStateEnum.Fault, $"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialize the robot (async setup like network connections)
    /// </summary>
    public async Task Init()
    {
        Console.WriteLine("DEBUG: WheeledRobot Init() called");
        try
        {
            // Temporarily disable motors for quick testing during debugging
            if (driveSystem != null) driveSystem.DriveActive = false;

            // Configure the CommunicationSystem
            if (communicationSystem != null) await communicationSystem.Init();

            // Display startup message
            if (lcd != null) lcd.SetText("SimpleRobot");
            
            stateManager.SetState(RobotStateEnum.Ready, "Robot ready");
            Console.WriteLine("DEBUG: WheeledRobot Init() finished");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to initialize robot: {ex.Message}");
            stateManager.SetState(RobotStateEnum.Fault, $"Init failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle emergency stop state changes from button or MQTT
    /// </summary>
    private void OnEmergencyStopChanged(object? sender, bool isActive)
    {
        if (isActive)
        {
            stateManager.SetState(RobotStateEnum.EmergencyStopped, "Emergency stop activated");
            driveSystem?.EmergencyStop();
            if (driveSystem != null) driveSystem.DriveActive = false;
        }
        else
        {
            stateManager.SetState(RobotStateEnum.Operating, "Resuming operation");
            if (driveSystem != null) driveSystem.DriveActive = true;
        }
    }

    /// <summary>
    /// Handle emergency stop commands from MQTT
    /// </summary>
    private void OnEmergencyStopCommand(object? sender, bool stopState)
    {
        alertSystem?.SetEmergencyStop(stopState, stopState ? "MQTT Emergency Stop" : "");
    }

    /// <summary>
    /// Handle alert commands from MQTT
    /// </summary>
    private void OnAlertCommand(object? sender, string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            alertSystem?.DisplayMessage(message);
            Console.WriteLine($"DEBUG: Alert command from MQTT: {message}");
        }
    }

    /// <summary>
    /// Handle drive and other commands from MQTT
    /// </summary>
    private void OnCommandReceived(object? sender, MqttCommand cmd)
    {
        Console.WriteLine($"DEBUG: Command received: type={cmd.Type}, direction={cmd.Direction}, value={cmd.Value}");

        switch (cmd.Type)
        {
            case "drive":
                // Handle drive command
                // cmd.Direction: forward, backward, left, right
                // cmd.Value: speed (0.0 - 1.0)
                HandleDriveCommand(cmd.Direction, cmd.Value);
                break;

            case "message":
                // Handle custom message
                alertSystem?.DisplayMessage(cmd.Message);
                break;

            default:
                Console.WriteLine($"DEBUG: Unknown command type: {cmd.Type}");
                break;
        }
    }

    /// <summary>
    /// Handle drive commands from MQTT
    /// </summary>
    private void HandleDriveCommand(string direction, double speed)
    {
        if (stateManager.CurrentState == RobotStateEnum.EmergencyStopped)
        {
            Console.WriteLine("DEBUG: Cannot drive - emergency stop active");
            return;
        }

        speed = Math.Clamp(speed, -1.0, 1.0); // Clamp to valid range

        switch (direction.ToLower())
        {
            case "forward":
                if (driveSystem != null) driveSystem.targetSpeed = speed * config.DriveConfig.MaxSpeed;
                stateManager.SetState(RobotStateEnum.Operating, "Driving forward");
                break;

            case "backward":
                if (driveSystem != null) driveSystem.targetSpeed = -speed * config.DriveConfig.MaxSpeed;
                stateManager.SetState(RobotStateEnum.Operating, "Driving backward");
                break;

            case "stop":
                if (driveSystem != null) driveSystem.targetSpeed = 0.0;
                stateManager.SetState(RobotStateEnum.Ready, "Stopped by command");
                break;

            default:
                Console.WriteLine($"DEBUG: Unknown drive direction: {direction}");
                break;
        }
    }

    public async void Update()
    {
        try
        {
            // Update all systems
            obstacleDetectionSystem?.Update();
            driveSystem?.Update();
            alertSystem?.Update();
            dataSystem?.Update();
            colorSystem?.Update();
            lineSystem?.Update();

            // Publish data to MQTT
            if (communicationSystem != null && obstacleDetectionSystem != null && lineSystem != null && colorSystem != null)
            {
                await communicationSystem.PublishRobotState(stateManager);
                await communicationSystem.PublishObstacleData(obstacleDetectionSystem);
                await communicationSystem.PublishLineData(lineSystem);
                await communicationSystem.PublishColorData(colorSystem.color);
            }

            int distance = obstacleDetectionSystem?.ObstacleDistance ?? 0;

            // Handle obstacle detection
            if (distance < 20 && alertSystem != null && !alertSystem.EmergencyStop && !obstacleStop)
            {
                obstacleStop = true;
                driveSystem?.EmergencyStop();
                if (driveSystem != null) driveSystem.DriveActive = false;
                stateManager.SetState(RobotStateEnum.Stopped, $"Obstacle detected: {distance} cm");
                alertSystem.AlertOn($"Obstacle\nDistance {distance} cm");
                if (communicationSystem != null) await communicationSystem.PublishAlertState(true, $"Obstacle at {distance}cm");
            }
            else if (distance >= 5 && obstacleStop && alertSystem != null && !alertSystem.EmergencyStop)
            {
                obstacleStop = false;
                if (driveSystem != null) driveSystem.DriveActive = true;
                stateManager.SetState(RobotStateEnum.Operating, "Obstacle cleared");
                alertSystem.AlertOff();
                if (communicationSystem != null) await communicationSystem.PublishAlertState(false);
            }

            // Adjust speed based on distance
            if (driveSystem != null)
            {
                if (distance >= 5 && distance < 15)
                {
                    driveSystem.targetSpeed = 0.15;
                }
                else if (distance >= 15 && distance < 40)
                {
                    driveSystem.targetSpeed = 0.2;
                }
                else if (distance >= 40)
                {
                    driveSystem.targetSpeed = 0.2;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Exception in robot Update: {ex.Message}");
            stateManager.SetState(RobotStateEnum.Fault, $"Update error: {ex.Message}");
        }
    }

    /// <summary>
    /// Callback for line detection system
    /// </summary>
    public void lineCallback(bool left, bool forward, bool right)
    {
        Console.WriteLine($"Line detection: left={left}, forward={forward}, right={right}");
        driveSystem?.LineInput(left, forward, right);
    }
}

