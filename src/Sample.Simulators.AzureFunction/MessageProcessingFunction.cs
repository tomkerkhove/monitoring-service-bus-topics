using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sample.Core;
using Sample.Core.Extensions;
using Sample.Core.Messages.v1;

namespace Sample.Simulators.AzureFunction
{
    public static class MessageProcessingFunction
    {
        private static readonly Random _randomizer = new Random();

        [FunctionName("message-processor")]
        public static void Run([ServiceBusTrigger("%TopicName%", "%SubscriptionName%", Connection = "ServiceBusConnectionString")] Message message, ILogger log)
        {
            var messageType = message.GetMessageType();

            // Break stuff
            if (_randomizer.Next(1, 11) > 5)
            {
                throw new Exception($"Failed to process message of type {messageType}");
            }

            switch (messageType)
            {
                case MessageType.OrdersV1:
                    ProcessOrder(message, log);
                    break;
                case MessageType.ShipmentV1:
                    ProcessShipment(message, log);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ProcessShipment(Message message, ILogger log)
        {
            var shipment = ParseMessage<ShipmentV1>(message);
            log.LogInformation("Processing shipment {ShipmentId} for order #{OrderId}", shipment.Id, shipment.Order.Id);
        }

        private static void ProcessOrder(Message message, ILogger log)
        {
            var order = ParseMessage<OrderV1>(message);
            log.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);
        }

        private static TMessage ParseMessage<TMessage>(Message message)
        {
            var rawBody = Encoding.UTF8.GetString(message.Body);
            return JsonConvert.DeserializeObject<TMessage>(rawBody);
        }
    }
}
