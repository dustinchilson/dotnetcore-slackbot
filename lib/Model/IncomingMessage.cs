using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Slackbot.Model
{
    public class IncomingMessage : Message
    {
        public IncomingMessage() 
            : base(null, null)
        {
        }

        public IEnumerable<string> MentionedUsers { get; set; } = new string[0];
        public string RawJson { get; set; }

        internal async Task<IncomingMessage> FindMentionedUsers(Slack slack, string message)
        {
            var mentionedUsers = new List<string>();
            var matches = Regex.Matches(message, "<@(.*?)>");

            foreach (Match match in matches)
            {
                mentionedUsers.Add(await slack.GetUsername(match.Groups[1].ToString()));
            }

            this.MentionedUsers = mentionedUsers;
            return this;
        }
    }
}