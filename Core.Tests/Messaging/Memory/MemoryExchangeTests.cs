using Core.Messaging.Memory;
using Shouldly;
using Xunit;

namespace Core.Tests.Memory
{
	public class MemoryExchangeTests
	{
		[Fact]
		public void When_there_are_multiple_buses()
		{

			var oneFirst = 0;
			var oneSecond = 0;
			var twoFirst = 0;
			var twoSecond = 0;


			var bus1 = new MemoryBus();
			
			bus1.Add("first", x => oneFirst++);
			bus1.Add("second", x => oneSecond++);


			var bus2 = new MemoryBus();

			bus2.Add("first", x => twoFirst++);
			bus2.Add("second", x => twoSecond++);

			var exchange = new MemoryExchange();
			exchange.Add(bus1);
			exchange.Add(bus2);

			bus1.Publish("first", new MemoryProps { RoutingKey = "first"});

			oneFirst.ShouldBe(1);
			twoFirst.ShouldBe(1);

			oneSecond.ShouldBe(0);
			twoSecond.ShouldBe(0);
		}
	}
}
