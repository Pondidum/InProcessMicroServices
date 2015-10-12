using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryPublisher : IMessagePublisher
	{
		private readonly Cache<string, HashSet<Action<MemoryProps, string>>> _queues;
		private readonly IEnumerable<Action<MemoryProps, string>> _listeners;

		public MemoryPublisher(Cache<string, HashSet<Action<MemoryProps, string>>> queues, IEnumerable<Action<MemoryProps, string>> listeners)
		{
			_queues = queues;
			_listeners = listeners;
		}

		public void Publish(string routingKey, object message)
		{
			var json = JsonConvert.SerializeObject(message);
			var props = new MemoryProps { RoutingKey = routingKey };

			foreach (var listener in _listeners)
			{
				listener(props, json);
			}
		}

		public void Query<TResponse>(object message, Action<TResponse> callback)
		{
			var json = JsonConvert.SerializeObject(message);
			var props = new MemoryProps { ReplyTo = Guid.NewGuid().ToString() };

			Action<MemoryProps, string> replyListener = (p, j) =>
			{
				var instance = JsonConvert.DeserializeObject<TResponse>(j);
				callback(instance);
			};

			_queues[props.ReplyTo].Add(replyListener);

			foreach (var listener in _listeners)
			{
				listener(props, json);
			}

			_queues[props.ReplyTo].Remove(replyListener);
		}
	}
}
