using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace NanoMq.Publisher;

public static class NanoMqClient
{
    public static async Task Publish(string url, string topic, string payload)
    {
        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId("NanoMqPublisher")
            .WithWillResponseTopic("topic1/response")
            .WithTcpServer(url)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithResponseTopic("topic1/response")
            .WithCorrelationData(Encoding.UTF8.GetBytes("CorrelationData"))
            .WithPayload(payload)
            .Build();

        var result = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        // await mqttClient.DisconnectAsync();

        Console.WriteLine("MQTT application message is published.");
    }

    public static async Task SubscribeTopic(string url, string topic)
    {
        var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();
        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            Console.WriteLine("Received application message.");
            e.ReasonCode = MqttApplicationMessageReceivedReasonCode.Success;
            e.ResponseReasonString = "That worked!";
            Encoding.UTF8.GetString(e.ApplicationMessage.Payload).DumpToConsole();
            Encoding.UTF8.GetString(e.ApplicationMessage.CorrelationData).DumpToConsole();
            return Task.CompletedTask;
        };

        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(url).WithClientId("NanoMqPublisher").Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(topic);
                })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");
    }

    public static async Task SubscribeResponseTopic(string url)
    {
        var mqttFactory = new MqttFactory();
        using var mqttClient = mqttFactory.CreateMqttClient();

        mqttClient.ApplicationMessageReceivedAsync += args =>
        {
            args.DumpToConsole();
            return Task.CompletedTask;
        };
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(url).WithClientId("NanoMqPublisher").Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic("topic1/response").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce);
                })
            .Build();

        var result = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        Console.WriteLine("MQTT client subscribed to response.");
    }
}