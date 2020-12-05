using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sample.Core;
using Sample.Core.Messages.v1;

namespace Sample.Simulators.AzureFunction
{
    public class ShipmentGeneratorFunction : GeneratorFunction
    {
        [FunctionName("shipment-generator")]
        public async Task Run([TimerTrigger("*/15 * * * * *")]TimerInfo timerInfo, ILogger logger,
            [ServiceBus("%TopicName%", Connection = "ServiceBusConnectionString")] IAsyncCollector<Message> outputMessages)
        {
            logger.LogInformation($"Generating new batch of shipments at: {DateTime.UtcNow}");

            for (int messageCount = 1; messageCount <= 5; messageCount++)
            {
                var shipment = GenerateShipment();
                var message = WrapInServiceBusMessage(MessageType.ShipmentV1, shipment);

                logger.LogInformation($"Queuing shipment {shipment.Id} to {shipment.Location.Address} in {shipment.Location.City} for order #{shipment.Order.Id}");

                await outputMessages.AddAsync(message);
            }
        }

        private static ShipmentV1 GenerateShipment()
        {
            var locationInfo = new Faker<LocationV1>()
                .RuleFor(u => u.Address, (f, u) => f.Address.StreetAddress())
                .RuleFor(u => u.City, (f, u) => f.Address.City());
            var customerInfo = new Faker<CustomerV1>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

            var orderInfo = new Faker<OrderV1>()
                .RuleFor(u => u.Customer, () => customerInfo)
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Amount, f => f.Random.Int())
                .RuleFor(u => u.ArticleNumber, f => f.Commerce.Product())
                .Generate();
            var shipmentInfo = new Faker<ShipmentV1>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Order, f => orderInfo)
                .RuleFor(u => u.Location, f => locationInfo)
                .Generate();

            return shipmentInfo;
        }
    }
}
