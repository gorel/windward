/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" 
 * As long as you retain this notice you can do whatever you want with this 
 * stuff. If you meet an employee from Windward meet some day, and you think
 * this stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using Windwardopolis2Library.map;
using Windwardopolis2Library.units;

namespace Windwardopolis2Library.ai_interface
{
	public interface IPlayerAI : IDisposable
	{
		/// <summary>
		///     The GUID for this player's connection. This will change if the connection has to be re-established. It is
		///     null for the local AIs.
		/// </summary>
		string TcpGuid { get; set; }

		/// <summary>
		/// Called when the game starts, providing all info.
		/// </summary>
		/// <param name="map">The game map.</param>
		/// <param name="me">The player being setup.</param>
		/// <param name="players">All the players.</param>
		/// <param name="companies">All companies on the board.</param>
		/// <param name="stores">All coffee stores.</param>
		/// <param name="passengers">All the passengers.</param>
		/// <param name="ordersEvent">Callback to pass orders to the engine.</param>
		/// <param name="sendPowerup">Callback to send powerup plays to the engine.</param>
		void Setup(GameMap map, Player me, List<Player> players, List<Company> companies,
						List<CoffeeStore> stores, List<Passenger> passengers, PlayerAIBase.PlayerOrdersEvent ordersEvent,
						PlayerAIBase.PlayerPowerupsEvent sendPowerup);

		/// <summary>
		///     Call the AI with a status message.
		/// </summary>
		/// <param name="xmlMessage">Can be null. The remote AI XML for a message without the status or my player values set.</param>
		/// <param name="about">The player this status is about. Will be set to receiving user for status not specific to a player.</param>
		/// <param name="status">The status for this message.</param>
		/// <param name="players">All the players.</param>
		/// <param name="passengers">All the passengers.</param>
		void GameStatus(XDocument xmlMessage, Player about, PlayerAIBase.STATUS status, List<Player> players,
			List<Passenger> passengers);

		/// <summary>
		///     Post an order to the engine. This can be called from a thread other than the UI thread.
		/// </summary>
		/// <param name="myPlayerGuid">The player this order is for.</param>
		/// <param name="path">The new path for this player's limo. Count == 0 for no path change.</param>
		/// <param name="pickUp">The new pick-up list for this player's limo. Count == 0 for no pick-up change.</param>
		void PostOrder(string myPlayerGuid, List<Point> path, List<Passenger> pickUp);

		/// <summary>
		/// Status of playing a power-up
		/// </summary>
		/// <param name="player">The player impacted. null if several/all.</param>
		/// <param name="status">The status for this message.</param>
		/// <param name="powerUp">The powerup played.</param>
		void GameStatus(Player player, PlayerAIBase.STATUS status, PowerUp powerUp);
	}

	public class PlayerAIBase
	{
		public delegate void PlayerOrdersEvent(string playerGuid, List<Point> path, List<Passenger> pickUp);

		public enum CARD_ACTION
		{
			DRAW,
			DISCARD,
			PLAY
		}

		/// <summary>
		/// Send the engine a power-up card command.
		/// </summary>
		/// <param name="playerGuid">the player sending the message.</param>
		/// <param name="action">The action to take with the card.</param>
		/// <param name="card">The card to perform the action on.</param>
		public delegate void PlayerPowerupsEvent(string playerGuid, CARD_ACTION action, PowerUp card);

		public enum STATUS
		{
			/// <summary>
			///     Called every N ticks to update the AI with the game status.
			/// </summary>
			UPDATE,

			/// <summary>
			///     The car has no path.
			/// </summary>
			NO_PATH,

			/// <summary>
			///     The passenger was abandoned, no passenger was picked up.
			/// </summary>
			PASSENGER_ABANDONED,

			/// <summary>
			///     The passenger was delivered, no passenger was picked up.
			/// </summary>
			PASSENGER_DELIVERED,

			/// <summary>
			///     The passenger was delivered or abandoned, a new passenger was picked up.
			/// </summary>
			PASSENGER_DELIVERED_AND_PICKED_UP,

			/// <summary>
			///     The passenger refused to exit at the bus stop because an enemy was there.
			/// </summary>
			PASSENGER_REFUSED_ENEMY,

			/// <summary>
			///     A passenger was picked up. There was no passenger to deliver.
			/// </summary>
			PASSENGER_PICKED_UP,

			/// <summary>
			///     At a bus stop, nothing happened (no drop off, no pick up).
			/// </summary>
			PASSENGER_NO_ACTION,

			/// <summary>
			/// Coffee stop did not stock up car. You cannot stock up when you have a passenger.
			/// </summary>
			COFFEE_STORE_NO_STOCK_UP,

			/// <summary>
			/// Coffee stop stocked up car.
			/// </summary>
			COFFEE_STORE_CAR_RESTOCKED,

			/// <summary>
			/// The passenger refused to board due to lack of coffee.
			/// </summary>
			PASSENGER_REFUSED_NO_COFFEE,

			/// <summary>
			/// The passenger was delivered or abandoned, the new passenger refused to board due to lack of coffee.
			/// </summary>
			PASSENGER_DELIVERED_AND_PICK_UP_REFUSED,

			/// <summary>
			/// A draw request was refused as too many powerups are already in hand.
			/// </summary>
			POWER_UP_DRAW_TOO_MANY,

			/// <summary>
			/// A play request for a card not in hand.
			/// </summary>
			POWER_UP_PLAY_NOT_EXIST,

			/// <summary>
			/// A play request for a card drawn but haven't visited a stop yet.
			/// </summary>
			POWER_UP_PLAY_NOT_READY,

			/// <summary>
			/// It's illegal to play this card at this time.
			/// </summary>
			POWER_UP_ILLEGAL_TO_PLAY,

			/// <summary>
			/// The power up was played. For one that impacts a transit, the passenger is delivered.
			/// </summary>
			POWER_UP_PLAYED,

			/// <summary>
			/// The number of power-ups in the hand were too many. Randome one(s) discarded to reduce to the correct amount.
			/// </summary>
			POWER_UP_HAND_TOO_MANY,
		}
	}
}