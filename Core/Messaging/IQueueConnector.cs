using System;

namespace Core.Messaging
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive);
		IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback);

		void Publish(string queueName, string routingKey, object message);
		void Query<TResponse>(string queueName, object message, Action<TResponse> callback);
	}
}
