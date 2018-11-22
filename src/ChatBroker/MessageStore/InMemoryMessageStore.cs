using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBroker
{
    public class Message
    {
        public DateTimeOffset SentAt { get; set; }
        public string Content { get; set; }
    }

    public class InMemoryMessageStore : IMessageStore
    {
        private ConcurrentDictionary<string, List<Message>> _messages = new ConcurrentDictionary<string, List<Message>>();

        public Task<int> SaveMessage(string receiver, string message)
        {
            if (_messages.ContainsKey(receiver))
                _messages.TryAdd(receiver, new List<Message>());

            _messages[receiver].Add(new Message { SentAt = DateTimeOffset.UtcNow, Content = message });

            return Task.FromResult(0);
        }

        public Task<IEnumerable<dynamic>> GetMessages(string receiverName)
        {
            _messages.TryGetValue(receiverName, out List<Message> items);
            IEnumerable<dynamic> result = items.ConvertAll(e => e as dynamic);
            return Task.FromResult(result);
        }

        public Task<DateTimeOffset> GetLastMessageSentTime(string receiverName)
        {
            _messages.TryGetValue(receiverName, out List<Message> items);

            if (items == null)
                return Task.FromResult(DateTimeOffset.MinValue);

            return Task.FromResult(items.Last().SentAt);
        }
    }
}