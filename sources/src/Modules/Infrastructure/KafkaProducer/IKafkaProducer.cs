using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.KafkaProducer
{
    public interface IKafkaProducer
    {
        Task PublishMessageAsync(string message);
    }
}
