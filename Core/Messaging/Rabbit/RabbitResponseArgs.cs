using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Core.Messaging.Rabbit
{
	public class RabbitResponseArgs : IResponseArgs
	{
		private readonly IModel _channel;
		private readonly IBasicProperties _basicProperties;

		public RabbitResponseArgs(IModel channel, IBasicProperties basicProperties)
		{
			_channel = channel;
			_basicProperties = basicProperties;
		}

		public bool CanRespond()
		{
			return _basicProperties.IsReplyToPresent();
		}

		public void RespondWith(object message)
		{
			var props = _channel.CreateBasicProperties();
			props.CorrelationId = _basicProperties.CorrelationId;

			var mJson = JsonConvert.SerializeObject(message);

			_channel.BasicPublish(
				exchange: "",
				routingKey: _basicProperties.ReplyTo,
				basicProperties: props,
				body: Encoding.UTF8.GetBytes(mJson));

		}
	}
}
