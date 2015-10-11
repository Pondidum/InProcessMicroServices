using System;

namespace Core.Messaging
{
	public interface IMessagePublisher
	{
		void Publish(string routingKey, object message);
		void Query<TResponse>(object message, Action<TResponse> callback);
	}
}
