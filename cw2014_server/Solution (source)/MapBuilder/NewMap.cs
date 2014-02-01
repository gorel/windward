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

namespace MapBuilder
{
	public partial class NewMap : Form
	{
		public NewMap()
		{
			InitializeComponent();
			textBoxSize_TextChanged(null, null);
		}

		public int MapHeight
		{
			get { return int.Parse(textBoxHeight.Text); }
		}

		public int MapWidth
		{
			get { return int.Parse(textBoxWidth.Text); }
		}

		private void textBoxSize_TextChanged(object sender, EventArgs e)
		{
			bool enabled = true;
			int num;
			if (!int.TryParse(textBoxHeight.Text, out num))
				enabled = false;
			else if (!int.TryParse(textBoxWidth.Text, out num))
				enabled = false;
			btnOk.Enabled = enabled;
		}
	}
}