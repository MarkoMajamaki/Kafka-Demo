using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Shared;

namespace Consumer;

public class HellWorldMessageConsumer : IHostedService
{
    private readonly string topic = "helloworld";
    private readonly string groupId = "test_group";
    private readonly KafkaOptions _kafkaOptions;

    public HellWorldMessageConsumer(IOptions<KafkaOptions> kafkaOptions)
    {
        _kafkaOptions = kafkaOptions.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig {
            GroupId = groupId,
            BootstrapServers = _kafkaOptions.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        try 
        {
            using(var consumerBuilder = new ConsumerBuilder<Ignore, string> (config).Build()) 
            {
                // Subscribe to topic
                consumerBuilder.Subscribe(topic);
                var cancelToken = new CancellationTokenSource();

                try
                {
                    while (true) 
                    {
                        var consumer = consumerBuilder.Consume(cancelToken.Token);
                        var messageReceived = JsonSerializer.Deserialize<HelloWorldMessage>(consumer.Message.Value);
                        Debug.WriteLine($"Processing message: {consumer.Message.Value}");
                    }
                } 
                catch (OperationCanceledException) 
                {
                    consumerBuilder.Close();
                }
            }
        } 
        catch (Exception ex) 
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        return Task.CompletedTask;    
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine("HellWorldMessageConsumer StopAsync");

        return Task.CompletedTask;
    }
}