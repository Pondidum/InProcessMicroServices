using System;

namespace Core.Messaging
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive);
		IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback);

		IMessagePublisher CreatePublisher(string queueName);
	}
}
