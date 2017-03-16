## .Net Core Slackbot Library

A simple slackbot library built to listen for and respond to messages.

## Install

`Install-Package Slackbot`

## Usage

```
var bot = new Bot("bot-token");

bot.OnMessage += (sender, message) =>
{
    if (message.MentionedUsers.Any(user => user == "bot-username"))
    {
        bot.SendMessage(message.Channel, $"Hi {message.User}, thanks for mentioning my name!");
    }
};

bot.Run();
```

## Release Notes

https://github.com/mattcbaker/dotnetcore-slackbot/blob/master/RELEASE-NOTES.md

## Nuget

https://www.nuget.org/packages/Slackbot

## License

This code is provided under the under the [MIT license](LICENSE)
