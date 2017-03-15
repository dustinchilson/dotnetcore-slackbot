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
using Slackbot.Utils;

namespace Slackbot
{
    public class Bot
    {
        private readonly ILoggerFactory _loggerFactory;
        private SocketConnection _socketConnection;
        private readonly ILogger<Bot> _logger;
        private readonly Slack _slack;

        public event EventHandler<IncomingMessage> OnMessage;

        public Bot(string token, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Bot>();
            _slack = new Slack(token, loggerFactory.CreateLogger<Slack>());
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
            _socketConnection = new SocketConnection(url, _loggerFactory.CreateLogger<SocketConnection>());

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

            _logger?.LogDebug($"Handling message of type {message.Type}");

            if (message.Type == "message" && message.Subtype != "channel_join")
            {
                var incomingMessage = await jobj.ToObject<IncomingMessage>().FindMentionedUsers(_slack, data);
                incomingMessage.RawJson = data;
                OnMessage?.Invoke(this, incomingMessage);
            }

            if (message.Type == "error" || (string.IsNullOrEmpty(message.Type) && !jobj.SelectToken("$.ok").Value<bool>()))
                _logger?.LogError($"Err: {data}");
        }
    }
}