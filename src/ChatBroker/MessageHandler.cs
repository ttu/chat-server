using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
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
        private readonly ILogger<MessageHandler> _logger;
        private readonly string _queueName = "hello";

        public MessageHandler(IConnection connection, 
            IConnectionMultiplexer connectionMultiplexer, 
            IHttpClientFactory httpClientFactory,
            ILogger<MessageHandler> logger)
        {
            _connection = connection;
            _connectionMultiplexer = connectionMultiplexer;
            _httpClientFactory = httpClientFactory;
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

            var clientIp = JObject.Parse(message)["receiver"].Value<string>();

            var db = _connectionMultiplexer.GetDatabase();
            var serverAddressValue = await db.StringGetAsync(clientIp);
            var serverAddress = serverAddressValue.ToString();

            using (var client = _httpClientFactory.CreateClient())
            {
                try
                {
                    var url = $"http://{serverAddress}/api/receive";

                    _logger.LogInformation($"Sending message to: {url}");

                    var result = await client.PostAsync(url, new StringContent(message));

                    if (result.IsSuccessStatusCode == false)
                    {
                        // Add result to send later queue
                        _logger.LogInformation($"Failed sending message to: {url}");
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
    }
}