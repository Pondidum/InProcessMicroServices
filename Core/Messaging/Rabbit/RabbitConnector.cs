using System;
using RabbitMQ.Client;

namespace Core.Messaging.Rabbit
{
	public class RabbitConnector : IQueueConnector
	{
		private readonly ConnectionFactory _factory;

		public RabbitConnector(string host)
		{
			_factory = new ConnectionFactory
			{
				HostName = host
			};
		}

		public IDisposable SubscribeTo<T>(string exchangeName, string bindingKey, Action<T> callback)
		{
			return new RabbitListener<T>(_factory, exchangeName, bindingKey, callback);
		}

		public IDisposable SubscribeTo<T>(string exchangeName, Action<IResponseArgs, T> callback)
		{
			return new RabbitResponder<T>(_factory, exchangeName, callback);
		}

		public IMessagePublisher CreatePublisher(string exchangeName)
		{
			return new RabbitMessagePublisher(_factory, exchangeName);
		}
	}
}
