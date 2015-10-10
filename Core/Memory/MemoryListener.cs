using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Memory
{
	public class MemoryListener : IDisposable
	{
		private readonly HashSet<MemoryListener> _memoryListeners;
		private readonly string _routingKey;
		private readonly Action<object> _onReceive;

		public MemoryListener(HashSet<MemoryListener> memoryListeners, string routingKey, Action<object> onReceive)
		{
			_memoryListeners = memoryListeners;
			_routingKey = routingKey;
			_onReceive = onReceive;
		}

		public void OnMessage(string routingKey, object message)
		{
			if (string.Equals(routingKey, _routingKey, StringComparison.OrdinalIgnoreCase) == false)
				return;

			_onReceive(message);
		}

		public static Segment CreateExpressionTree(string routingPattern)
		{
			return routingPattern.Split('.')
				.Reverse()
				.Aggregate((Segment)null, (agg, part) => new Segment(part, agg));
		}

		public static bool Matches(Segment routingTree, string messageRoutingKey)
		{
			return routingTree.IsMatch(messageRoutingKey.Split('.'));
		}

		public class Segment
		{
			private readonly string _pattern;
			private readonly Segment _child;

			public Segment(string partPattern, Segment child)
			{
				_pattern = partPattern;
				_child = child;
			}

			public bool IsMatch(string[] parts)
			{
				var first = parts.First();
				var remaining = parts.Skip(1).ToArray();

				if (_pattern == "#" && _child == null)
				{
					return true;
				}

				if (first == _pattern || (_pattern == "*" && Regex.IsMatch(first, "^.*$")))
				{
					if (_child != null && remaining.Any())
						return _child.IsMatch(remaining);

					return _child == null && remaining.Any() == false;
				}

				return false;
			}
		}



		public void Dispose()
		{
			_memoryListeners.Remove(this);
		}
	}
}
