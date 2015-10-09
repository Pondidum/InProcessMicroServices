using System;

namespace Core
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo(string exchangeName, string routingKey, Action<object> onReceive);
	}
}
