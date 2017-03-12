using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nito.AsyncEx;
using Slackbot.Model;
using Slackbot.MessageHandling;
using Slackbot.Utils;

namespace Slackbot
{
    public class Bot
    {
        public readonly string Token;
        public readonly string Username;

        private SocketConnection _socketConnection;
        private readonly MessageHandlerFactory _factory;
        private readonly ILogger<Bot> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        private readonly Slack _slack;

        public event EventHandler<Message> OnMessage;

        public static Bot Factory(string token, string username, IServiceProvider serviceProvider)
        {
            return new Bot(token, username, serviceProvider);
        }

        private Bot(string token, string username, IServiceProvider serviceProvider)
        {
            Token = token;
            Username = username;
            _serviceProvider = serviceProvider;

            _slack = new Slack(Token, serviceProvider.GetService<ILogger<Slack>>());
            _logger = _serviceProvider.GetService<ILoggerFactory>().CreateLogger<Bot>();
            _factory = new MessageHandlerFactory(_serviceProvider);
        }

        public Bot Run()
        {
            AsyncContext.Run(async () => await Connect());
            return this;
        }

        public async Task SendMessageAsync(Message message)
        {
            var json = this.CreateMessage(message);
            var outboundBytes = Encoding.UTF8.GetBytes(json);
            var outboundBuffer = new ArraySegment<byte>(outboundBytes);

            _logger?.LogDebug($"Sending Message: {json}");

            await _socketConnection.SendDataAsync(outboundBuffer);
        }

        public async Task SendAdvancedMessageAsync(AdvancedMessage message)
        {
            await _slack.SendMessage(message);
        }

        private string CreateMessage<T>(T message)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver  = new LowercaseContractResolver()
            };
            
            return JsonConvert.SerializeObject(message, Formatting.None, jsonSettings);
        }

        private async Task Connect()
        {
            var url = await _slack.GetWebsocketUrl();
            _socketConnection = new SocketConnection(url);

            _socketConnection.OnData += async (sender, data) =>
            {
                try
                {
                    await HandleOnData(data);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            };
        }

        private async Task HandleOnData(string data)
        {
            var jobj = JObject.Parse(data);
            var message = jobj.ToObject<SlackData>();

            _logger?.LogTrace($"Handling message of type {message.Type}");

            if (message.Type == "message")
            {
                var incomingMessage = await jobj.ToObject<IncomingMessage>()
                                                .FindMentionedUsers(_slack, data);
                
                OnMessage?.Invoke(this, incomingMessage);

                if (_factory != null)
                {
                    var handlers = _factory.CreateMessageHandlers();
                    foreach (IMessageHandler handler in handlers)
                    {
                        if (await handler.ShouldHandleAsync(incomingMessage))
                            await handler.HandleMessageAsync(this, incomingMessage);
                    }
                }
            }

            if (message.Type == "error" || (string.IsNullOrEmpty(message.Type) && !jobj.SelectToken("$.ok").Value<bool>()))
                _logger?.LogError($"Err: {data}");
        }
    }
}