using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.Messaging.Rabbit
{
	public class RabbitResponder<TMessage> : IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly Action<IResponseArgs, TMessage> _onMessage;

		private readonly EventingBasicConsumer _consumer;

		public RabbitResponder(ConnectionFactory factory, string queueName, Action<IResponseArgs, TMessage> onMessage)
		{
			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
			_onMessage = onMessage;

			_channel.QueueDeclare(queue: queueName,
								 durable: false,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);

			_channel.BasicQos(0, 1, false);

			_consumer = new EventingBasicConsumer(_channel);
			_consumer.Received += OnReceived;

			_channel.BasicConsume(queueName, false, _consumer);
		}

		private void OnReceived(object sender, BasicDeliverEventArgs e)
		{
			var json = Encoding.UTF8.GetString(e.Body);
			var instance = JsonConvert.DeserializeObject<TMessage>(json);
			
			_onMessage(
				new RabbitResponseArgs(_channel, e.BasicProperties),
				instance);

			_channel.BasicAck(e.DeliveryTag, multiple: false);
		}

		public void Dispose()
		{
			_consumer.Received -= OnReceived;
			_channel.Dispose();

			_connection.Dispose();
		}
	}
}
