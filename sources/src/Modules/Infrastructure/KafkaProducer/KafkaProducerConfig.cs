using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.KafkaProducer
{
    public class KafkaProducerConfig
    {
        public string BootstrapServers { get; set; }

        public string TopicName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
