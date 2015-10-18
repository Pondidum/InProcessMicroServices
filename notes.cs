public class Scratchpad
	{
		private readonly ITestOutputHelper _output;

		public Scratchpad(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void When_testing_something()
		{

			var bus1 = new Bus();

			bus1.Subscribe("first", x => _output.WriteLine("Bus1 => ({0}) => {1}", "first", x.Json));
			bus1.Subscribe("second", x => _output.WriteLine("Bus1 => ({0}) => {1}", "second", x.Json));


			var bus2 = new Bus();

			bus2.Subscribe("first", x => _output.WriteLine("Bus2 => ({0}) => {1}", "first", x.Json));
			bus2.Subscribe("second", x => _output.WriteLine("Bus2 => ({0}) => {1}", "second", x.Json));

			var exchange = new Exchange();
			exchange.AddBus(bus1);
			exchange.AddBus(bus2);

			bus1.Publish("first", new Message { Json = "a message"});
		}


		private class Exchange
		{
			private readonly List<Bus> _buses;

			public Exchange()
			{
				_buses = new List<Bus>();
			}

			public void AddBus(Bus bus)
			{
				_buses.Add(bus);

				bus.AddSink((queue, message) => OnSinkMessage(bus, queue, message));
			}

			private void OnSinkMessage(Bus bus, string queueName, Message message)
			{
				var otherBuses = _buses.Where(b => b != bus);

				foreach (var other in otherBuses)
				{
					other.Broadcast(queueName, message);
				}
			}
		}

		private class Bus
		{
			private readonly Dictionary<string, List<Action<Message>>> _subscribers;
			private readonly List<Action<string, Message>> _sinks;

			public Bus()
			{
				_subscribers = new Dictionary<string, List<Action<Message>>>(StringComparer.OrdinalIgnoreCase);
				_sinks = new List<Action<string, Message>>();
			}

			public void AddSink(Action<string, Message> sink)
			{
				_sinks.Add(sink);
			}

			public void Subscribe(string queueName, Action<Message> onMessage)
			{
				if (_subscribers.ContainsKey(queueName) == false)
					_subscribers[queueName] = new List<Action<Message>>();

				_subscribers[queueName].Add(onMessage);
			}

			public void Publish(string queueName, Message message)
			{
				Broadcast(queueName, message);
				_sinks.ForEach(sink => sink(queueName, message));
			}

			public void Broadcast(string queueName, Message message)
			{
				if (_subscribers.ContainsKey(queueName) == false)
					return;

				_subscribers[queueName].ForEach(subsciber => subsciber(message));
			}

		}

		private class Message
		{
			public string RoutingKey { get; set; }
			public string Json { get; set; }

			public static Message From(string key, object content)
			{
				return new Message
				{
					RoutingKey = key,
					Json = JsonConvert.SerializeObject(content)
				};
			}
		}
	}
