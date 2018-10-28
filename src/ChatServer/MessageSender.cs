using RabbitMQ.Client;
using System.Text;

namespace ChatServer
{
    public interface IMessageSender
    {
        void Send(string message);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IModel _channel;
        private readonly string _queueName = "hello";

        public MessageSender(IModel channel)
        {
            _channel = channel;
            // TODO: Should use QueueDeclareNoWait in initialization?
            _channel.QueueDeclareNoWait(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Send(string message)
        {
            //string output = new string(message.Where(c => !char.IsControl(c)).ToArray());
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}