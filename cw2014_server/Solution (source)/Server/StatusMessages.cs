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
	public partial class StatusMessages : Form
	{
		public StatusMessages()
		{
			InitializeComponent();
		}

		public void AddMessage(string message)
		{
			listBoxStatus.Items.Add(message);
			listBoxStatus.TopIndex = listBoxStatus.Items.Count - 1;
		}

		private void StatusMessages_Load(object sender, EventArgs e)
		{
			MainWindow.RestoreWindow(this, "status");
		}
	}
}