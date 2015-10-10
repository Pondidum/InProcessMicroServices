using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Messaging.Memory;
using LongRunning;

namespace HostApplication
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var connector = new MemoryConnector();
			var view = new ConsoleView();

			var longRunning = new LongRunningComponent();
			longRunning.Initialise(connector);

			using (var presenter = new MainPresenter(view, connector))
			{
				view.Start();
				Console.ReadKey();
			}
		}
	}
}
