using System;
using System.Collections.Generic;

namespace Slackbot.Model
{
    public class AdvancedMessage
    {
        public AdvancedMessage()
        {
            As_User = true;
        }

        public bool As_User { get; private set; }

        public string Channel { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}