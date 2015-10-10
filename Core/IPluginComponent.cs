using Core.Messaging;

namespace Core
{
	public interface IPluginComponent
	{
		void Initialise(IQueueConnector connector);
	}
}
