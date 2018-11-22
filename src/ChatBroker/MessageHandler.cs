using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatBroker
{
    public class MessageHandler
    {
        private readonly IConnection _connection;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMessageStore _messageStore;
        private readonly ILogger<MessageHandler> _logger;
        private readonly string _queueName = "hello";

        public MessageHandler(IConnection connection,
            IConnectionMultiplexer connectionMultiplexer,
            IHttpClientFactory httpClientFactory,
            IMessageStore messageStore,
            ILogger<MessageHandler> logger)
        {
            _connection = connection;
            _connectionMultiplexer = connectionMultiplexer;
            _httpClientFactory = httpClientFactory;
            _messageStore = messageStore;
            _logger = logger;

            Start();
        }

        public void Start()
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Task.Run(async () => await HandleMessage(message));
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        }

        public async Task HandleMessage(string message)
        {
            _logger.LogInformation("New messsage");

            var receiverName = JObject.Parse(message)["receiver"].Value<string>();

            var db = _connectionMultiplexer.GetDatabase();
            var serverAddressValue = await db.StringGetAsync(receiverName);
            var serverAddress = serverAddressValue.ToString();

            if (string.IsNullOrEmpty(serverAddress))
            {
                await _messageStore.SaveMessage(receiverName, message);
                _logger.LogInformation($"User: {receiverName} is not online");
                return;
            }

            using (var client = _httpClientFactory.CreateClient())
            {
                var url = $"{serverAddress}/api/receive";

                try
                {
                    _logger.LogInformation($"Sending message to: {url}");

                    var result = await client.PostAsync(url, new StringContent(message));

                    if (result.IsSuccessStatusCode == false)
                    {
                        await _messageStore.SaveMessage(receiverName, message);
                        _logger.LogInformation($"Failed sending message to: {url}");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed sending message to: {url}", e);

                    throw;
                }
            }
        }
    }
}