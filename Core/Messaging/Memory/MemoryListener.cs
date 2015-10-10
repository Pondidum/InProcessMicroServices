using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Messaging.Memory
{
	public class MemoryListener : IDisposable
	{
		private readonly HashSet<MemoryListener> _memoryListeners;
		private readonly Segment _routingKey;
		private readonly Action<string> _onReceive;

		public MemoryListener(HashSet<MemoryListener> memoryListeners, string bindingKey, Action<string> onReceive)
		{
			_memoryListeners = memoryListeners;
			_routingKey = CreateExpressionTree(bindingKey);
			_onReceive = onReceive;
		}

		public void OnMessage(string routingKey, string json)
		{
			if (_routingKey.IsMatch(routingKey.Split('.')) == false)
				return;

			_onReceive(json);
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
				if (_pattern == "#" && _child == null)
				{
					return true;
				}

				var first = parts.First();

				if (first == _pattern || (_pattern == "*" && Regex.IsMatch(first, "^.*$")))
				{
					var remaining = parts.Skip(1).ToArray();

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