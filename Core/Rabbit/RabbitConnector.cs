using System;
using RabbitMQ.Client;

namespace Core.Rabbit
{
	public class RabbitConnector : IQueueConnector
	{
		private readonly ConnectionFactory _factory;

		public RabbitConnector()
		{
			_factory = new ConnectionFactory();
		}

		public IDisposable SubscribeTo(string exchangeName, string routingKey, Action<object> onReceive)
		{
			var connection = _factory.CreateConnection();

			return new RabbitListener(connection, exchangeName, routingKey, onReceive);
		}

	}
}
