using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryConnector : IQueueConnector
	{
		private readonly Cache<string, HashSet<MemoryListener>> _exchanges;

		public MemoryConnector()
		{
			_exchanges = new Cache<string, HashSet<MemoryListener>>(
				new Dictionary<string, HashSet<MemoryListener>>(StringComparer.OrdinalIgnoreCase),
				key => new HashSet<MemoryListener>()
			);
		}

		public IDisposable SubscribeTo<T>(string exchangeName, string bindingKey, Action<T> onReceive)
		{
			var listener = new MemoryListener(
				_exchanges[exchangeName],
				bindingKey,
				json => onReceive(JsonConvert.DeserializeObject<T>(json)));

			_exchanges[exchangeName].Add(listener);

			return listener;
		}

		public IMessagePublisher CreatePublisher(string exchangeName)
		{
			return new MemoryPublisher(_exchanges[exchangeName]);
		}
	}
}
