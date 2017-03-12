using System;

namespace Slackbot.Model
{
    public class Message
    {
        public Message(string channel, string text)
        {
            Channel = channel;
            Text = text;
            Type = "message";
        }

        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Type { get; private set; }

        public string Text { get; set; }
        public string Channel { get; set; }
    }
}