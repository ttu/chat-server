using RabbitMQ.Client;
using System.Linq;
using System.Text;

namespace ChatServer
{
    public interface IMessageSender
    {
        void Send(string message);
    }

    public class Message
    {
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public string Payload { get; set; }
    }

    public class MessageSender : IMessageSender
    {
        private readonly IModel _channel;
        private readonly string _queueName = "hello";

        public MessageSender(IModel channel)
        {
            _channel = channel;
            // TODO: Should use QueueDeclareNoWait in initialization?
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Send(string message)
        {
            string output = new string(message.Where(c => !char.IsControl(c)).ToArray());
            var body = Encoding.UTF8.GetBytes(output);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}