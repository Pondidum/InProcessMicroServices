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
		private readonly string _exchangeName;
		private readonly string _routingKey;
		private readonly Action<RabbitResponseArgs, TMessage> _onMessage;

		private readonly EventingBasicConsumer _consumer;
		private readonly QueueDeclareOk _queueName;

		public RabbitResponder(ConnectionFactory factory, string exchangeName, string routingKey, Action<RabbitResponseArgs, TMessage> onMessage)
		{
			_connection = factory.CreateConnection();
			_channel = _connection.CreateModel();
			_exchangeName = exchangeName;
			_routingKey = routingKey;
			_onMessage = onMessage;

			//_channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, true, false, null);
			_channel.QueueDeclare(queue: "rpc_queue",
								 durable: false,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);
			_consumer = new EventingBasicConsumer(_channel);
			_consumer.Received += OnReceived;

			_queueName = _channel.QueueDeclare();

			_channel.QueueBind(_queueName, _exchangeName, _routingKey);
			_channel.BasicConsume(_queueName, false, _consumer);
		}

		private void OnReceived(object sender, BasicDeliverEventArgs e)
		{
			var json = Encoding.UTF8.GetString(e.Body);
			var instance = JsonConvert.DeserializeObject<TMessage>(json);

			//_onMessage(instance);
		}

		public void Dispose()
		{
			_consumer.Received -= OnReceived;

			_channel.QueueUnbind(_queueName, _exchangeName, _routingKey, null);
			_channel.Dispose();

			_connection.Dispose();
		}
	}

	public class RabbitResponseArgs
	{
		
	}
}
