using RobotApp.Components.Layout.Classes;
public class MqttMessageProcessingService /*: IHostedService*/
{
    // private readonly IUserRepository _userRepository;
    private readonly SimpleMqttClient _mqttClient;

    public MqttMessageProcessingService(SimpleMqttClient mqttClient)
    {
      	// _userRepository = userRepository;  
      	_mqttClient = mqttClient;
        
        _mqttClient.OnMessageReceived += (sender, args) => {
            Console.WriteLine($"Incoming MQTT message on {args.Topic}:{args.Message}");
        };
    }

    // public Task StartAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }

    // public Task StopAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
}