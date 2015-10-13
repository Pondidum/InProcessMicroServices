using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryResponder<TMessage> : IDisposable
	{
		private readonly MemoryConnector _connector;
		private readonly string _queueName;
		private readonly Action<IResponseArgs, TMessage> _callback;

		public MemoryResponder(MemoryConnector connector, string queueName, Action<IResponseArgs, TMessage> callback)
		{
			_connector = connector;
			_queueName = queueName;
			_callback = callback;

			_connector.Add(queueName, OnMessage);
		}

		private void OnMessage(MemoryProps props, string json)
		{
			var instance = JsonConvert.DeserializeObject<TMessage>(json);

			_callback(
				new MemoryResponseArgs(_connector, props),
				instance
				);
		}

		public void Dispose()
		{
			_connector.Remove(_queueName, OnMessage);
		}
	}
}
