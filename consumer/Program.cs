using Confluent.Kafka;
using consumer.Service;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

    Log.Information("Startin Consumer application");

    var services = new ServiceCollection();

    var settings = new ElasticsearchClientSettings(
        new Uri("http://elasticsearch:9200")
    )
    .DefaultIndex("report-index");

var elasticClient = new ElasticsearchClient(settings);
    services.AddSingleton(elasticClient);

  
    services.AddTransient<ProcessingService>();
    var serviceProvider = services.BuildServiceProvider();

    var bootstrapServers =
        Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
        ?? "localhost:9092";

    var topic =
        Environment.GetEnvironmentVariable("KAFKA_TOPIC")
        ?? "topic";

    var groupId =
        Environment.GetEnvironmentVariable("KAFKA_GROUP_ID")
        ?? "csharp-consumer";

    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = bootstrapServers,
        GroupId = groupId,
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    Log.Information("Kafka consumer created");

    consumer.Subscribe(topic);
    Log.Information($"Subscribed to topic: {topic}, group: {groupId}");

while (true)
    {
        try
        {
            Log.Information("Waiting for Kafka message...");
            var result = consumer.Consume(TimeSpan.FromSeconds(10));
            if (result == null || result.Message.Value == null)
            {
                continue;
            }
            Log.Information($"[Received Message]: {result.Message.Value}");
            using var scope = serviceProvider.CreateScope();
            var processingService = scope.ServiceProvider.GetRequiredService<ProcessingService>();
            var isSuccess = await processingService.ProcessEventAsync(result.Message.Value);

            if (isSuccess)
            {
                consumer.Commit(result); 
            }
            else
            {
                Log.Warning("Message processing failed, skipping commit.");
            }
         }
        catch (Exception ex)
        {
            Log.Error($"Error occurred: {ex.Message}");
        }

    }


