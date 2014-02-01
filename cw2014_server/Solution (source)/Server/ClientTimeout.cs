/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System;
using System.Windows.Forms;

namespace Windwardopolis2
{
	public partial class ClientTimeout : Form
	{
		public ClientTimeout(int movesPerSecond)
		{
			InitializeComponent();
#if DEBUG_MODE
			spinTimeoutSeconds.Maximum = int.MaxValue;
#else
			spinTimeoutSeconds.Maximum = 30;
#endif
			spinTimeoutSeconds.Value = Math.Min(Math.Max(1, movesPerSecond), spinTimeoutSeconds.Maximum);
		}

		public int TimeoutSeconds
		{
			get { return (int) spinTimeoutSeconds.Value; }
		}
	}
}