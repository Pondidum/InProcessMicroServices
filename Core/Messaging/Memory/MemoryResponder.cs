using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryResponder<TMessage> : IDisposable
	{
		private readonly Cache<string, HashSet<Action<MemoryProps, string>>> _queues;
		private readonly string _queueName;
		private readonly Action<IResponseArgs, TMessage> _callback;

		public MemoryResponder(Cache<string, HashSet<Action<MemoryProps, string>>> queues, string queueName, Action<IResponseArgs, TMessage> callback)
		{
			_queues = queues;
			_queueName = queueName;
			_callback = callback;
			queues[queueName].Add(OnMessage);
		}

		private void OnMessage(MemoryProps props, string json)
		{
			var instance = JsonConvert.DeserializeObject<TMessage>(json);

			_callback(
				new MemoryResponseArgs(_queues, props),
				instance
				);
		}

		public void Dispose()
		{
			_queues[_queueName].Remove(OnMessage);
		}
	}
}
