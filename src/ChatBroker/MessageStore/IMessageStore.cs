using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBroker
{
    public interface IMessageStore
    {
        Task<int> SaveMessage(string receiver, string message);

        Task<IEnumerable<dynamic>> GetMessages(string receiverName);

        Task<DateTimeOffset> GetLastMessageSentTime(string receiverName);
    }
}