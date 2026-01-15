
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using HiveMQtt.MQTT5.Types;
using NLog.Layouts;
using SimpleMqtt;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Central communication hub for the robot
/// Handles all MQTT communication including:
/// - Data export (sensors, state)
/// - Remote commands (drive, emergency stop, messages)
/// - Bandwidth-conscious data publishing
/// </summary>
public class CommunicationSystem
{
    private readonly RobotConfiguration config;
    private SimpleMqttClient? mqttClient;
    private WheeledRobot robot;
    private PeriodTimer? dataPublishTimer;
    private PeriodTimer? statusPublishTimer;

    // Topic definitions
    private readonly string topicCommand;
    private readonly string topicAlert;
    private readonly string topicDistance;
    private readonly string topicHumidity;
    private readonly string topicTemperature;
    private readonly string topicColor;
    private readonly string topicState;
    private readonly string topicLineDetection;
    private readonly string topicObstacles;
    private readonly string topicMessage;
    private readonly string topicStatus;
    
    private string clientId;

    /// <summary>
    /// Event triggered when a command is received from MQTT
    /// </summary>
    public event EventHandler<MqttCommand>? CommandReceived;

    /// <summary>
    /// Event triggered when an MQTT alert command is received
    /// </summary>
    public event EventHandler<string>? AlertCommandReceived;

    /// <summary>
    /// Event triggered when an emergency stop command is received
    /// </summary>
    public event EventHandler<bool>? EmergencyStopCommandReceived;

    public CommunicationSystem(WheeledRobot robot, RobotConfiguration config)
    {
        this.robot = robot;
        this.config = config;
        
        // Initialize timers
        try
        {
            dataPublishTimer = new PeriodTimer(config.CommunicationConfig.DataPublishIntervalMs);
            statusPublishTimer = new PeriodTimer(config.CommunicationConfig.StatusPublishIntervalMs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Failed to initialize timers: {ex.Message}");
        }

        // Setup topics
        string baseTopic = config.CommunicationConfig.BaseTopic;
        string robotName = config.RobotName;
        
        topicCommand = $"{baseTopic}/{robotName}/command";
        topicAlert = $"{baseTopic}/{robotName}/alert";
        topicDistance = $"{baseTopic}/{robotName}/distance";
        topicHumidity = $"{baseTopic}/{robotName}/humidity";
        topicTemperature = $"{baseTopic}/{robotName}/temperature";
        topicColor = $"{baseTopic}/{robotName}/color";
        topicState = $"{baseTopic}/{robotName}/state";
        topicLineDetection = $"{baseTopic}/{robotName}/line";
        topicObstacles = $"{baseTopic}/{robotName}/obstacles";
        topicMessage = $"{baseTopic}/{robotName}/message";
        topicStatus = $"{baseTopic}/{robotName}/status";
        
        clientId = $"{Environment.MachineName}-mqtt-client";
        
        try
        {
            mqttClient = SimpleMqttClient.CreateSimpleMqttClientForHiveMQ(clientId);
            mqttClient.OnMessageReceived += MessageCallback;
            Console.WriteLine("DEBUG: SimpleMqttClient created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to create MQTT client: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialize communication system (subscribe to topics)
    /// </summary>
    public async Task Init()
    {
        try
        {
            if (mqttClient != null)
            {
                await mqttClient.SubscribeToTopic(topicCommand);
                Console.WriteLine($"DEBUG: Subscribed to command topic: {topicCommand}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to subscribe to command topic: {ex.Message}");
        }
    }

    /// <summary>
    /// Process incoming MQTT messages
    /// </summary>
    private void MessageCallback(object? sender, SimpleMqttMessage msg)
    {
        Console.WriteLine($"DEBUG: MQTT message received: topic={msg.Topic}, msg={msg.Message}");

        if (msg.Topic == topicCommand && msg.Message != null)
        {
            ProcessCommand(msg.Message);
        }
    }

    /// <summary>
    /// Parse and process command messages
    /// Supports formats like: "drive:forward:0.5", "emergency:stop", "alert:message"
    /// </summary>
    private void ProcessCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        var parts = command.Split(':');
        var commandType = parts[0].ToLower();

        try
        {
            switch (commandType)
            {
                case "drive":
                    if (parts.Length >= 3)
                    {
                        var direction = parts[1].ToLower();
                        if (double.TryParse(parts[2], out double speed))
                        {
                            CommandReceived?.Invoke(this, new MqttCommand 
                            { 
                                Type = "drive", 
                                Direction = direction, 
                                Value = speed 
                            });
                        }
                    }
                    break;

                case "emergency":
                    bool stopState = parts.Length < 2 || parts[1].ToLower() == "stop";
                    EmergencyStopCommandReceived?.Invoke(this, stopState);
                    break;

                case "alert":
                    if (parts.Length >= 2)
                    {
                        string message = string.Join(":", parts.Skip(1));
                        AlertCommandReceived?.Invoke(this, message);
                    }
                    break;

                case "message":
                    if (parts.Length >= 2)
                    {
                        string message = string.Join(":", parts.Skip(1));
                        CommandReceived?.Invoke(this, new MqttCommand 
                        { 
                            Type = "message", 
                            Message = message 
                        });
                    }
                    break;

                default:
                    Console.WriteLine($"DEBUG: Unknown command type: {commandType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to process command '{command}': {ex.Message}");
        }
    }

    /// <summary>
    /// Publish robot state information (robot status, operating state)
    /// Called periodically to keep dashboard updated
    /// </summary>
    public async Task PublishRobotState(StateManager stateManager)
    {
        if (statusPublishTimer == null || !statusPublishTimer.Check())
            return;

        try
        {
            var stateData = new
            {
                state = stateManager.GetMqttString(),
                displayState = stateManager.GetDisplayString(),
                message = stateManager.StateMessage,
                stateDurationMs = (int)stateManager.CurrentStateDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(stateData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicState);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish robot state: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish obstacle detection data
    /// </summary>
    public async Task PublishObstacleData(ObstacleDetectionSystem obstacleSystem)
    {
        if (dataPublishTimer == null || !dataPublishTimer.Check())
            return;

        try
        {
            var obstacleData = new
            {
                minDistance = obstacleSystem.ObstacleDistance,
                directional = obstacleSystem.GetDirectionalDistancesJson(),
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(obstacleData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicObstacles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish obstacle data: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish line detection data
    /// </summary>
    public async Task PublishLineData(LineSystem lineSystem)
    {
        if (dataPublishTimer == null || !dataPublishTimer.Check())
            return;

        try
        {
            var lineData = new
            {
                sensors = lineSystem.GetSensorStatesJson(),
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(lineData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicLineDetection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish line detection data: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish color sensor data
    /// </summary>
    public async Task PublishColorData(string color)
    {
        if (dataPublishTimer == null || !dataPublishTimer.Check())
            return;

        try
        {
            var colorData = new
            {
                color = color,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(colorData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicColor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish color data: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish temperature measurement
    /// </summary>
    public async Task PublishTemperature(string temperature)
    {
        try
        {
            var tempData = new
            {
                value = temperature,
                unit = "°C",
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(tempData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicTemperature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish temperature data: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish humidity measurement
    /// </summary>
    public async Task PublishHumidity(string humidity)
    {
        try
        {
            var humData = new
            {
                value = humidity,
                unit = "%",
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(humData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicHumidity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish humidity data: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish alert state
    /// </summary>
    public async Task PublishAlertState(bool isActive, string message = "")
    {
        try
        {
            var alertData = new
            {
                active = isActive,
                message = message,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(alertData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicAlert);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish alert state: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish a custom message
    /// </summary>
    public async Task PublishMessage(string message)
    {
        try
        {
            var msgData = new
            {
                message = message,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var json = JsonSerializer.Serialize(msgData);
            if (mqttClient != null)
                await mqttClient.PublishMessage(json, topicMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to publish message: {ex.Message}");
        }
    }
}

/// <summary>
/// Represents a command received from MQTT
/// </summary>
public class MqttCommand
{
    public string Type { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Message { get; set; } = string.Empty;
}