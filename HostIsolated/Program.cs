using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Messaging;

namespace HostIsolated
{
	class Program
	{
		static void Main(string[] args)
		{
			//var path = @"D:\dev\projects\InProcessMicroServices\HostIsolated\bin\Debug\plugins\LongRunning\LongRunning.dll";
			var path = @"plugins\LongRunning\LongRunning.dll";

			Console.WriteLine("Attempting to setup an AppDomain:");

			BuildDomainFor(path);

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static void BuildDomainFor(string assemblyPath)
		{
			var name = Path.GetFileNameWithoutExtension(assemblyPath);
			var binPath = Path.GetDirectoryName(assemblyPath);

			var setup = new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
				PrivateBinPath = binPath
			};

			var domain = AppDomain.CreateDomain("Plugin." + name, AppDomain.CurrentDomain.Evidence, setup);

			var hostType = typeof(PluginHost);
			var host = (PluginHost)domain.CreateInstanceAndUnwrap(hostType.Assembly.FullName, hostType.FullName);

			//var connector = new MemoryConnector();
			host.Load(assemblyPath);
			host.Initialise();

			host.SendMessage("Notifications", "Scanner.Start");
			//connector.SubscribeTo<ScannerPulse>("Notifications", "Scanner.Pulse", m =>
			//{
			//	Console.WriteLine(m.Count);
			//});

			//connector.Publish("Notifications", "Scanner.Start", new
			//{
			//	Name = "TestScanner",
			//	Term = "Omg!"
			//});

		}
	}

	public class ScannerPulse
	{
		public int Count { get; set; }
	}
}
