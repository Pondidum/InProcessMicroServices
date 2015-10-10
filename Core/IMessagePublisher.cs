namespace Core
{
	public interface IMessagePublisher
	{
		void Publish(string routingKey, object message);
	}
}
