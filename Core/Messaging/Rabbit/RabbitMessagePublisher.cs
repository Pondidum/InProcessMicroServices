using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Core.Messaging.Rabbit
{
	public class RabbitMessagePublisher : IMessagePublisher
	{
		private readonly ConnectionFactory _factory;
		private readonly string _exchangeName;

		public RabbitMessagePublisher(ConnectionFactory factory, string exchangeName)
		{
			_factory = factory;
			_exchangeName = exchangeName;
		}

		public void Publish(string routingKey, object message)
		{
			var json = JsonConvert.SerializeObject(message);
			var body = Encoding.UTF8.GetBytes(json);

			using(var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, true, false, null);

				var basicProperties = channel.CreateBasicProperties();
				channel.BasicPublish(_exchangeName, routingKey, basicProperties, body);
			}
		}
	}
}
