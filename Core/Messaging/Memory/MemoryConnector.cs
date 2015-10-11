using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryConnector : IQueueConnector
	{
		private readonly Cache<string, HashSet<MemoryListener>> _queues;

		public MemoryConnector()
		{
			_queues = new Cache<string, HashSet<MemoryListener>>(
				new Dictionary<string, HashSet<MemoryListener>>(StringComparer.OrdinalIgnoreCase),
				key => new HashSet<MemoryListener>()
			);
		}

		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive)
		{
			var listener = new MemoryListener(
				_queues[queueName],
				bindingKey,
				json => onReceive(JsonConvert.DeserializeObject<T>(json)));

			_queues[queueName].Add(listener);

			return listener;
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			throw new NotImplementedException();
		}

		public IMessagePublisher CreatePublisher(string queueName)
		{
			return new MemoryPublisher(_queues[queueName]);
		}
	}
}
