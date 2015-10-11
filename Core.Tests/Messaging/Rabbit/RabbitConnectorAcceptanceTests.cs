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

		[RequiresRabbitFact]
		public void When_doing_rpc()
		{
			_connector.SubscribeTo<TestMessage>("TestQueue", (c, m) =>
			{
				c.CanRespond().ShouldBe(true);
				c.RespondWith(new TestResponse { Reply = "Yes" });
			});

			var publisher = (RabbitMessagePublisher)_connector.CreatePublisher("TestQueue");
			var wait = new AutoResetEvent(false);

			publisher.Query<TestResponse>(new TestMessage { Name = "Andy Dote" }, m =>
			{
				m.Reply.ShouldBe("Yes");
				wait.Set();
			});

			wait.WaitOne();
		}

		private class TestMessage
		{
			public string Name { get; set; }
		}

		private class TestResponse
		{
			public string Reply { get; set; }
		}
	}
}
