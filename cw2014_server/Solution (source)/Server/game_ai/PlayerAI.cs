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
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Windwardopolis2Library;
using Windwardopolis2Library.ai_interface;
using Windwardopolis2Library.map;
using Windwardopolis2Library.units;

namespace Windwardopolis2.game_ai
{
	/// <summary>
	///     A very simplistic implementation of the AI. This AI is used for additional players if we don't have 10 remote
	///     players.
	/// </summary>
	public class PlayerAI : IPlayerAI
	{
		private Thread aiThread;
		private AiWorker aiWorker;
		private Player me;
		private List<Passenger> passengers;

		private static readonly Random rand = new Random();

		/// <summary>
		///     The GUID for this player's connection. This will change if the connection has to be re-established. It is
		///     null for the local AIs.
		/// </summary>
		public string TcpGuid
		{
			get { return null; }
			set
			{
				/* nada */
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (aiThread == null)
				return;
			aiWorker.ExitThread = true;
			aiWorker.EventThread.Set();
			aiThread.Join(100);
			if (aiThread.IsAlive)
			{
				Trap.trap();
				aiThread.Abort();
			}
			aiThread = null;
		}

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
		public void Setup(GameMap map, Player me, List<Player> players, List<Company> companies, List<CoffeeStore> stores, List<Passenger> passengers,
							PlayerAIBase.PlayerOrdersEvent ordersEvent, PlayerAIBase.PlayerPowerupsEvent sendPowerup)
		{
			this.me = me;
			this.passengers = passengers;
			ConfigurationManager.RefreshSection("appSettings");
			string strPlay = ConfigurationManager.AppSettings["ai-powerups"];
			bool playPowerUps = strPlay == null || strPlay != "off";


			// start the thread
			aiWorker = new AiWorker(map, me, playPowerUps, players, companies, stores, passengers, ordersEvent, sendPowerup);
			aiThread = new Thread(aiWorker.MainLoop) {Name = me.Name.Replace(' ', '_'), IsBackground = true};
			aiThread.Start();

			// cause it to pick a passenger to pick up and calculate the path to that company.
			aiWorker.AddMessage(PlayerAIBase.STATUS.NO_PATH, me, AllPickups(me, passengers));
		}

		/// <summary>
		///     Call the AI with a status message.
		/// </summary>
		/// <param name="xmlMessage">Can be null. The remote AI XML for a message without the status or my player values set.</param>
		/// <param name="about">The player this status is about. Will be set to receiving user for status not specific to a player.</param>
		/// <param name="status">The status for this message.</param>
		/// <param name="players">All the players.</param>
		/// <param name="passengers">All the passengers.</param>
		public void GameStatus(XDocument xmlMessage, Player about, PlayerAIBase.STATUS status, List<Player> players,
			List<Passenger> passengers)
		{
			// only care if for me
			if ((about != null) && (about.Guid != aiWorker.MyPlayerGuid))
				return;

			Player player = players.Find(pl => pl.Guid == aiWorker.MyPlayerGuid);
			Trap.trap(about == null);
			if (about == null)
				about = player;
			aiWorker.AddMessage(status, about, AllPickups(player, passengers));
		}

		public void GameStatus(Player player, PlayerAIBase.STATUS status, PowerUp powerup)
		{
			// redo the path if we got relocated
			if ((status == PlayerAIBase.STATUS.POWER_UP_PLAYED) && ((powerup.Card == PowerUp.CARD.RELOCATE_ALL_CARS) || 
										((powerup.Card == PowerUp.CARD.CHANGE_DESTINATION) && powerup.Player.Guid == me.Guid)))
				aiWorker.AddMessage(PlayerAIBase.STATUS.NO_PATH, me, AllPickups(me, passengers));
		}

		/// <summary>
		/// Post an order to the engine. This can be called from a thread other than the UI thread.
		/// </summary>
		/// <param name="myPlayerGuid">The player this order is for.</param>
		/// <param name="path">The new path for this player's limo. Count == 0 for no path change.</param>
		/// <param name="pickUp">The new pick-up list for this player's limo. Count == 0 for no pick-up change.</param>
		public void PostOrder(string myPlayerGuid, List<Point> path, List<Passenger> pickUp)
		{
			// we don't use this - remote AIs only
		}

		/// <summary>
		///     List of all passengers we can pick up. Does not include ones already delivered and if presently carrying one,
		///     does not include that one. Returned in random order, different random order each time called.
		/// </summary>
		private static List<Passenger> AllPickups(Player me, IEnumerable<Passenger> passengers)
		{
			List<Passenger> pickup = new List<Passenger>();
			pickup.AddRange(passengers.Where(
				psngr =>
					(!me.PassengersDelivered.Contains(psngr)) && (psngr != me.Passenger) && (psngr.Car == null) &&
					(psngr.Destination != null)).OrderBy(psngr => rand.Next()));
			return pickup;
		}

		#region the AI worker thread

		private class StatusMessage
		{
			public PlayerAIBase.STATUS Status { get; private set; }
			/// <summary>
			/// Where the player's limo is presently located.
			/// </summary>
			public Point LimoTileLocation { get; private set; }
			/// <summary>
			/// The destination bus stop for the passenger in the limo.
			/// </summary>
			public Point PassengerDestBusStop { get; private set; }
			/// <summary>
			/// All passengers at the present bus stop that this player can transport (at this stop, not already transported).
			/// </summary>
			public List<Passenger> Pickup { get; private set; }

			public StatusMessage(PlayerAIBase.STATUS status, Player player, List<Passenger> pickup)
			{
				Status = status;
				LimoTileLocation = player.Limo.Location.TilePosition;
				PassengerDestBusStop = player.Passenger == null ? Point.Empty : player.Passenger.Destination.BusStop;
				Pickup = pickup;
			}
		}

		private class AiWorker
		{
			private readonly GameMap gameMap;

			private readonly Player me;
			private readonly bool playPowerUps;
			private readonly List<PowerUp> powerupsDeck;
			private readonly List<PowerUp> powerupsHand;
			private readonly List<Player> players;
			private readonly List<Company> companies;
			private readonly List<CoffeeStore> stores;
			private readonly List<Passenger> passengers;
			private readonly PlayerAIBase.PlayerOrdersEvent sendOrders;
			private readonly PlayerAIBase.PlayerPowerupsEvent sendPowerup;

			private readonly Queue<StatusMessage> messages;

			public string MyPlayerGuid { get { return me.Guid; } }

			/// <summary>
			///     The event handle to bounce when a message is added or the thread is ended.
			/// </summary>
			public EventWaitHandle EventThread { get; private set; }

			/// <summary>
			///     Set to true when ending this thread. Need to bounce EventThread after setting this.
			/// </summary>
			public bool ExitThread { private get; set; }

			public AiWorker(GameMap gameMap, Player me, bool playPowerUps, IEnumerable<Player> players, IEnumerable<Company> companies, IEnumerable<CoffeeStore> stores,
						IEnumerable<Passenger> passengers, PlayerAIBase.PlayerOrdersEvent sendOrders, PlayerAIBase.PlayerPowerupsEvent sendPowerup)
			{
				this.gameMap = gameMap;
				this.me = me;
				this.playPowerUps = playPowerUps;
				powerupsDeck = me.PowerUpsDeck.OrderBy(cpy => rand.Next()).ToList();
				powerupsHand = new List<PowerUp>();
				this.players = new List<Player>(players);
				this.companies = new List<Company>(companies);
				this.stores = new List<CoffeeStore>(stores);
				this.passengers = new List<Passenger>(passengers);
				this.sendOrders = sendOrders;
				this.sendPowerup = sendPowerup;
				messages = new Queue<StatusMessage>();

				// get it ready to handle events.
				EventThread = new AutoResetEvent(false);
				ExitThread = false;
			}

			public void AddMessage(PlayerAIBase.STATUS status, Player about, List<Passenger> pickup)
			{
				StatusMessage msg = new StatusMessage(status, about, pickup);
				lock (messages)
					messages.Enqueue(msg);
				EventThread.Set();
			}

			public void MainLoop()
			{
				while (EventThread.WaitOne())
				{
					if (ExitThread)
						return;
					try
					{
						Point ptLimoLocation = Point.Empty;
						Point ptDest = Point.Empty;
						List<Passenger> pickup = null;
						// build up a single operation from all messages
						lock (messages)
						{
							while (messages.Count > 0)
							{
								StatusMessage msg = messages.Dequeue();
								if (msg == null)
								{
									Trap.trap();
									continue;
								}

								// we do random powerups here.
								if (playPowerUps && msg.Status == PlayerAIBase.STATUS.UPDATE)
								{
									if ((powerupsHand.Count != 0) && (rand.Next(50) < 47))
										continue;
									int maxInHand = me.MaxPowerUpsInHand;
									// not enough, draw
									if (powerupsHand.Count < maxInHand && powerupsDeck.Count > 0)
									{
										for (int index = 0; index < maxInHand - powerupsHand.Count && powerupsDeck.Count > 0; index++)
										{
											// select a card
											PowerUp pu = powerupsDeck.First();
											powerupsDeck.Remove(pu);
											powerupsHand.Add(pu);
											sendPowerup(MyPlayerGuid, PlayerAIBase.CARD_ACTION.DRAW, pu);
										}
										continue;
									}

									// can we play one?
									PowerUp pu2 = powerupsHand.FirstOrDefault(p => p.OkToPlay);
									if (pu2 == null)
										continue;
									// 10% discard, 90% play
									if (rand.Next(10) == 0)
										sendPowerup(MyPlayerGuid, PlayerAIBase.CARD_ACTION.DISCARD, pu2);
									else
									{
										if (pu2.Card == PowerUp.CARD.MOVE_PASSENGER)
											pu2.Passenger = passengers.OrderBy(c => rand.Next()).First(p => p.Car == null);
										if (pu2.Card == PowerUp.CARD.CHANGE_DESTINATION || pu2.Card == PowerUp.CARD.STOP_CAR)
										{
											IList<Player> plyrsWithPsngrs = players.Where(pl => pl.Guid != MyPlayerGuid && pl.Passenger != null).ToList();
											if (plyrsWithPsngrs.Count == 0)
												continue;
											pu2.Player = plyrsWithPsngrs.OrderBy(c => rand.Next()).First();
											Trap.trap(pu2.Player == null);
										}
										sendPowerup(MyPlayerGuid, PlayerAIBase.CARD_ACTION.PLAY, pu2);
									}
									powerupsHand.Remove(pu2);
									continue;
								}

								ptLimoLocation = msg.LimoTileLocation;

								// handle the passenger
								switch (msg.Status)
								{
									case PlayerAIBase.STATUS.NO_PATH:
									case PlayerAIBase.STATUS.PASSENGER_NO_ACTION:
										if (msg.PassengerDestBusStop == Point.Empty)
										{
											pickup = msg.Pickup;
											if (pickup.Count == 0)
												break;
											ptDest = pickup[0].Lobby.BusStop;
										}
										else
											ptDest = msg.PassengerDestBusStop;
										break;
									case PlayerAIBase.STATUS.PASSENGER_DELIVERED:
									case PlayerAIBase.STATUS.PASSENGER_ABANDONED:
										pickup = msg.Pickup;
										if (pickup.Count == 0)
											break;
										ptDest = pickup[0].Lobby.BusStop;
										break;

									case PlayerAIBase.STATUS.PASSENGER_REFUSED_ENEMY:
										ptDest =
											companies.Where(cpy => cpy.BusStop != msg.PassengerDestBusStop).OrderBy(cpy => rand.Next()).First().BusStop;
										break;
									case PlayerAIBase.STATUS.PASSENGER_DELIVERED_AND_PICKED_UP:
									case PlayerAIBase.STATUS.PASSENGER_PICKED_UP:
										pickup = msg.Pickup;
										if (pickup.Count == 0)
											break;
										ptDest = msg.PassengerDestBusStop;
										break;
								}

								// coffee store override
								switch (msg.Status)
								{
									case PlayerAIBase.STATUS.PASSENGER_DELIVERED_AND_PICKED_UP:
									case PlayerAIBase.STATUS.PASSENGER_DELIVERED:
									case PlayerAIBase.STATUS.PASSENGER_ABANDONED:
										if (me.Limo.CoffeeServings <= 0)
										{
											ptDest = stores.OrderBy(st => rand.Next()).First().BusStop;
											pickup = null;
										}
										break;
									case PlayerAIBase.STATUS.PASSENGER_REFUSED_NO_COFFEE:
									case PlayerAIBase.STATUS.PASSENGER_DELIVERED_AND_PICK_UP_REFUSED:
										ptDest = stores.OrderBy(st => rand.Next()).First().BusStop;
										break;
									case PlayerAIBase.STATUS.COFFEE_STORE_CAR_RESTOCKED:
										pickup = msg.Pickup;
										if (pickup.Count == 0)
											break;
										ptDest = pickup[0].Lobby.BusStop;
										break;
								}
							}
						}

						if (ptDest == Point.Empty)
							continue;
						if (pickup == null)
							pickup = new List<Passenger>();

						// get the path from where we are to the dest.
						List<Point> path = CalculatePathPlus1(ptLimoLocation, ptDest);

						sendOrders(MyPlayerGuid, path, pickup);
					}
					catch (Exception)
					{
						Trap.trap();
						// next message
					}
				}
			}

			private List<Point> CalculatePathPlus1(Point ptLimo, Point ptDest)
			{
				List<Point> path = SimpleAStar.CalculatePath(gameMap, ptLimo, ptDest);
				// add in leaving the bus stop so it has orders while we get the message saying it got there and are deciding what to do next.
				if (path.Count > 1)
					path.Add(path[path.Count - 2]);
				return path;
			}
		}

		#endregion
	}
}