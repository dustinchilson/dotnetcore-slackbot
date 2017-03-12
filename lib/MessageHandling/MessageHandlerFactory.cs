using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Slackbot.MessageHandling
{
    internal class MessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IMessageHandler> CreateMessageHandlers()
        {
            return _serviceProvider.GetServices<IMessageHandler>();
        }
    }
}
