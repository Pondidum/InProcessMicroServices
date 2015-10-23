using System;
using Core;
using Core.Messaging;

namespace EchoPlugin
{
	public class EchoComponent : IPluginComponent
	{
		public void Initialise(IQueueConnector connector)
		{
			connector.SubscribeTo<MessageDto>("echo", (r, args) =>
			{
				Console.WriteLine("[{0}]EchoPlugin Recieved: {1}", AppDomain.CurrentDomain.FriendlyName, args.Message);
				r.RespondWith(new MessageDto { Message = "Echo: " + args.Message });
			});
		}

		private class MessageDto
		{
			public string Message { get; set; }
		}
	}
}