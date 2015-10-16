using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Core.Messaging;
using Core.Messaging.Memory;

namespace HostIsolated
{
	class Program
	{
		static void Main(string[] args)
		{
			var path = @"D:\dev\projects\InProcessMicroServices\HostIsolated\bin\Debug\plugins\LongRunning\LongRunning.dll";

			Console.WriteLine("Attempting to setup an AppDomain:");

			BuildDomainFor(path);

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static void BuildDomainFor(string assemblyPath)
		{
			var name = Path.GetFileNameWithoutExtension(assemblyPath);
			var dirPath = Path.GetFullPath(Path.GetDirectoryName(assemblyPath));

			var setup = new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
				PrivateBinPath = dirPath,
				//ShadowCopyFiles = "true",
				//ShadowCopyDirectories = dirPath,
			};

			var domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, setup);

			var hostType = typeof(PluginHost);
			var host = (PluginHost)domain.CreateInstanceAndUnwrap(hostType.Assembly.FullName, hostType.FullName);

			var connector = new MemoryConnector();
			host.Initialise(connector);

			connector.SubscribeTo<ScannerPulse>("Notifications", "Scanner.Pulse", m =>
			{
				Console.WriteLine(m.Count);
			});

			connector.Publish("Notifications", "Scanner.Start", new
			{
				Name = "TestScanner",
				Term = "Omg!"
			});

		}
	}

	public class ScannerPulse
	{
		public int Count { get; set; }
	}

	[Serializable]
	public class PluginHost : MarshalByRefObject
	{
		private readonly List<IPluginComponent> _plugins;

		public PluginHost()
		{
			var pluginType = typeof(IPluginComponent);

			_plugins = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.GetInterface(pluginType.Name) != null)
				.Select(t => t.GetConstructor(Type.EmptyTypes))
				.Where(c => c != null)
				.Select(c => c.Invoke(null))
				.Cast<IPluginComponent>()
				.ToList();
		}

		public void Initialise(IQueueConnector connector)
		{
			_plugins.ForEach(p => p.Initialise(connector));
		}
	}
}
