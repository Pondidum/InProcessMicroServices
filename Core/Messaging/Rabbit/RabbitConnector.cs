using System;
using RabbitMQ.Client;

namespace Core.Messaging.Rabbit
{
	public class RabbitConnector : IQueueConnector
	{
		private readonly ConnectionFactory _factory;

		public RabbitConnector()
		{
			_factory = new ConnectionFactory();
		}

		public IDisposable SubscribeTo<T>(string exchangeName, string bindingKey, Action<T> onReceive)
		{
			var connection = _factory.CreateConnection();

			return new RabbitListener<T>(connection, exchangeName, bindingKey, onReceive);
		}

		public IMessagePublisher CreatePublisher(string exchangeName)
		{
			throw new NotImplementedException();
		}
	}
}
