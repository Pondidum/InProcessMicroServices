using System;
using System.Collections.Generic;

namespace Core.Messaging.Memory
{
	public class MemoryResponseArgs : IResponseArgs
	{
		private readonly Cache<string, HashSet<Action<MemoryProps, string>>> _queues;
		private readonly MemoryProps _props;

		public MemoryResponseArgs(Cache<string, HashSet<Action<MemoryProps, string>>> queues, MemoryProps props)
		{
			_queues = queues;
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

			var publisher = new MemoryPublisher(null, _queues[_props.ReplyTo]);

			publisher.Publish(_props.RoutingKey, message);
		}
	}
}
