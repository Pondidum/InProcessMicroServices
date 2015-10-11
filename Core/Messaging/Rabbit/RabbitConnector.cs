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

		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> callback)
		{
			return new RabbitListener<T>(_factory, queueName, bindingKey, callback);
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			return new RabbitResponder<T>(_factory, queueName, callback);
		}

		public IMessagePublisher CreatePublisher(string queueName)
		{
			return new RabbitMessagePublisher(_factory, queueName);
		}
	}
}
