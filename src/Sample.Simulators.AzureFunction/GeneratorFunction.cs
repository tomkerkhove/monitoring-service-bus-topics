using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Sample.Core;

namespace Sample.Simulators.AzureFunction
{
    public class GeneratorFunction
    {
        protected Message WrapInServiceBusMessage(MessageType messageType, object payload)
        {
            var rawOrder = JsonConvert.SerializeObject(payload);
            var messageBody = Encoding.UTF8.GetBytes(rawOrder);
            var message = new Message(messageBody);
            message.UserProperties.Add(MessageHeaders.MessageType, messageType.ToString());
            return message;
        }
    }
}
