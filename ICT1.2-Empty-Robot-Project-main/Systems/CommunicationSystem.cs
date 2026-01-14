
using Avans.StatisticalRobot.Interfaces;
using HiveMQtt.MQTT5.Types;
using NLog.Layouts;
using SimpleMqtt;

public class CommunicationSystem
{
    string baseTopic = "avansict";
    private SimpleMqttClient mqttClient;
    private WheeledRobot robot;
    private string name;
    private readonly string topicCommand,topicAlert,topicDistance,topicHum,topicTemp;
    private string clientId = $"{Environment.MachineName}-mqtt-client";
    public CommunicationSystem(WheeledRobot robot,string name)
    {
        this.robot = robot;
        this.name = name;
        this.topicCommand = $"{baseTopic}/{name}/command";
        this.topicAlert = $"avansict/{name}/alert";
        this.topicDistance = $"avansict/{name}/distance";
        this.topicTemp = $"{baseTopic}/{name}/temp";
        this.topicHum = $"avansict/{name}/hum";
        mqttClient = SimpleMqttClient.CreateSimpleMqttClientForHiveMQ(clientId);
        mqttClient.OnMessageReceived += MessageCallback;
    }
    public async Task Init()
    {
        await mqttClient.SubscribeToTopic(topicCommand);
    }
    private void MessageCallback(object? sender, SimpleMqttMessage msg)
    {
        Console.WriteLine($"DEBUG: MQTT message received: topic={msg.Topic}, msg={msg.Message}");
        robot.HandleMessage(msg);
    }
    public async Task SendAlertState(string state)
    {
        // Console.WriteLine($"DEBUG: Publishing alert state: topic={topicAlert}, msg={state}");
        await mqttClient.PublishMessage(state, topicAlert);
    }
    public async Task SendDistanceMeasurement(int distance)
    {
        // Console.WriteLine($"DEBUG: Publishing distance measurement: topic={topicDistance}, msg={distance.ToString()}");
        await mqttClient.PublishMessage(distance.ToString(), topicDistance);
    }

    internal async Task SendHumMeasurment(string humMeasurment)
    {
        await mqttClient.PublishMessage(humMeasurment.ToString(), topicHum);
    }

    internal async Task SendTempMeasurment(string tempMeasurment)
    {
        await mqttClient.PublishMessage(tempMeasurment.ToString(), topicTemp);
    }
}