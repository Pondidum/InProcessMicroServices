namespace Core.Messaging.Memory
{
	public class MemoryProps
	{
		public string RoutingKey { get; set; }
		public string ReplyTo { get; set; }

		public string Body { get; set; }
	}
}
