/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System.Collections.Generic;
using Windwardopolis2Library.map;
using Windwardopolis2Library.units;

namespace Windwardopolis2Library
{
	public interface IMapInfo
	{
		/// <summary>
		///     The game map.
		/// </summary>
		GameMap Map { get; }

		/// <summary>
		///     The Player's Limos to display on the map.
		/// </summary>
		List<Player> Players { get; }

		/// <summary>
		///     The number of pixels per tile. This will range from 48 to 6.
		/// </summary>
		int PixelsPerTile { get; }
	}
}