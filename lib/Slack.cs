using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Slackbot.Model;
using Slackbot.Utils;

namespace Slackbot
{
    public class Slack
    {
        private readonly string _token;
        private readonly ILogger<Slack> _logger;

        public Slack(string token, ILogger<Slack> logger)
        {
            _token = token;
            _logger = logger;
        }

        public async Task<string> GetWebsocketUrl()
        {
            var uri = $"https://slack.com/api/rtm.start?token={_token}";

            using (var client = new HttpClient())
            {
                var responseContent = await client.GetAsync(uri).ContinueWith(s => HandleErrors<HelloRTMSession>(s.Result)).Result;
                return responseContent.Url;
            }
        }

        public async Task<string> GetUsername(string userId)
        {
            var uri = $"https://slack.com/api/users.list?token={_token}";

            using (var client = new HttpClient())
            {
                var responseContent = await client.GetAsync(uri).ContinueWith(s => HandleErrors<SlackUserList>(s.Result)).Result;
                return responseContent.Members.First(member => member.Id == userId).Name;
            }
        }

        public async Task<Message> SendMessage(AdvancedMessage message)
        {
            var json = JsonConvert.SerializeObject(message.Attachments, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new LowercaseContractResolver()
            });

            var attachments = WebUtility.UrlEncode(json);

            var uri = $"https://slack.com/api/chat.postMessage?token={_token}&channel={message.Channel}&as_user={message.As_User}&attachments={attachments}";

            using (var client = new HttpClient())
            {
                //var content = new StringContent(jsonBody, Encoding.UTF8, "text/plain");
                return await client.PostAsync(uri, null).ContinueWith(s => HandleErrors<Message>(s.Result)).Result;
            }
        }

        private async Task<TResult> HandleErrors<TResult>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(responseContent);

            if (response.IsSuccessStatusCode && jobj.SelectToken("$.ok").Value<bool>())
                return jobj.ToObject<TResult>();

            _logger.LogError(responseContent);
            throw new HttpRequestException(responseContent);
        }
    }
}