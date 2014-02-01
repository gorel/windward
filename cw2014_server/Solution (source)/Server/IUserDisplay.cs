/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Windwardopolis2
{
	/// <summary>
	///     The engine calls the main window using this interface.
	/// </summary>
	public interface IUserDisplay
	{
		/// <summary>
		///     Called when starting a game.
		/// </summary>
		void SetupGame();

		/// <summary>
		///     New player added to the game
		/// </summary>
		void NewPlayerAdded();

		/// <summary>
		///     Called to update the map window.
		/// </summary>
		void UpdateMap();

		/// <summary>
		///     Now has a new map.
		/// </summary>
		void NewMap();

		/// <summary>
		///     Called to re-render all locations with vehicles.
		/// </summary>
		/// <param name="limoLocations">Where the limos are presently located.</param>
		void RenderMapChanges(List<Point> limoLocations);

		/// <summary>
		///     Called to update (re-draw) the player status windows.
		/// </summary>
		void UpdatePlayers();

		/// <summary>
		///     Called to update (re-draw) the debug windows.
		/// </summary>
		void UpdateDebug();

		/// <summary>
		///     Called to delete all player status windows the player status windows.
		/// </summary>
		void ResetPlayers();

		/// <summary>
		///     Called to update the main window menu.
		/// </summary>
		void UpdateMenu();

		/// <summary>
		///     Adds a message to the status window.
		/// </summary>
		/// <param name="message">The message to add.</param>
		void StatusMessage(string message);

		/// <summary>
		///     Called each time the turn or phase number increases. Displays the numbers in the window.
		/// </summary>
		/// <param name="turn">The turn number.</param>
		void TurnNumber(int turn);

		/// <summary>
		///     Used for Invoke when we get TCP callbacks.
		/// </summary>
		Control CtrlForInvoke { get; }

		void Exit();
	}

	/// <summary>
	///     This class represents a laser beam being fired.
	/// </summary>
	internal class LaserBeam
	{
		public LaserBeam(Point positionStart, Point positionEnd, bool laser)
		{
			PositionStart = positionStart;
			PositionEnd = positionEnd;
			Laser = laser;
		}

		/// <summary>
		///     The square the laser beam starts in.
		/// </summary>
		public Point PositionStart { get; private set; }

		/// <summary>
		///     The square the laser beam ends in.
		/// </summary>
		public Point PositionEnd { get; private set; }

		/// <summary>
		///     The laser that is firing.
		/// </summary>
		public bool Laser { get; private set; }
	}
}