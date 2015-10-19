using System;
using System.Collections.Generic;

namespace Core.Messaging.Memory
{
	public class MemoryBus
	{
		private readonly Cache<string, HashSet<Action<MemoryProps>>> _queues;

		public MemoryBus()
		{
			_queues = new Cache<string, HashSet<Action<MemoryProps>>>(
				new Dictionary<string, HashSet<Action<MemoryProps>>>(StringComparer.OrdinalIgnoreCase),
				key => new HashSet<Action<MemoryProps>>()
				);
		}

		internal void Add(string queueName, Action<MemoryProps> action)
		{
			_queues[queueName].Add(action);
		}

		internal void Remove(string queueName, Action<MemoryProps> action)
		{
			_queues[queueName].Remove(action);
		}

		internal void Publish(string queueName, MemoryProps props)
		{
			foreach (var listener in _queues[queueName])
			{
				listener(props);
			}
		}

	}
}
