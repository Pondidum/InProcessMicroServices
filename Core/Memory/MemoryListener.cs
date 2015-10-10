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

		public static bool Matches(string routingPattern, string messageRoutingKey)
		{
			var parts = routingPattern.Split('.');
			var s = new Segment(parts.First());
			var root = s;

			foreach (var source in parts.Skip(1))
			{
				s = s.SetChild(new Segment(source));
			}

			return root.IsMatch(messageRoutingKey.Split('.'));

		}

		private class Segment
		{
			private string _pattern;
			private Segment _child;

			public Segment(string partPattern)
			{
				_pattern = partPattern;
			}

			public Segment SetChild(Segment child)
			{
				_child = child;
				return child;
			}

			public bool IsMatch(IEnumerable<string> parts)
			{
				var first = parts.First();
				var remaining = parts.Skip(1);

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
