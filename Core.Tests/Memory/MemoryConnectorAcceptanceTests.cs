using System;
using Core.Memory;
using Shouldly;
using Xunit;

namespace Core.Tests.Memory
{
	public class MemoryConnectorAcceptanceTests
	{
		private const string ExchangeName = "domainMessages";
		private readonly MemoryConnector _connector;
		private readonly Message _message;

		public MemoryConnectorAcceptanceTests()
		{
			_connector = new MemoryConnector();
			_message = new Message
			{
				ID = Guid.NewGuid(),
				Name = "some person name"
			};
		}

		[Fact]
		public void When_publishing_and_there_are_no_subscribers()
		{
			_connector
				.CreatePublisher(ExchangeName)
				.Publish("people.create", _message);
		}

		[Fact]
		public void When_publishing_and_there_is_one_matching_subscriber()
		{
			Message received = null;
			_connector.SubscribeTo(ExchangeName, "people.create", m => received = (Message)m);

			_connector
				.CreatePublisher(ExchangeName)
				.Publish("people.create", _message);

			received.ShouldNotBeSameAs(_message);
			received.ID.ShouldBe(_message.ID);
			received.Name.ShouldBe(_message.Name);
		}

		[Fact]
		public void When_publishing_and_there_are_multiple_matching_subscribers()
		{
			Message first = null;
			Message second = null;

			_connector.SubscribeTo(ExchangeName, "people.create", m => first = (Message)m);
			_connector.SubscribeTo(ExchangeName, "people.create", m => second = (Message)m);

			_connector
				.CreatePublisher(ExchangeName)
				.Publish("people.create", _message);

			first.ShouldNotBeSameAs(_message);
			first.ID.ShouldBe(_message.ID);
			first.Name.ShouldBe(_message.Name);

			second.ShouldNotBeSameAs(_message);
			second.ID.ShouldBe(_message.ID);
			second.Name.ShouldBe(_message.Name);

			first.ShouldNotBeSameAs(second);
		}

		[Fact]
		public void When_publishing_and_there_is_one_non_matching_subscriber()
		{
			Message received = null;
			_connector.SubscribeTo(ExchangeName, "people.create", m => received = (Message)m);

			_connector
				.CreatePublisher(ExchangeName)
				.Publish("people.edit", _message);

			received.ShouldBe(null);
		}

		[Fact]
		public void When_publishing_and_there_are_multiple_non_matching_subscribers()
		{
			Message first = null;
			Message second = null;

			_connector.SubscribeTo(ExchangeName, "people.create", m => first = (Message)m);
			_connector.SubscribeTo(ExchangeName, "people.create", m => second = (Message)m);

			_connector
				.CreatePublisher(ExchangeName)
				.Publish("people.edit", _message);

			first.ShouldBe(null);
			second.ShouldBe(null);
		}

		private class Message
		{
			public Guid ID { get; set; }
			public string Name { get; set; }
		}
	}
}