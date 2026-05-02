using System.Collections.Concurrent;

namespace TaskJect.Web.Services
{
    public class TelegramMessageQueue
    {
        private readonly ConcurrentQueue<(Guid userId, string message)> _messages = new();

        public void Enqueue(Guid userId, string message)
        {
            _messages.Enqueue((userId, message));
        }

        public bool TryDequeue(out (Guid userId, string message) message)
        {
            return _messages.TryDequeue(out message);
        }
    }

}
