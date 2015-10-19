using System;
using Newtonsoft.Json;

namespace Core.Messaging.Memory
{
	public class MemoryConnector : IQueueConnector
	{
		public MemoryConnector()
		{
			Bus = new MemoryBus();
		}

		public MemoryBus Bus { get; }

		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> onReceive)
		{
			var listener = new MemoryListener(
				bindingKey,
				json => onReceive(JsonConvert.DeserializeObject<T>(json)));

			Bus.Add(queueName, listener.OnMessage);

			return new Remover(() => Bus.Remove(queueName, listener.OnMessage));
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			Action<MemoryProps> listener = props =>
			{
				var instance = JsonConvert.DeserializeObject<T>(props.Body);
				var responseProps = new MemoryResponseArgs(this, props);

				callback(responseProps, instance);
			};

			Bus.Add(queueName, listener);

			return new Remover(() => Bus.Remove(queueName, listener));
		}

		public void Publish(string queueName, string routingKey, object message)
		{
			var json = JsonConvert.SerializeObject(message);
			var props = new MemoryProps
			{
				RoutingKey = routingKey,
				Body = json
			};

			Bus.Publish(queueName, props);
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

			Bus.Add(props.ReplyTo, replyListener);
			Bus.Publish(queueName, props);
			Bus.Remove(props.ReplyTo, replyListener);
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
