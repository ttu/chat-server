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
        private readonly string _queueName = "hello";

        public MessageHandler(IConnection connection, IConnectionMultiplexer connectionMultiplexer, IHttpClientFactory httpClientFactory)
        {
            _connection = connection;
            _connectionMultiplexer = connectionMultiplexer;
            _httpClientFactory = httpClientFactory;

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
            var clientIp = JObject.Parse(message)["receiver"].Value<string>();

            var db = _connectionMultiplexer.GetDatabase();
            var serverAddressValue = await db.StringGetAsync(clientIp);
            var serverAddress = serverAddressValue.ToString();

            using (var client = _httpClientFactory.CreateClient())
            {
                try
                {
                    var result = await client.PostAsJsonAsync("http://" + serverAddress + "/api/receive", message);

                    if (result.IsSuccessStatusCode == false)
                    {
                        // Add result to send later queue
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