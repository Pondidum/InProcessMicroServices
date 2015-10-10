namespace Core.Messaging
{
	public interface IMessagePublisher
	{
		void Publish(string routingKey, object message);
	}
}
