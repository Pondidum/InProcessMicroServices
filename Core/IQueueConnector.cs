using System;

namespace Core
{
	public interface IQueueConnector
	{
		IDisposable SubscribeTo(string exchangeName, string bindingKey, Action<object> onReceive);
	}
}
