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
using System.Linq;
using System.Xml.Linq;
using Windwardopolis2Library.ai_interface;
using Windwardopolis2Library.map;

namespace Windwardopolis2Library.units
{
	/// <summary>
	///     Adds engine items to the player object.
	/// </summary>
	public class Player : IDisposable
	{
		/// <summary>
		///     How many passengers must be delivered to win.
		/// </summary>
		public const int NUM_PASSENGERS_TO_WIN = 8;

		private readonly IPlayerAI ai;

		private float passengerDeliveredPoints;

		/// <summary>
		/// List of the power-ups we need to display for 5 seconds.
		/// </summary>
		private readonly List<PowerUpIcon> displayIcons; 

		/// <summary>
		///     What the communication with the remote AI player mode is.
		/// </summary>
		public enum COMM_MODE
		{
			/// <summary>
			///     Waiting for initial start move.
			/// </summary>
			WAITING_FOR_START,

			/// <summary>
			///     Got the start move.
			/// </summary>
			RECEIVED_START,
		}

		/// <summary>
		///     Create a player object. This is used during setup.
		/// </summary>
		/// <param name="guid">The unique identifier for this player.</param>
		/// <param name="name">The name of the player.</param>
		/// <param name="school">The school this player is from.</param>
		/// <param name="language">The computer language this A.I. was written in.</param>
		/// <param name="avatar">The avatar of the player.</param>
		/// <param name="limo">The limo for this player.</param>
		/// <param name="spriteColor">The color of this player's sprite.</param>
		/// <param name="ai">The AI for this player.</param>
		public Player(string guid, string name, string school, string language, Image avatar, Limo limo, Color spriteColor,
			IPlayerAI ai)
		{
			Guid = guid;
			Name = name;
			School = school.Length <= 11 ? school : school.Substring(0, 11);
			Language = language;
			Avatar = avatar;
			Limo = limo;
			passengerDeliveredPoints = 0;
			SpriteColor = spriteColor;
			TransparentSpriteColor = Color.FromArgb(96, spriteColor.R, spriteColor.G, spriteColor.B);
			this.ai = ai;
			IsConnected = true;
			displayIcons = new List<PowerUpIcon>();

			PassengersDelivered = new List<Passenger>();
			PickUp = new List<Passenger>();
			Scoreboard = new List<float>();

			PowerUpsDeck = new List<PowerUp>();
			PowerUpsDrawn = new List<PowerUp>();
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			ai.Dispose();
		}

		public void Reset()
		{
			// limos back to start, passengers reset (location and destinations)?
			passengerDeliveredPoints = 0;
			PassengersDelivered.Clear();
			PickUp.Clear();
			Limo.Reset();
			Limo.CoffeeServings = 3;
			Passenger = null;
			WaitingForReply = COMM_MODE.WAITING_FOR_START;
			TicksToFullSpeed = 0;
			displayIcons.Clear();
		}

		/// <summary>
		///     The GUID for this player's connection. This will change if the connection has to be re-established. It is
		///     null for the local AIs.
		/// </summary>
		public string TcpGuid
		{
			get { return ai.TcpGuid; }
			set
			{
				IsConnected = value != null;
				ai.TcpGuid = value;
			}
		}

		/// <summary>
		///     true if connected.
		/// </summary>
		public bool IsConnected { get; private set; }

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
		/// <param name="sendPowerup">Callback to send powerup card plays.</param>
		public void Setup(GameMap map, Player me, List<Player> players, List<Company> companies,
						List<CoffeeStore> stores, List<Passenger> passengers, PlayerAIBase.PlayerOrdersEvent ordersEvent,
						PlayerAIBase.PlayerPowerupsEvent sendPowerup)
		{
			ai.Setup(map, me, players, companies, stores, passengers, ordersEvent, sendPowerup);
		}

		/// <summary>
		///     Call the AI with a status message.
		/// </summary>
		/// <param name="xmlMessage">Can be null. The remote AI XML for a message without the status or my player values set.</param>
		/// <param name="about">The player this is about.</param>
		/// <param name="status">The status for this message.</param>
		/// <param name="players">All the players.</param>
		/// <param name="passengers">All the passengers.</param>
		public void GameStatus(XDocument xmlMessage, Player about, PlayerAIBase.STATUS status, List<Player> players,
			List<Passenger> passengers)
		{
			ai.GameStatus(xmlMessage, about, status, players, passengers);
		}

		/// <summary>
		///     Post an order to the engine. This can be called from a thread other than the UI thread.
		/// </summary>
		/// <param name="path">The new path for this player's limo. Count == 0 for no path change.</param>
		/// <param name="pickUp">The new pick-up list for this player's limo. Count == 0 for no pick-up change.</param>
		public void SendOrder(List<Point> path, List<Passenger> pickUp)
		{
			ai.PostOrder(Guid, path, pickUp);
		}

		/// <summary>
		/// Status of playing a power-up
		/// </summary>
		/// <param name="player">The player impacted. null if several/all.</param>
		/// <param name="status">The status for this message.</param>
		/// <param name="powerup">The power-up card this is about</param>
		public void GameStatus(Player player, PlayerAIBase.STATUS status, PowerUp powerup)
		{
			ai.GameStatus(player, status, powerup);
		}

		#region properties

		/// <summary>
		///     The unique identifier for this player. This will remain constant for the length of the game (while the Player
		///     objects passed will
		///     change on every call).
		/// </summary>
		public string Guid { get; private set; }

		/// <summary>
		///     The name of the player.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     The avatar of the player.
		/// </summary>
		public Image Avatar { get; private set; }

		/// <summary>
		///     The player's limo.
		/// </summary>
		public Limo Limo { get; private set; }

		/// <summary>
		///     The school this player is from.
		/// </summary>
		public string School { get; private set; }

		/// <summary>
		///     The computer language this A.I. was written in.
		/// </summary>
		public string Language { get; private set; }

		/// <summary>
		///     We are waiting for a reply from this player.
		/// </summary>
		public COMM_MODE WaitingForReply { get; set; }

		/// <summary>
		///     The score for this player - this game
		/// </summary>
		public float Score
		{
			get
			{
				if (PassengersDelivered.Count >= NUM_PASSENGERS_TO_WIN)
					return passengerDeliveredPoints + 2;
				if (Passenger == null)
					return passengerDeliveredPoints;
				int distTotal = Math.Abs(Passenger.Destination.BusStop.X - Passenger.Start.BusStop.X) +
				                Math.Abs(Passenger.Destination.BusStop.Y - Passenger.Start.BusStop.Y);
				int distRemaining = Math.Abs(Passenger.Destination.BusStop.X - Limo.Location.TilePosition.X) +
				                    Math.Abs(Passenger.Destination.BusStop.Y - Limo.Location.TilePosition.Y);
				if (distRemaining > distTotal)
					return passengerDeliveredPoints;
				// all the way is 1/2 point
				return passengerDeliveredPoints + 0.5f*(distTotal - distRemaining)/distTotal;
			}
		}

		/// <summary>
		/// true if this player is the winner.
		/// </summary>
		public bool isWinner
		{
			get
			{
				return PassengersDelivered.Count >= NUM_PASSENGERS_TO_WIN;
			}
		}

		/// <summary>
		///     The passengers delivered - this game
		/// </summary>
		public List<Passenger> PassengersDelivered { get; private set; }

		/// <summary>
		///     The score for this player - previous games.
		/// </summary>
		public List<float> Scoreboard { get; private set; }

		/// <summary>
		///     The color for this player on the status window.
		/// </summary>
		public Color SpriteColor { get; private set; }

		/// <summary>
		///     The color for this player on the status window.
		/// </summary>
		public Color TransparentSpriteColor { get; private set; }

		/// <summary>
		///     Passenger in limo. null if none.
		/// </summary>
		public Passenger Passenger { get; set; }

		/// <summary>
		///     The next bus stop in this player's path. null if none.
		/// </summary>
		public Company NextBusStop { get; set; }

		/// <summary>
		///     Who to pick up at the next bus stop. Can be empty and can also only list people not there.
		/// </summary>
		public List<Passenger> PickUp { get; private set; }


		/// <summary>
		/// The powerups this player can draw.
		/// </summary>
		public List<PowerUp> PowerUpsDeck { get; private set; }

		/// <summary>
		/// The powerups this player has drawn (may have to wait before playing it).
		/// </summary>
		public List<PowerUp> PowerUpsDrawn { get; private set; }

		/// <summary>
		/// The powerup presently in use by this player. This is set for a very short period, from when the engine receives the card played
		/// until the next tick when it processes it, and then discards it.
		/// </summary>
		public PowerUp PowerUpInAction { get; set; }

		/// <summary>
		/// The powerup to play at the next bus stop.
		/// </summary>
		public PowerUp PowerUpNextBusStop { get; set; }

		/// <summary>
		/// The powerup in effect for the transit presently on.
		/// </summary>
		public PowerUp PowerUpThisTransit { get; set; }

		/// <summary>
		/// The maximum number of power-ups this player can hold.
		/// </summary>
		public int MaxPowerUpsInHand
		{
			get
			{
				float score = Score;
				if (score < 0.5)
					return 4;
				if (score < 6)
					return 3;
				if (score < 8.5)
					return 2;
				return 1;
			}
		}

		/// <summary>
		/// When this player's car is set to 1/4 speed, this is the number of ticks before they come back to full speed.
		/// </summary>
		public int TicksToFullSpeed { get; set; }

		#endregion

		/// <summary>
		///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		/// </returns>
		public override string ToString()
		{
			return String.Format("{0}, Score: {1}", Name, Score);
		}

		public void Delivered(Passenger passenger)
		{
			passengerDeliveredPoints += passenger.PointsDelivered;
			PassengersDelivered.Add(passenger);
		}

		/// <summary>
		/// Add in extra points (for multiplier deliveries).
		/// </summary>
		/// <param name="points">the points to add.</param>
		public void AddPoints(float points)
		{
			passengerDeliveredPoints += points;
		}

		/// <summary>
		/// Add a powerup that is displayed for 5 seconds in this player's status.
		/// </summary>
		/// <param name="pu">The power-up to display.</param>
		/// <param name="ticks">Number of ticks to display this for.</param>
		public void AddToDisplay(PowerUp pu, int ticks)
		{
			displayIcons.RemoveAll(p => p.powerup.Card == pu.Card);
			displayIcons.Add(new PowerUpIcon(pu, ticks));
		}

		/// <summary>
		/// One tick occured. Use this to count down the power-up's to display.
		/// </summary>
		public void Tick()
		{
			if (displayIcons.Count == 0)
				return;
			foreach (var pui in displayIcons)
				pui.ticks --;
			displayIcons.RemoveAll(p => p.ticks <= 0);
		}

		/// <summary>
		/// The cards that need to be displayed.
		/// </summary>
		public List<PowerUp.CARD> DisplayCards
		{
			get
			{
				return displayIcons.Select(diOn => diOn.powerup.Card).ToList();
			}
		}

		private class PowerUpIcon
		{
			internal readonly PowerUp powerup;
			internal int ticks;

			public PowerUpIcon(PowerUp powerup, int ticks)
			{
				this.powerup = powerup;
				this.ticks = ticks;
			}
		}
	}
}