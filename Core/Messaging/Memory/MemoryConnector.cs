using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	[Serializable]
	public class MemoryConnector : IQueueConnector
	{
		private readonly Cache<string, HashSet<Action<MemoryProps>>> _queues;

		public MemoryConnector()
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


		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive)
		{
			var listener = new MemoryListener(
				bindingKey,
				json => onReceive(JsonConvert.DeserializeObject<T>(json)));

			Add(queueName, listener.OnMessage);
			
			return new Remover(() => Remove(queueName, listener.OnMessage));
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			return new MemoryResponder<T>(this, queueName, callback);
		}

		public void Publish(string queueName, string routingKey, object message)
		{
			var json = JsonConvert.SerializeObject(message);
			var props = new MemoryProps
			{
				RoutingKey = routingKey,
				Body = json
			};

			foreach (var listener in _queues[queueName])
			{
				listener(props);
			}
		}

		public void Query<TResponse>(string queueName, object message, Action<TResponse> callback)
		{
			var json = JsonConvert.SerializeObject(message);
			var props = new MemoryProps
			{
				ReplyTo = Guid.NewGuid().ToString(),
				Body = json
			};

			Action<MemoryProps> replyListener = (p) =>
			{
				var instance = JsonConvert.DeserializeObject<TResponse>(p.Body);
				callback(instance);
			};

			_queues[props.ReplyTo].Add(replyListener);

			foreach (var listener in _queues[queueName])
			{
				listener(props);
			}

			_queues[props.ReplyTo].Remove(replyListener);
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
