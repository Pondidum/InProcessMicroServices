using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.Messaging.Rabbit
{
	public class RabbitMessagePublisher : IMessagePublisher
	{
		private readonly ConnectionFactory _factory;
		private readonly string _exchangeName;

		public RabbitMessagePublisher(ConnectionFactory factory, string exchangeName)
		{
			_factory = factory;
			_exchangeName = exchangeName;
		}

		public void Publish(string routingKey, object message)
		{
			using (var connection = _factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, true, false, null);

				var basicProperties = channel.CreateBasicProperties();
				channel.BasicPublish(_exchangeName, routingKey, basicProperties, BodyFrom(message));
			}
		}

		public void Query<TResponse>(object message, Action<TResponse> callback)
		{
			var connection = _factory.CreateConnection();
			var channel = connection.CreateModel();

			var correlationID = Guid.NewGuid().ToString();
			var replyTo = channel.QueueDeclare().QueueName;

			var listener = new EventingBasicConsumer(channel);
			channel.BasicConsume(replyTo, true, listener);
			listener.Received += (s, e) =>
			{
				if (e.BasicProperties.CorrelationId == correlationID)
				{
					callback(MessageFrom<TResponse>(e.Body));
					channel.Dispose();
					connection.Dispose();
				}
			};

			var props = channel.CreateBasicProperties();
			props.CorrelationId = correlationID;
			props.ReplyTo = replyTo;

			channel.BasicPublish(
				exchange: "",
				routingKey: _exchangeName,
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
