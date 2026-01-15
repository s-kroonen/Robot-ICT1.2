using RobotApp.Data;
using RobotApp.Models.Measurements;
using RobotApp.Repositories;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RobotApp.Services.Mqtt;

public class MqttService : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private readonly RobotStateService _stateService;
    private readonly SimpleMqttClient _client;
    private readonly IServiceScopeFactory _scopeFactory;


    private readonly string _baseTopic;

    public MqttService(
        IConfiguration config,
        RobotStateService stateService,
        IServiceScopeFactory scopeFactory)
    {
        _config = config;
        _stateService = stateService;
        _scopeFactory = scopeFactory;

        _baseTopic = _config["Mqtt:BaseTopic"]!;

        var clientId = $"dashboard-{Environment.MachineName}-{Guid.NewGuid()}";

        _client = SimpleMqttClient.CreateSimpleMqttClientForHiveMQ(clientId);
        _client.OnMessageReceived += HandleMessage;
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Subscribing to: {0}", _baseTopic);
        await _client.SubscribeToTopic($"{_baseTopic}/#");
    }

    private void HandleMessage(object? sender, SimpleMqttMessage msg)
    {
        if (msg.Topic is null || msg.Message is null)
            return;

        ParsedTopic parsed;

        try
        {
            parsed = MqttTopicParser.Parse(msg.Topic, _baseTopic);
        }
        catch
        {
            return;
        }

        switch (parsed.Metric)
        {
            case "temperature":
                HandleTemperature(parsed.Robot, msg.Message);
                break;

            case "humidity":
                HandleHumidity(parsed.Robot, msg.Message);
                break;

            case "state":
                HandleState(parsed.Robot, msg.Message);
                break;

            case "alert":
                HandleAlert(parsed.Robot, msg.Message);
                break;
        }
    }

    private async Task HandleTemperature(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<TemperatureDto>(json)!;
        if (dto == null) return;

        using var scope = _scopeFactory.CreateScope();
        var robots = scope.ServiceProvider.GetRequiredService<IRobotRepository>();
        var measurements = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();

        await robots.GetOrCreateAsync(robot);

        await measurements.SaveTemperatureAsync(new TemperatureMeasurement
        {
            RobotName = robot,
            Value = double.Parse(dto.value),
            Timestamp = dto.timestamp
        });

        _stateService.Update(robot, s =>
        {
            s.Temperature = double.Parse(dto.value);
        });
    }

    private async Task HandleHumidity(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<HumidityDto>(json)!;
        if (dto == null) return;

        using var scope = _scopeFactory.CreateScope();
        var robots = scope.ServiceProvider.GetRequiredService<IRobotRepository>();
        var measurements = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();

        await robots.GetOrCreateAsync(robot);

        await measurements.SaveHumidityAsync(new HumidityMeasurement
        {
            RobotName = robot,
            Value = double.Parse(dto.value),
            Timestamp = dto.timestamp
        });
        _stateService.Update(robot, s =>
        {
            s.Humidity = double.Parse(dto.value);
        });
    }

    private async Task HandleState(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<StateDto>(json)!;
        if (dto == null) return;

        using var scope = _scopeFactory.CreateScope();
        var robots = scope.ServiceProvider.GetRequiredService<IRobotRepository>();
        var measurements = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();

        await robots.GetOrCreateAsync(robot);

        await measurements.SaveStateAsync(new StateSnapshot
        {
            RobotName = robot,
            State = dto.state,
            DisplayState = dto.displayState,
            Message = dto.message,
            Timestamp = dto.timestamp,
        });

        _stateService.Update(robot, s =>
        {
            s.State = dto.state;
            s.DisplayState = dto.displayState;
            s.Message = dto.message;
        });
    }

    private async Task HandleAlert(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<AlertDto>(json)!;

        _stateService.Update(robot, s =>
        {
            s.AlertActive = dto.active;
            s.AlertMessage = dto.message;
        });
    }

    // ---------------- COMMANDS ----------------

    public Task SendCommand(string robot, string command)
    {
        var topic = $"{_baseTopic}/{robot}/command";
        return _client.PublishMessage(command, topic);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await Task.CompletedTask;
    }
}
