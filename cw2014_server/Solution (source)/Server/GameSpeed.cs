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
	public partial class GameSpeed : Form
	{
		public GameSpeed(int movesPerSecond)
		{
			InitializeComponent();
			spinMovesPerSec.Value = Math.Min(1000, movesPerSecond);
		}

		public int MovesPerSecond
		{
			get { return (int) spinMovesPerSec.Value; }
		}
	}
}