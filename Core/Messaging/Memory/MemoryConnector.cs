using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryConnector : IQueueConnector
	{
		private readonly Cache<string, HashSet<Action<MemoryProps, string>>> _queues;

		public MemoryConnector()
		{
			_queues = new Cache<string, HashSet<Action<MemoryProps, string>>>(
				new Dictionary<string, HashSet<Action<MemoryProps, string>>>(StringComparer.OrdinalIgnoreCase),
				key => new HashSet<Action<MemoryProps, string>>()
			);
		}

		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive)
		{
			var listener = new MemoryListener(
				bindingKey,
				json => onReceive(JsonConvert.DeserializeObject<T>(json)));

			_queues[queueName].Add(listener.OnMessage);
			
			return new Remover(() => _queues[queueName].Remove(listener.OnMessage));
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			return new MemoryResponder<T>(_queues, queueName, callback);
		}

		public IMessagePublisher CreatePublisher(string queueName)
		{
			return new MemoryPublisher(_queues, _queues[queueName]);
		}

		private class Remover : IDisposable
		{
			private readonly Action _onDispose;

			public Remover(Action onDispose)
			{
				_onDispose = onDispose;
			}

			public void Dispose()
			{
				_onDispose();
			}
		}
	}
}
