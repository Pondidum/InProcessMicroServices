using System.Collections.Generic;
using System.Linq;

namespace Core.Messaging.Memory
{
	public class MemoryExchange
	{
		private readonly List<MemoryBus> _buses;

		public MemoryExchange()
		{
			_buses = new List<MemoryBus>();
		}

		public void Add(MemoryBus bus)
		{
			_buses.Add(bus);

			bus.AddSink((queue, props) => OnSink(bus, queue, props));
		}

		private void OnSink(MemoryBus bus, string queue, MemoryProps props)
		{
			var others = _buses.Where(b => b != bus);

			foreach (var other in others)
				other.Broadcast(queue, props);

		}
	}
}
