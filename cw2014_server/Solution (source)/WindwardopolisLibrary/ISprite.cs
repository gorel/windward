/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System.Drawing;

namespace Windwardopolis2Library
{
	public interface ISprite
	{
		// return true to kill it
		bool IncreaseTick();

		Image SpriteBitmap { get; }
	}
}