using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Core.Rabbit
{
	public class RabbitListener : IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly string _exchangeName;
		private readonly string _routingKey;
		private readonly Action<object> _onMessage;

		private readonly EventingBasicConsumer _consumer;
		private readonly QueueDeclareOk _queueName;

		public RabbitListener(IConnection connection, string exchangeName, string routingKey, Action<object> onMessage)
		{
			_connection = connection;
			_channel = connection.CreateModel();
			_exchangeName = exchangeName;
			_routingKey = routingKey;
			_onMessage = onMessage;

			_consumer = new EventingBasicConsumer(_channel);
			_consumer.Received += OnReceived;

			_queueName = _channel.QueueDeclare();

			_channel.QueueBind(_queueName, _exchangeName, _routingKey);
			_channel.BasicConsume(_queueName, false, _consumer);
		}

		private void OnReceived(object sender, BasicDeliverEventArgs e)
		{
			_onMessage(e.Body);
		}

		public void Dispose()
		{
			_consumer.Received -= OnReceived;

			_channel.QueueUnbind(_queueName, _exchangeName, _routingKey, null);
			_channel.Dispose();

			_connection.Dispose();
		}
	}
}
