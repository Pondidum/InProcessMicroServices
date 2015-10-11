using System;

namespace Core.Messaging
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo<T>(string exchangeName, string bindingKey, Action<T> onReceive);
		IDisposable SubscribeTo<T>(string exchangeName, Action<IResponseArgs, T> callback);

		IMessagePublisher CreatePublisher(string exchangeName);
	}
}
