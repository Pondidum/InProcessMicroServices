using System;
using Core.Messaging;

namespace HostApplication
{
	public class MainPresenter : IDisposable
	{
		private readonly IMainView _view;
		private readonly IDisposable _listener;
		private IQueueConnector _connector;

		public MainPresenter(IMainView view, IQueueConnector connector)
		{
			_view = view;

			_listener = connector.SubscribeTo<ScannerPulse>("Notifications", "Scanner.Pulse", OnScannerPulse);
			_connector = connector;
			
			_view.StartClicked += OnStartClicked;
			_view.StopClicked += OnStopClicked;
		}

		private void OnScannerPulse(ScannerPulse pulse)
		{
			_view.UpdateCounter(pulse.Count);
		}

		private void OnStartClicked()
		{
			_connector.Publish("Notifications", "Scanner.Start", new ScannerControl
			{
				Name ="TestScanner",
				Term = "Omg!"
			});
		}

		private void OnStopClicked()
		{
			_connector.Publish("Notifications", "Scanner.Stop", new ScannerControl
			{
				Name = "TestScanner",
				Term = "Omg!"
			});
		}

		public void Dispose()
		{
			_listener.Dispose();

			_view.StartClicked -= OnStartClicked;
			_view.StopClicked -= OnStopClicked;
		}

		private class ScannerPulse
		{
			public string Name { get; set; }
			public int Count { get; set; }
		}

		private class ScannerControl
		{
			public string Name { get; set; }
			public string Term { get; set; }
		}
	}
}
