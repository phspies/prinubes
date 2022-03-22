using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prinubes.Common.Kafka.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prinubes.Common.Kafka
{
    public class KafkaWorker : BackgroundService
    {
        private readonly IKafkaMessageConsumerManager kafkaMessageConsumerManager;
        private readonly ILogger<KafkaWorker> logger;

        public KafkaWorker(ILogger<KafkaWorker> _logger, IKafkaMessageConsumerManager _kafkaMessageConsumerManager)
        {
            logger = _logger;
            kafkaMessageConsumerManager = _kafkaMessageConsumerManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            kafkaMessageConsumerManager.StartConsumers(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
