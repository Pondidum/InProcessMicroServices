using System;
using System.Configuration;
using System.Threading;
using Core.Messaging.Rabbit;
using Shouldly;
using Xunit;

namespace Core.Tests.Messaging.Rabbit
{
	public class RabbitConnectorAcceptanceTests
	{
		private readonly RabbitConnector _connector;

		public RabbitConnectorAcceptanceTests()
		{
			_connector = new RabbitConnector(ConfigurationManager.AppSettings["RabbitHost"]);
		}

		[RequiresRabbitFact]
		public void When_subscribing_to_a_message()
		{
			TestMessage message = null;
			var wait = new AutoResetEvent(false);

			Action<TestMessage> callback = m =>
			{
				wait.Set();
				message = m;
			};

			using (_connector.SubscribeTo("TestExchange", "#", callback))
			{
				_connector
					.CreatePublisher("TestExchange")
					.Publish("Person.Create", new TestMessage { Name = "Andy Dote" });

				wait.WaitOne();
				message.Name.ShouldBe("Andy Dote");
			}
		}

		private class TestMessage
		{
			public string Name { get; set; }
		}
	}
}
