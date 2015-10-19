using System;
using System.Collections.Generic;

namespace Core.Messaging.Memory
{
	public class MemoryBus
	{
		private readonly Cache<string, HashSet<Action<MemoryProps>>> _queues;
		private readonly HashSet<Action<string, MemoryProps>> _sinks;

		public MemoryBus()
		{
			_queues = new Cache<string, HashSet<Action<MemoryProps>>>(
				new Dictionary<string, HashSet<Action<MemoryProps>>>(StringComparer.OrdinalIgnoreCase),
				key => new HashSet<Action<MemoryProps>>()
			);

			_sinks = new HashSet<Action<string, MemoryProps>>();
		}

		internal void AddSink(Action<string, MemoryProps> sink)
		{
			_sinks.Add(sink);
		}

		public void Add(string queueName, Action<MemoryProps> action)
		{
			_queues[queueName].Add(action);
		}

		public void Remove(string queueName, Action<MemoryProps> action)
		{
			_queues[queueName].Remove(action);
		}

		public void Publish(string queueName, MemoryProps props)
		{
			Broadcast(queueName, props);

			foreach (var sink in _sinks)
				sink(queueName, props);
		}

		internal void Broadcast(string queueName, MemoryProps props)
		{
			foreach (var listener in _queues[queueName])
				listener(props);
		}
	}
}
