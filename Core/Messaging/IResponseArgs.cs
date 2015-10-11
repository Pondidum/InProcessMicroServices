namespace Core.Messaging
{
	public interface IResponseArgs
	{
		bool CanRespond();
		void RespondWith(object message);
	}
}