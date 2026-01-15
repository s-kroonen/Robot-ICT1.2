namespace RobotApp.Services.Mqtt
{
    public record ParsedTopic(
    string Robot,
    string Metric
);

    public static class MqttTopicParser
    {
        public static ParsedTopic Parse(string topic, string baseTopic)
        {
            // avansict/{robot}/{metric}
            var parts = topic.Split('/');
            if (parts.Length < 3)
                throw new InvalidOperationException($"Invalid topic: {topic}");

            return new ParsedTopic(
                Robot: parts[1],
                Metric: parts[2]
            );
        }
    }

}
