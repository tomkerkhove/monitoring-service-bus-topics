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
    public class OrderGeneratorFunction : GeneratorFunction
    {
        [FunctionName("order-generator")]
        public async Task Run([TimerTrigger("*/30 * * * * *")]TimerInfo timerInfo, ILogger logger,
            [ServiceBus("%TopicName%", Connection = "ServiceBusConnectionString")] IAsyncCollector<Message> outputMessages)
        {
            logger.LogInformation($"Generating new batch of orders at: {DateTime.UtcNow}");

            for (int messageCount = 1; messageCount <= 25; messageCount++)
            {
                var order = GenerateOrder();
                var message = WrapInServiceBusMessage(MessageType.OrdersV1, order);

                logger.LogInformation($"Queuing order {order.Id} - A {order.ArticleNumber} for {order.Customer.FirstName} {order.Customer.LastName}");


                await outputMessages.AddAsync(message);
            }
        }

        private static OrderV1 GenerateOrder()
        {
            var customerGenerator = new Faker<CustomerV1>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

            var orderGenerator = new Faker<OrderV1>()
                .RuleFor(u => u.Customer, () => customerGenerator)
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Amount, f => f.Random.Int())
                .RuleFor(u => u.ArticleNumber, f => f.Commerce.Product());

            return orderGenerator.Generate();
        }
    }
}
