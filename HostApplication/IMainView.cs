using System.Windows.Forms;

namespace HostApplication
{
	public interface IMainView
	{
		event EventAction StartClicked;
		event EventAction StopClicked;

		void UpdateCounter(int value);
	}
}
