using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Messaging;

namespace LongRunning
{
	public class LongRunningComponent : IPluginComponent, IDisposable
	{
		private readonly HashSet<IDisposable> _listeners;
		private readonly List<KeyValuePair<string, CancellationTokenSource>> _scanners;
		private readonly Random _random;
		private IQueueConnector _connector;

		public LongRunningComponent()
		{
			_random = new Random();
			_listeners = new HashSet<IDisposable>();
			_scanners = new List<KeyValuePair<string, CancellationTokenSource>>();
		}

		private void RunTask(string key, CancellationTokenSource source)
		{
			var i = 0;
			var sleep = _random.Next(500, 5000);
			while (source.IsCancellationRequested == false)
			{
				_connector.Publish("Notifications", "Scanner.Pulse", new { Name = key, Count = i });
				i++;

				Thread.Sleep(sleep);
			}
		}


		public void Initialise(IQueueConnector connector)
		{
			_listeners.Add(connector.SubscribeTo<ScannerControl>("Notifications", "Scanner.Start", OnStartScanner));
			_listeners.Add(connector.SubscribeTo<ScannerControl>("Notifications", "Scanner.Stop", OnStopScanner));

			_connector = connector;
		}

		private void OnStartScanner(ScannerControl message)
		{
			var source = new CancellationTokenSource();

			Task.Run(() => RunTask(message.Name, source), source.Token);

			_scanners.Add(new KeyValuePair<string, CancellationTokenSource>(message.Name, source));
		}

		private void OnStopScanner(ScannerControl message)
		{
			var toStop = _scanners.Where(s => s.Key == message.Name).ToList();

			foreach (var pair in toStop)
			{
				_scanners.Remove(pair);
				pair.Value.Cancel();
			}
		}

		public void Dispose()
		{
			foreach (var listener in _listeners)
			{
				listener.Dispose();
			}

			_listeners.Clear();
		}

		private class ScannerControl
		{
			public string Name { get; set; }
			public string Term { get; set; }
		}
	}
}
