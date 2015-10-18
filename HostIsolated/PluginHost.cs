using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using Core.Messaging.Memory;

namespace HostIsolated
{
	[Serializable]
	public class PluginHost : MarshalByRefObject
	{
		private readonly List<IPluginComponent> _plugins;
		private readonly MemoryConnector _connector;

		public PluginHost()
		{
			_plugins = new List<IPluginComponent>();
			_connector = new MemoryConnector();
		}

		public void Load(string assemblyPath)
		{
			var full = Path.GetFullPath(assemblyPath);
			var name = AssemblyName.GetAssemblyName(full);

			AppDomain.CurrentDomain.Load(name);
		}

		public void Initialise()
		{
			var types = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.SelectMany(asm => asm.GetTypes())
				.Where(type => type.GetInterfaces().Contains(typeof(IPluginComponent)))
				.ToList();

			var plugins = types
				.Select(type => type.GetConstructor(Type.EmptyTypes))
				.Where(ctor => ctor != null)
				.Select(ctor => ctor.Invoke(null))
				.Cast<IPluginComponent>()
				.ToList();

			plugins.ForEach(p => p.Initialise(_connector));

			_plugins.AddRange(plugins);
		}

		public void SendMessage(string queueName, string routingKey)
		{
			_connector.Publish(queueName, routingKey, new object());
		}
	}
}
