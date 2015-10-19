namespace Core.Messaging.Memory
{
	public class MemoryResponseArgs : IResponseArgs
	{
		private readonly MemoryConnector _connector;
		private readonly MemoryProps _props;

		public MemoryResponseArgs(MemoryConnector connector, MemoryProps props)
		{
			_connector = connector;
			_props = props;
		}

		public bool CanRespond()
		{
			return string.IsNullOrWhiteSpace(_props.ReplyTo) == false;
		}

		public void RespondWith(object message)
		{
			if (CanRespond() == false)
				return;

			_connector.Publish(_props.ReplyTo, _props.RoutingKey, message);
		}
	}
}
