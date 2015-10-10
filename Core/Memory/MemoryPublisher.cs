using System.Collections.Generic;
using Newtonsoft.Json;

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
			var json = JsonConvert.SerializeObject(message);

			foreach (var listener in _listeners)
			{
				var instance = JsonConvert.DeserializeObject(json, message.GetType());

				listener.OnMessage(routingKey, instance);
			}
		}
	}
}
