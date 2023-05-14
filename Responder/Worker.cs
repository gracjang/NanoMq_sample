using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Responder;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {

            Console.WriteLine("Starting response MQTT Client...");

            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();
            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("Received application message.");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ CorrelationData = {Encoding.UTF8.GetString(e.ApplicationMessage.CorrelationData)}"); ;

                e.ReasonCode = MqttApplicationMessageReceivedReasonCode.Success;
                e.ResponseReasonString = "That worked! Response from Responder.";

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("topic1/response")
                    .WithCorrelationData(Encoding.UTF8.GetBytes("CorrelationData"))
                    .WithPayload("This is a response.")
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                await mqttClient.DisconnectAsync(cancellationToken: stoppingToken);
            };

            mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine("Disconnected responder.");
                return Task.CompletedTask;
            };

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("0.0.0.0")
                .WithClientId("NanoMqPublisher")
                .WithProtocolVersion(MqttProtocolVersion.V500).Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter("topic1", MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            Console.ReadLine();
            Console.WriteLine("MQTT client subscribed to response topic.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}