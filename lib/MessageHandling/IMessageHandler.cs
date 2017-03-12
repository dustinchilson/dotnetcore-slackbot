using System.Threading.Tasks;
using Slackbot.Model;

namespace Slackbot.MessageHandling
{
    public interface IMessageHandler
    {
        Task<bool> ShouldHandleAsync(IncomingMessage message);
        Task HandleMessageAsync(Bot bot, IncomingMessage message);
    }
}