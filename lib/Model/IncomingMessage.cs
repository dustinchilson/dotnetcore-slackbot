using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Slackbot.Model
{
    public class IncomingMessage : Message
    {
        private string _user;

        public IncomingMessage() 
            : base(null, null)
        {
        }

        public IEnumerable<string> MentionedUsers { get; set; } = new string[0];
        public string User { get; set; }
        public string UserName { get; set; }

        [JsonIgnore]
        public string RawJson { get; set; }

        internal async Task FindSendingUser(Slack slack)
        {
            this.UserName = await slack.GetUsername(this.User);
        }

        internal async Task FindMentionedUsers(Slack slack)
        {
            var mentionedUsers = new List<string>();
            var matches = Regex.Matches(this.RawJson, "<@(.*?)>");

            foreach (Match match in matches)
            {
                mentionedUsers.Add(await slack.GetUsername(match.Groups[1].ToString()));
            }

            this.MentionedUsers = mentionedUsers;
        }
    }
}