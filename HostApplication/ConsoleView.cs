using System;
using System.Windows.Forms;

namespace HostApplication
{
	public class ConsoleView : IMainView
	{
		public event EventAction StartClicked;
		public event EventAction StopClicked;

		public void UpdateCounter(int value)
		{
			Console.WriteLine(value);
		}

		public void Start()
		{
			StartClicked();
		}

		public void Stop()
		{
			StopClicked();
		}
	}
}
