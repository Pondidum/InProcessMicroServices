using System;
using System.Collections.Generic;

namespace Core.Memory
{
	public class MemoryListener : IDisposable
	{
		private readonly HashSet<MemoryListener> _memoryListeners;
		private readonly string _routingKey;
		private readonly Action<object> _onReceive;

		public MemoryListener(HashSet<MemoryListener> memoryListeners, string routingKey, Action<object> onReceive)
		{
			_memoryListeners = memoryListeners;
			_routingKey = routingKey;
			_onReceive = onReceive;
		}

		public void OnMessage(string routingKey, object message)
		{
			if (string.Equals(routingKey, _routingKey, StringComparison.OrdinalIgnoreCase) == false)
				return;

			_onReceive(message);
		}

		public void Dispose()
		{
			_memoryListeners.Remove(this);
		}
	}
}
