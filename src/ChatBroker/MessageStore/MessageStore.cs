using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatBroker
{
    public class MessageStore : IMessageStore
    {
        private const string _insertCommand = "INSERT INTO Message (Receiver, SentAt, Message) VALUES (@receiver, @sentAt, @messag)";
        private const string _allMessagesQuery = "SELECT * FROM Message WHERE Receiver = @receiverName";
        private const string _lastMessageTimeQuery = "SELECT MAX(SentAt) FROM Message WHERE Receiver = @receiverName";

        private readonly string _connectionString;

        public MessageStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> SaveMessage(string receiver, string message)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                using (var cmd = new NpgsqlCommand(_insertCommand, conn))
                {
                    cmd.Parameters.AddWithValue("receiver", receiver);
                    cmd.Parameters.AddWithValue("sentAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    cmd.Parameters.AddWithValue("message", message);
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<IEnumerable<dynamic>> GetMessages(string receiverName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return await conn.QueryAsync<dynamic>(_allMessagesQuery, new { receiverName });
            }
        }

        public async Task<DateTimeOffset> GetLastMessageSentTime(string receiverName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var lastTime = await conn.QueryFirstAsync<long>(_lastMessageTimeQuery, new { receiverName });
                return DateTimeOffset.FromUnixTimeMilliseconds(lastTime);
            }
        }
    }
}