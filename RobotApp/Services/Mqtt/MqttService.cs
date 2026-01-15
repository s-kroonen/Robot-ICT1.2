using System.Text.Json;

namespace RobotApp.Services.Mqtt;

public class MqttService : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private readonly RobotStateService _stateService;
    private readonly SimpleMqttClient _client;

    private readonly string _baseTopic;

    public MqttService(
        IConfiguration config,
        RobotStateService stateService)
    {
        _config = config;
        _stateService = stateService;

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

    private void HandleTemperature(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<TemperatureDto>(json)!;
        _stateService.Update(robot, s =>
        {
            s.Temperature = double.Parse(dto.value);
        });
    }

    private void HandleHumidity(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<HumidityDto>(json)!;

        _stateService.Update(robot, s =>
        {
            s.Humidity = double.Parse(dto.value);
        });
    }

    private void HandleState(string robot, string json)
    {
        var dto = JsonSerializer.Deserialize<StateDto>(json)!;

        _stateService.Update(robot, s =>
        {
            s.State = dto.state;
            s.DisplayState = dto.displayState;
            s.Message = dto.message;
        });
    }

    private void HandleAlert(string robot, string json)
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
