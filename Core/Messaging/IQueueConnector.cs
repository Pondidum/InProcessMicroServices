using System;

namespace Core.Messaging
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo(string exchangeName, string bindingKey, Action<object> onReceive);
	}
}
