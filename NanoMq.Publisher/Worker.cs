using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace NanoMq.Publisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttClient _client;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _client = new MqttFactory().CreateMqttClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {

            Console.WriteLine("Starting Requester MQTT Client...");

            // Configure options
            var options = new MqttClientOptionsBuilder()
                .WithClientId("NanoMqPublisher")
                .WithTcpServer("0.0.0.0", 1883) // Port is optional
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .Build();

            // Handlers
            _client.ConnectedAsync += e =>
            {
                Console.WriteLine("Connected successfully with NanoMq.");
                Console.WriteLine();

                // Subscribe to a topic
                return _client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("topic1/response")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce).Build(), cancellationToken: stoppingToken);
            };

            _client.DisconnectedAsync += e =>
            {
                Console.WriteLine("Disconnected requester.");
                return Task.CompletedTask;
            };

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("### RECEIVED RESPONSE MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");

                await _client.DisconnectAsync(cancellationToken: stoppingToken);
            };

            // Actually connect
            await _client.ConnectAsync(options, stoppingToken);

            // Publish a message with a response topic
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("topic1")
                .WithCorrelationData(Encoding.UTF8.GetBytes("CorrelationData"))
                .WithPayload("This is a request")
                .WithResponseTopic("topic1/response")
                .Build();

            await _client.PublishAsync(message, stoppingToken);

            Console.ReadLine();
            await _client.DisconnectAsync(cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}