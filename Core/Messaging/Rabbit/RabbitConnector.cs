using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.Messaging.Rabbit
{
	public class RabbitConnector : IQueueConnector
	{
		private readonly ConnectionFactory _factory;

		public RabbitConnector(string host)
		{
			_factory = new ConnectionFactory
			{
				HostName = host
			};
		}

		public IDisposable SubscribeTo<T>(string queueName, string bindingKey, Action<T> callback)
		{
			return new RabbitListener<T>(_factory, queueName, bindingKey, callback);
		}

		public IDisposable SubscribeTo<T>(string queueName, Action<IResponseArgs, T> callback)
		{
			return new RabbitResponder<T>(_factory, queueName, callback);
		}

		public void Publish(string queueName, string routingKey, object message)
		{
			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(queueName, ExchangeType.Topic, true, false, null);

				var basicProperties = channel.CreateBasicProperties();
				channel.BasicPublish(queueName, routingKey, basicProperties, BodyFrom(message));
			}
		}

		public void Query<TResponse>(string queueName, object message, Action<TResponse> callback)
		{
			var connection = _factory.CreateConnection();
			var channel = connection.CreateModel();

			var correlationID = Guid.NewGuid().ToString();
			var replyTo = channel.QueueDeclare().QueueName;

			var listener = new EventingBasicConsumer(channel);
			channel.BasicConsume(replyTo, true, listener);
			listener.Received += (s, e) =>
			{
				if (e.BasicProperties.CorrelationId != correlationID)
					return;

				try
				{
					callback(MessageFrom<TResponse>(e.Body));
				}
				finally
				{
					channel.Dispose();
					connection.Dispose();
				}
			};

			var props = channel.CreateBasicProperties();
			props.CorrelationId = correlationID;
			props.ReplyTo = replyTo;

			channel.BasicPublish(
				exchange: "",
				routingKey: queueName,
				basicProperties: props,
				body: BodyFrom(message));
		}


		private static byte[] BodyFrom(object message)
		{
			var json = JsonConvert.SerializeObject(message);
			return Encoding.UTF8.GetBytes(json);
		}

		private static T MessageFrom<T>(byte[] body)
		{
			var json = Encoding.UTF8.GetString(body);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
