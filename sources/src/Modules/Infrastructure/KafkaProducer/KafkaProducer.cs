using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.KafkaProducer
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaProducerConfig _kafkaProducerConfig;

        public KafkaProducer(IOptions<KafkaProducerConfig> kafkaProducerConfig)
        {
            _kafkaProducerConfig = kafkaProducerConfig.Value;
        }

        public async Task PublishMessageAsync(string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaProducerConfig.BootstrapServers,
                SaslMechanism = SaslMechanism.ScramSha256,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslUsername = _kafkaProducerConfig.UserName,
                SaslPassword = _kafkaProducerConfig.Password,
            };

            using var producer = new ProducerBuilder<Null, string>(config).Build();
            await producer.ProduceAsync(_kafkaProducerConfig.TopicName, new Message<Null, string> { Value = message });
        }
    }
}
