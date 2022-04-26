using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using Shared;

namespace Producer.Api;

[ApiController]
[Route("[controller]")]
public class PublishMessagesController : ControllerBase
{    
    private readonly string topic = "helloworld";
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<PublishMessagesController> _logger;

    public PublishMessagesController(
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<PublishMessagesController> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task <IActionResult> Post([FromBody] HelloWorldMessage message) 
    {
        ProducerConfig config = new ProducerConfig {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            ClientId = Dns.GetHostName()
        };

        string messageFormatted = JsonSerializer.Serialize(message);

        using (var producer = new ProducerBuilder<string, string>(config.AsEnumerable()).Build())
        {
            var result = await producer.ProduceAsync(topic, new Message<string, string> {
                Value = messageFormatted
            });
        }

        _logger.LogInformation("Message produced!");

        return Ok();
    }
}