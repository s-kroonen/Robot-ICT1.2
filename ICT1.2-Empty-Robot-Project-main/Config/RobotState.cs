using System;

/// <summary>
/// Enumeration for robot operational states
/// </summary>
public enum RobotStateEnum
{
    Initializing,      // Robot is starting up
    Ready,             // Robot is ready but stationary
    Operating,         // Robot is actively operating
    Stopped,           // Robot stopped due to obstacle
    EmergencyStopped,  // Robot emergency stopped
    Fault,             // Robot encountered a fault
    Offline            // Robot is offline
}

/// <summary>
/// Manages the robot's operational state
/// </summary>
public class StateManager
{
    private RobotStateEnum currentState = RobotStateEnum.Initializing;
    private DateTime lastStateChange = DateTime.UtcNow;
    private string stateMessage = string.Empty;

    public event EventHandler<RobotStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Get the current robot state
    /// </summary>
    public RobotStateEnum CurrentState => currentState;

    /// <summary>
    /// Get the current state message
    /// </summary>
    public string StateMessage => stateMessage;

    /// <summary>
    /// Get when the state last changed
    /// </summary>
    public DateTime LastStateChange => lastStateChange;

    /// <summary>
    /// Get the duration in the current state
    /// </summary>
    public TimeSpan CurrentStateDuration => DateTime.UtcNow - lastStateChange;

    /// <summary>
    /// Change the robot state
    /// </summary>
    public void SetState(RobotStateEnum newState, string message = "")
    {
        if (currentState != newState)
        {
            var previousState = currentState;
            currentState = newState;
            lastStateChange = DateTime.UtcNow;
            stateMessage = message;

            StateChanged?.Invoke(this, new RobotStateChangedEventArgs
            {
                PreviousState = previousState,
                NewState = newState,
                Message = message,
                Timestamp = lastStateChange
            });

            Console.WriteLine($"DEBUG: Robot state changed from {previousState} to {newState}. Message: {message}");
        }
    }

    /// <summary>
    /// Get a human-readable state string for display
    /// </summary>
    public string GetDisplayString()
    {
        return currentState switch
        {
            RobotStateEnum.Initializing => "Initializing",
            RobotStateEnum.Ready => "Ready",
            RobotStateEnum.Operating => "Operating",
            RobotStateEnum.Stopped => "Stopped",
            RobotStateEnum.EmergencyStopped => "Emergency Stop",
            RobotStateEnum.Fault => "Fault",
            RobotStateEnum.Offline => "Offline",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get MQTT-friendly state string
    /// </summary>
    public string GetMqttString()
    {
        return currentState.ToString().ToLower();
    }
}

/// <summary>
/// Event arguments for state changes
/// </summary>
public class RobotStateChangedEventArgs : EventArgs
{
    public RobotStateEnum PreviousState { get; set; }
    public RobotStateEnum NewState { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
