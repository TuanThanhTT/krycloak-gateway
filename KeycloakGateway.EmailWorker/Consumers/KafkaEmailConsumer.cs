//using Confluent.Kafka;
//using KeycloakGateway.Application.DTOs.Email;
//using KeycloakGateway.EmailWorker.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace KeycloakGateway.EmailWorker.Consumers
//{
//    public class KafkaEmailConsumer : BackgroundService
//    {
//        private readonly IConfiguration _config;
//        private readonly EmailSender _sender;
//        private readonly ILogger<KafkaEmailConsumer> _logger;

//        public KafkaEmailConsumer(
//            IConfiguration config,
//            EmailSender sender,
//            ILogger<KafkaEmailConsumer> logger)
//        {
//            _config = config;
//            _sender = sender;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            var consumerConfig = new ConsumerConfig
//            {
//                BootstrapServers = _config["Kafka:BootstrapServers"],
//                GroupId = _config["Kafka:GroupId"],
//                AutoOffsetReset = AutoOffsetReset.Earliest,
//                EnableAutoCommit = false
//            };

//            using var consumer =
//                new ConsumerBuilder<string, string>(consumerConfig).Build();

//            consumer.Subscribe(_config["Kafka:Topic"]);

//            _logger.LogInformation("Kafka Email Consumer started.");

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    var result = consumer.Consume(stoppingToken);

//                    if (result?.Message?.Value == null)
//                        continue;

//                    EmailMessage? message = null;

//                    try
//                    {
//                        message = JsonSerializer.Deserialize<EmailMessage>(
//                            result.Message.Value,
//                            new JsonSerializerOptions
//                            {
//                                PropertyNameCaseInsensitive = true
//                            });
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Invalid JSON format.");
//                        continue; // không commit
//                    }

//                    if (message == null)
//                        continue;

//                    var success = await _sender.SendAsync(message);

//                    if (success)
//                    {
//                        consumer.Commit(result);
//                    }
//                    else
//                    {
//                        _logger.LogWarning("Email sending failed. Offset not committed.");
//                    }
//                }
//                catch (OperationCanceledException)
//                {
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Kafka processing error.");
//                }
//            }

//            consumer.Close();
//        }
//    }
//}





//using Confluent.Kafka;

//public class KafkaEmailConsumer : BackgroundService
//{
//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        var config = new ConsumerConfig
//        {
//            BootstrapServers = "localhost:9092",
//            GroupId = "email-worker-group-NEW",
//            AutoOffsetReset = AutoOffsetReset.Earliest,
//            EnableAutoCommit = true
//        };

//        using var consumer = new ConsumerBuilder<Ignore, string>(config)
//            .SetErrorHandler((_, e) =>
//            {
//                Console.WriteLine($"❌ Kafka Error: {e.Reason}");
//            })
//            .Build();

//        consumer.Subscribe("email-topic");

//        Console.WriteLine("🔥 Kafka Worker Started");
//        Console.WriteLine("📡 Listening on topic: email-topic");
//        Console.WriteLine("-----------------------------------");

//        try
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                var result = consumer.Consume(stoppingToken);

//                if (result?.Message != null)
//                {
//                    Console.WriteLine($"✅ {result.Message.Value}");
//                }
//            }
//        }
//        catch (OperationCanceledException)
//        {
//            Console.WriteLine("🛑 Worker stopping...");
//        }
//        finally
//        {
//            consumer.Close();
//        }
//    }
//}



using Confluent.Kafka;
using KeycloakGateway.Application.DTOs.Email;
using KeycloakGateway.EmailWorker.Services;
using System.Text.Json;

namespace EmailWorker.Consumers;

public class KafkaEmailConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly EmailSender _sender;
    private readonly ILogger<KafkaEmailConsumer> _logger;

    public KafkaEmailConsumer(
        IConfiguration config,
        EmailSender sender,
        ILogger<KafkaEmailConsumer> logger)
    {
        _config = config;
        _sender = sender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = _config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer =
            new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        consumer.Subscribe(_config["Kafka:Topic"]);

        _logger.LogInformation("Kafka Email Consumer started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                if (result?.Message?.Value == null)
                    continue;

                EmailMessage? message = null;

                try
                {
                    message = JsonSerializer.Deserialize<EmailMessage>(
                        result.Message.Value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid JSON format.");
                    continue;
                }

                if (message == null)
                    continue;

                // 🔁 Retry 3 lần
                var retry = 0;
                var maxRetry = 3;
                var success = false;

                while (retry < maxRetry && !success)
                {
                    success = await _sender.SendAsync(message);
                    retry++;

                    if (!success)
                        await Task.Delay(2000, stoppingToken);
                }

                if (success)
                {
                    consumer.Commit(result);
                    _logger.LogInformation(
                        "Message committed. Offset: {Offset}",
                        result.Offset);
                }
                else
                {
                    _logger.LogWarning(
                        "Email failed after 3 retries. Offset NOT committed.");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka processing error.");
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka Email Consumer stopped.");
    }
}

