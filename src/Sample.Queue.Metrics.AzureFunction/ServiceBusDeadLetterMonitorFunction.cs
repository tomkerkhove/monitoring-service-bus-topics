using System;
using System.Collections.Generic;
using Arcus.Observability.Correlation;
using GuardNet;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Core.Extensions;

namespace Sample.Queue.Metrics.AzureFunction
{
    public class ServiceBusDeadLetterMonitorFunction
    {
        private readonly ILogger<ServiceBusDeadLetterMonitorFunction> _logger;
        private readonly ICorrelationInfoAccessor _correlationInfoAccessor;
        private readonly IConfiguration _configuration;

        public ServiceBusDeadLetterMonitorFunction(IConfiguration configuration, ICorrelationInfoAccessor correlationInfoAccessor, ILogger<ServiceBusDeadLetterMonitorFunction> logger)
        {
            Guard.NotNull(correlationInfoAccessor, nameof(correlationInfoAccessor));
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;
            _configuration = configuration;
            _correlationInfoAccessor = correlationInfoAccessor;
        }

        [FunctionName("azure-service-bus-deadletter-monitor")]
        public void Run([ServiceBusTrigger("%TopicName%", "%SubscriptionName%/$DeadLetterQueue", Connection = "ServiceBusConnectionString")] Message deadLetteredMessage,
                        [ServiceBus("%DeadletterEntityName%", Connection = "ServiceBusConnectionString")] out Message outputMessage)
        {
            SetCorrelationWhenSpecified(deadLetteredMessage);

            var messageType = deadLetteredMessage.GetMessageType();
            var topicName = _configuration["TopicName"];
            var subscriptionName = _configuration["SubscriptionName"];

            var contextualInformation = new Dictionary<string, object>
            {
                {"Message Type", messageType},
                {"Topic Name", topicName},
                {"Subscription Name", subscriptionName},
                {"Message ID", deadLetteredMessage.MessageId},
                {"Session ID", deadLetteredMessage.SessionId}
            };

            // Use enqueued time to ensure metric is reported when it was being deadlettered
            _logger.LogMetric("Deadlettered Messages", 1, deadLetteredMessage.SystemProperties.EnqueuedTimeUtc, contextualInformation);

            outputMessage = deadLetteredMessage.Clone();
        }

        private void SetCorrelationWhenSpecified(Message deadLetteredMessage)
        {
            if (string.IsNullOrWhiteSpace(deadLetteredMessage.CorrelationId) == false)
            {
                _correlationInfoAccessor.SetCorrelationInfo(new CorrelationInfo(deadLetteredMessage.CorrelationId, Guid.NewGuid().ToString()));
            }
        }
    }
}