//using Confluent.Kafka;
//using KeycloakGateway.Application.DTOs.Email;
//using KeycloakGateway.Application.Interfaces;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using static Confluent.Kafka.ConfigPropertyNames;

//namespace KeycloakGateway.Infrastructure.Kafka
//{
//    public class KafkaEmailProducer : IEmailProducer
//    {

//        private readonly IProducer<string, string> _producer;
//        private readonly string _topic;

//        public KafkaEmailProducer(IConfiguration config)
//        {
//            var producerConfig = new ProducerConfig
//            {
//                BootstrapServers = config["Kafka:BootstrapServers"]
//            };

//            _topic = config["Kafka:Topic"]
//                ?? throw new Exception("Kafka:Topic not configured");

//            _producer = new ProducerBuilder<string, string>(producerConfig)
//                .Build();
//        }

//        public async Task PublishAsync(EmailMessage message)
//        {
//            var json = JsonSerializer.Serialize(message);

//            await _producer.ProduceAsync(_topic, new Message<string, string>
//            {
//                Key = Guid.NewGuid().ToString(),
//                Value = json
//            });

//            Console.WriteLine($"Published email message to Kafka topic {_topic}");  
//        }

//    }
//}



using Confluent.Kafka;
using KeycloakGateway.Application.DTOs.Email;
using KeycloakGateway.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace KeycloakGateway.Infrastructure.Kafka
{
    public class KafkaEmailProducer : IEmailProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaEmailProducer(IConfiguration config)
        {
            Console.WriteLine("🔵 [KafkaProducer] Initializing...");

            var bootstrapServers = config["Kafka:BootstrapServers"];
            _topic = config["Kafka:Topic"];

            Console.WriteLine($"🔵 [KafkaProducer] BootstrapServers: {bootstrapServers}");
            Console.WriteLine($"🔵 [KafkaProducer] Topic: {_topic}");

            if (string.IsNullOrEmpty(bootstrapServers))
                throw new Exception("❌ Kafka:BootstrapServers not configured");

            if (string.IsNullOrEmpty(_topic))
                throw new Exception("❌ Kafka:Topic not configured");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) =>
                {
                    Console.WriteLine($"❌ [Kafka ERROR] {e.Reason}");
                })
                .SetLogHandler((_, log) =>
                {
                    Console.WriteLine($"📘 [Kafka LOG] {log.Message}");
                })
                .Build();

            Console.WriteLine("🟢 [KafkaProducer] Initialized successfully");
        }

        public async Task PublishAsync(EmailMessage message)
        {
            Console.WriteLine("🚀 [KafkaProducer] Preparing to publish message...");

            try
            {
                var json = JsonSerializer.Serialize(message);

                Console.WriteLine($"📦 [KafkaProducer] Payload: {json}");
                Console.WriteLine($"📌 [KafkaProducer] Publishing to topic: {_topic}");

                var result = await _producer.ProduceAsync(_topic, new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = json
                });

                Console.WriteLine("✅ [KafkaProducer] Message delivered!");
                Console.WriteLine($"   Topic: {result.Topic}");
                Console.WriteLine($"   Partition: {result.Partition}");
                Console.WriteLine($"   Offset: {result.Offset}");
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine("❌ [KafkaProducer] ProduceException occurred!");
                Console.WriteLine($"   Error: {ex.Error.Reason}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ [KafkaProducer] Unexpected error occurred!");
                Console.WriteLine($"   Message: {ex.Message}");
            }
        }
    }
}