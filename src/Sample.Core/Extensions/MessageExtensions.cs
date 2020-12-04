using System;
using Microsoft.Azure.ServiceBus;

namespace Sample.Core.Extensions
{
    public static class MessageExtensions
    {
        public static MessageType GetMessageType(this Message message)
        {
            if (message.UserProperties.TryGetValue(MessageHeaders.MessageType, out var rawMessageType))
            {
                if (Enum.TryParse<MessageType>(rawMessageType.ToString(), out var messageType))
                {
                    return messageType;
                }
            }

            return MessageType.Unknown;
        }
    }
}
