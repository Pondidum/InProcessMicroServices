using System;
using System.Configuration;
using System.Threading;
using Core.Messaging.Rabbit;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests.Messaging.Rabbit
{
	public class RabbitConnectorAcceptanceTests
	{
		private readonly ITestOutputHelper _output;
		private readonly RabbitConnector _connector;

		public RabbitConnectorAcceptanceTests(ITestOutputHelper output)
		{
			_output = output;
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
				c.RespondWith(new TestResponse { Reply = 17 });
			});

			var publisher = (RabbitMessagePublisher)_connector.CreatePublisher("TestQueue");
			var wait = new AutoResetEvent(false);

			publisher.Query<TestResponse>(new TestMessage { Name = "Andy Dote" }, m =>
			{
				m.Reply.ShouldBe(17);
				wait.Set();
			});

			wait.WaitOne();
		}

		[RequiresRabbitFact]
		public void When_doing_multiple_rpc()
		{
			var count = 0;
			_connector.SubscribeTo<TestMessage>("MutliQueue", (c, m) =>
			{
				_output.WriteLine("SubscribeTo.Respond {0}", count);
				c.CanRespond().ShouldBe(true);
				c.RespondWith(new TestResponse { Reply = count++ });
			});

			var publisher = (RabbitMessagePublisher)_connector.CreatePublisher("MutliQueue");
			var wait1 = new AutoResetEvent(false);
			var wait2 = new AutoResetEvent(false);

			publisher.Query<TestResponse>(new TestMessage { Name = "Andy Dote" }, m =>
			{
				_output.WriteLine("Query.0");
				m.Reply.ShouldBe(0);
				wait1.Set();
			});

			publisher.Query<TestResponse>(new TestMessage { Name = "Andy Dote" }, m =>
			{
				_output.WriteLine("Query.1");
				m.Reply.ShouldBe(1);
				wait2.Set();
			});

			wait1.WaitOne();
			wait2.WaitOne();
		}

		private class TestMessage
		{
			public string Name { get; set; }
		}

		private class TestResponse
		{
			public int Reply { get; set; }
		}
	}
}
