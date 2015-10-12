using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
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
			var props = new MemoryProps { RoutingKey = routingKey};

			foreach (var listener in _listeners)
			{
				listener.OnMessage(props, json);
			}
		}

		public void Query<TResponse>(object message, Action<TResponse> callback)
		{
			throw new NotImplementedException();
		}
	}
}
