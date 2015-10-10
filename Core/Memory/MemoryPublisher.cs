using System.Collections.Generic;

namespace Core.Memory
{
	public class MemoryPublisher : IMessagePublisher
	{
		private readonly HashSet<MemoryListener> _listeners;

		public MemoryPublisher(HashSet<MemoryListener> memoryListeners)
		{
			_listeners = memoryListeners;
		}

		public void Publish(string routingKey, object message)
		{
			foreach (var listener in _listeners)
			{
				listener.OnMessage(routingKey, message);
			}
		}
	}
}
