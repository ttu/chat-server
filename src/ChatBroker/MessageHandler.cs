
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ChatBroker
{
    public class MessageHandler
    {
        private readonly IConnection _connection;
        private readonly string _queueName = "hello";

        public MessageHandler(IConnection connection)
        {
            _connection = connection;
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
                HandleMessage(message);
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        }

        public void HandleMessage(string message)
        {
            // Get server url
        }
    }
}