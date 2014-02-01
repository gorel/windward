/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

namespace Windwardopolis2
{
	public interface IEngineCallback
	{
		/// <summary>
		///     Adds a message to the status window.
		/// </summary>
		/// <param name="message">The message to add.</param>
		void StatusMessage(string message);

		void ConnectionEstablished(string guid);

		void IncomingMessage(string guid, string message);

		void ConnectionLost(string guid);
	}
}