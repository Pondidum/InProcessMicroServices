using System;

namespace Core.Messaging
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo<T>(string exchangeName, string bindingKey, Action<T> onReceive);
		IMessagePublisher CreatePublisher(string exchangeName);
	}
}
