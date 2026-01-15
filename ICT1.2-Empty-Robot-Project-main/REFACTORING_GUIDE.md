# Robot ICT 1.2 - Refactored Architecture

## Overview

This document describes the refactored wheeled robot project with a focus on **Object-Oriented Programming (OOP)** principles and clean architecture. The refactoring introduces a centralized configuration system that makes the robot modular, flexible, and easy to extend.

## Key Improvements

### 1. **Centralized Configuration System**

#### `RobotConfiguration.cs`
- Central hub for all robot settings and sensor definitions
- Contains hardware pins, I2C addresses, motor parameters, and communication settings
- **No hardcoded values in systems** - all configuration is externalized
- Includes a `CreateDefaultConfiguration()` factory method for quick setup

**Benefits:**
- Change robot hardware without modifying system code
- Support multiple robot variants easily
- Test with different configurations

**Example:**
```csharp
var config = RobotConfiguration.CreateDefaultConfiguration();
// Or customize:
var customConfig = new RobotConfiguration { RobotName = "CustomBot" };
customConfig.AddSensor(new SensorConfiguration("my_sensor", SensorType.Ultrasonic2Pin, 5));
```

### 2. **Flexible Sensor Configuration**

#### `SensorConfiguration.cs`
- Describes individual sensors with all their properties
- Supports pin numbers, directions, I2C addresses, and custom settings
- Uses **fluent builder pattern** for easy configuration

**Features:**
- Pin and secondary pin support (for 2-pin ultrasonic sensors)
- Direction enum for spatial sensors (Forward, Reverse, Left, Right, ForwardLeft, etc.)
- I2C address for I2C devices
- Custom settings dictionary for sensor-specific configurations
- Enable/disable individual sensors without constructor changes

**Example:**
```csharp
var sensor = new SensorConfiguration("ultrasonic_forward", SensorType.Ultrasonic2Pin, 5)
    .WithSecondaryPin(6)
    .WithDirection(SensorDirection.Forward);

var colorSensor = new SensorConfiguration("rgb", SensorType.RGBColor, 0)
    .WithI2CAddress(0x29);
```

### 3. **Robot State Management**

#### `RobotState.cs` / `StateManager`
- Tracks the robot's operational state throughout its lifecycle
- States: Initializing, Ready, Operating, Stopped, EmergencyStopped, Fault, Offline
- **Event-driven** state changes notify all systems
- Provides MQTT-friendly state strings and display messages

**Features:**
- State change events for reactive programming
- Duration tracking for telemetry
- Human-readable and MQTT-formatted state strings

**Example:**
```csharp
stateManager.SetState(RobotStateEnum.Operating, "Moving forward");
// Event fires: StateChanged += (sender, args) => { ... }
```

### 4. **Refactored Systems (All Configuration-Based)**

#### **ObstacleDetectionSystem**
- Dynamically initializes **all ultrasonic sensors** from configuration
- Tracks distances by **direction** (8-directional obstacle awareness)
- Publishes structured JSON data to MQTT
- Zero hardcoded pins - fully configurable

```csharp
// All sensors initialized from config
var obstacles = new ObstacleDetectionSystem(config);
// Access specific direction distances
int forwardDist = obstacles.GetDistanceInDirection(SensorDirection.Forward);
// Get all directional data as JSON
string json = obstacles.GetDirectionalDistancesJson();
```

#### **LineSystem**
- Dynamically initializes **IR line sensors** from configuration
- Maintains sensor states by ID and direction
- Provides callback interface for line-following algorithms
- Exports sensor states as JSON for MQTT

```csharp
lineSystem.GetSensorStateByDirection(SensorDirection.Forward)
lineSystem.GetSensorStatesJson() // {"forward":"detected","left":"clear",...}
```

#### **DriveSystem**
- Uses `DriveConfiguration` for all motor parameters
- Eliminates magic numbers
- Supports external speed control via `targetSpeed` property

#### **ColorSystem**
- Initializes RGB sensor from configuration
- I2C address fully configurable
- Graceful fallback if sensor unavailable

#### **DataSystem**
- Publishes temperature and humidity to MQTT
- Interval-based publishing (configurable)
- Error handling for sensor failures

### 5. **Enhanced AlertSystem**

Now **handles both internal and MQTT control**:

**Features:**
- Manual button control (emergency stop)
- MQTT remote control of alerts and emergency stop
- Separate methods for different alert types
- Event `EmergencyStopChanged` for external listeners
- Display-only messages vs. full alerts with LED

**Example:**
```csharp
// From manual button or MQTT
alertSystem.SetEmergencyStop(true, "Obstacle too close");

// Display custom message (no LED)
alertSystem.DisplayMessage("Processing...");

// Full alert (LED + display + event)
alertSystem.AlertOn("ALERT!");
```

### 6. **Central Communication Hub**

#### **CommunicationSystem** - The Data Export Center

This is now the **single point** for all MQTT communication:

**Published Topics:**
- `avansict/{robot}/state` - Robot operational state
- `avansict/{robot}/obstacles` - Directional obstacle distances (JSON)
- `avansict/{robot}/line` - Line sensor states (JSON)
- `avansict/{robot}/color` - Current detected color
- `avansict/{robot}/temperature` - Temperature reading
- `avansict/{robot}/humidity` - Humidity reading
- `avansict/{robot}/alert` - Alert status and message
- `avansict/{robot}/message` - Custom messages

**Subscribed Topics:**
- `avansict/{robot}/command` - Remote commands

**Command Format:**
```
drive:forward:0.8      // Drive forward at 80% speed
drive:backward:0.5     // Drive backward at 50% speed
emergency:stop         // Emergency stop
emergency:resume       // Resume after emergency stop
alert:Custom message   // Display custom message
message:Status update  // Custom message
```

**Features:**
- **Bandwidth-conscious**: Sends processed data, not raw sensor values
- Timestamps on all messages (ISO 8601 format)
- Periodic publishing with configurable intervals
- JSON-formatted structured data for easy dashboard parsing
- Event-driven for remote commands

**Example:**
```csharp
await communicationSystem.PublishRobotState(stateManager);
await communicationSystem.PublishObstacleData(obstacleSystem);
await communicationSystem.PublishColorData(colorSystem.color);

// Subscribe to remote commands
communicationSystem.CommandReceived += (sender, cmd) => { ... };
communicationSystem.EmergencyStopCommandReceived += (sender, state) => { ... };
```

### 7. **WheeledRobot - Orchestration**

The main `WheeledRobot` class now:
- Uses configuration to initialize all systems
- Wires up event handlers between systems
- Manages state transitions
- Implements clean separation of concerns
- Handles MQTT commands and events

**Key Methods:**
- `Init()` - Async initialization (network setup)
- `Update()` - Main loop, coordinates all systems
- `lineCallback()` - Line detection callback
- `HandleDriveCommand()` - Process MQTT drive commands

**Event Handling:**
```csharp
alertSystem.EmergencyStopChanged += OnEmergencyStopChanged;
communicationSystem.CommandReceived += OnCommandReceived;
communicationSystem.EmergencyStopCommandReceived += OnEmergencyStopCommand;
communicationSystem.AlertCommandReceived += OnAlertCommand;
```

## Architecture Diagram

```
┌─────────────────────────────────────────┐
│     RobotConfiguration                  │
│  (Central Hub - All Settings)           │
└────────────────┬────────────────────────┘
                 │
        ┌────────┴────────────────────┬──────────┬──────────┐
        │                             │          │          │
        ▼                             ▼          ▼          ▼
  ┌──────────────┐          ┌──────────────┐ ┌────────┐ ┌──────────┐
  │WheeledRobot  │          │Sensors       │ │Motors  │ │Settings  │
  └──────┬───────┘          └──────────────┘ └────────┘ └──────────┘
         │
    ┌────┴───────┬──────────────┬──────────┬───────────────┐
    │            │              │          │               │
    ▼            ▼              ▼          ▼               ▼
┌─────────┐ ┌──────────┐ ┌────────┐ ┌──────────┐ ┌───────────────┐
│ Drive   │ │Obstacle  │ │ Line   │ │ Alert    │ │Communication │
│ System  │ │Detection │ │System  │ │ System   │ │ System (MQTT) │
└─────────┘ └──────────┘ └────────┘ └──────────┘ └───────────────┘
    │            │              │          │               │
    └────────────┴──────────────┴──────────┴───────────────┘
                            │
                ┌───────────┴────────────┐
                │                        │
                ▼                        ▼
          ┌──────────┐          ┌────────────────┐
          │  Motors  │          │  MQTT Broker   │
          └──────────┘          └────────────────┘
                                 (Publish/Subscribe)
```

## Usage Example

```csharp
// 1. Create configuration
var config = RobotConfiguration.CreateDefaultConfiguration();

// 2. Customize if needed
config.AddSensor(new SensorConfiguration("extra_ultrasonic", SensorType.Ultrasonic2Pin, 27)
    .WithSecondaryPin(28)
    .WithDirection(SensorDirection.Right));

// 3. Create robot with config
var robot = new WheeledRobot();

// 4. Initialize (connects to MQTT, initializes hardware)
await robot.Init();

// 5. Main loop
while (true)
{
    robot.Update();
    Robot.Wait(1);
}
```

## Adding New Sensors

To add a new ultrasonic sensor facing a different direction:

```csharp
config.AddSensor(new SensorConfiguration("ultrasonic_custom", SensorType.Ultrasonic2Pin, 21)
    .WithSecondaryPin(22)
    .WithDirection(SensorDirection.ForwardRight));
```

The `ObstacleDetectionSystem` automatically picks it up and tracks it by direction. No constructor changes needed!

## Bandwidth Optimization

All MQTT messages are **processed, not raw**:
- Obstacle distances aggregated and published as directional JSON
- Color sensor publishes detected color name, not raw RGB values
- Temperature/humidity formatted with units and timestamps
- State messages include human-readable display strings

Example obstacle message:
```json
{
  "minDistance": 25,
  "directional": {
    "forward": 45,
    "left": 30,
    "right": 25,
    "reverse": 60
  },
  "timestamp": "2026-01-15T10:30:45.123Z"
}
```

## Testing & Debugging

Each system can be tested independently:
```csharp
var config = RobotConfiguration.CreateDefaultConfiguration();
var obstacleSystem = new ObstacleDetectionSystem(config);
obstacleSystem.Update();
Console.WriteLine(obstacleSystem.GetDirectionalDistancesJson());
```

## Future Enhancements

1. **Configuration from JSON/YAML** - Load robot config from files
2. **Sensor health monitoring** - Track sensor failures in StateManager
3. **Data logging** - Log all state changes and sensor readings
4. **Dashboard integration** - Use structured MQTT data for real-time dashboards
5. **Multi-robot support** - Manage multiple robots with different configs
6. **Calibration system** - Store and load sensor calibration values

## Summary

This refactoring transforms the robot code from a tightly-coupled, hardcoded design into a **flexible, configuration-driven, event-driven architecture** that:

✅ Follows **SOLID principles** (especially Dependency Inversion & Open/Closed)  
✅ **No constructor changes** when adding/removing sensors  
✅ **Central communication hub** for bandwidth-conscious MQTT  
✅ **State management** with event notifications  
✅ **Supports 8-directional obstacle detection** with configurable sensors  
✅ **Dual control** of alerts (local button + MQTT)  
✅ **Excellent for OOP teaching** - clear patterns and principles  
