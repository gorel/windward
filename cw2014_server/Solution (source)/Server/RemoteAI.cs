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
using System.Text;
using System.Xml.Linq;
using log4net;
using Windwardopolis2Library;
using Windwardopolis2Library.ai_interface;
using Windwardopolis2Library.map;
using Windwardopolis2Library.units;

namespace Windwardopolis2
{
	/// <summary>
	///     Local end of communicattion link with remote AI.
	/// </summary>
	public class RemoteAI : IPlayerAI
	{
		private readonly Framework framework;
		private Player me;
		private PlayerAIBase.PlayerOrdersEvent sendOrders;
		private PlayerAIBase.PlayerPowerupsEvent sendPowerup;

		public RemoteAI(Framework framework, string guid)
		{
			this.framework = framework;
			TcpGuid = guid;
		}

		/// <summary>
		///     The GUID for this player's connection. This will change if the connection has to be re-established. It is
		///     null for the local AIs.
		/// </summary>
		public string TcpGuid { get; set; }

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
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
		/// <param name="sendEvent">Callback to send powerup plays to the engine.</param>
		public void Setup(GameMap map, Player me, List<Player> players, List<Company> companies,
						List<CoffeeStore> stores, List<Passenger> passengers, PlayerAIBase.PlayerOrdersEvent ordersEvent,
						PlayerAIBase.PlayerPowerupsEvent sendEvent)
		{
			sendOrders = ordersEvent;
			sendPowerup = sendEvent;
			this.me = me;

			XDocument doc = new XDocument();
			XElement elemRoot = new XElement("setup", new XAttribute("game-start", true), new XAttribute("my-guid", me.Guid));
			doc.Add(elemRoot);

			// the map
			XElement elemMap = new XElement("map", new XAttribute("width", map.Width), new XAttribute("height", map.Height),
				new XAttribute("units-tile", TileMovement.UNITS_PER_TILE));
			elemRoot.Add(elemMap);
			for (int x = 0; x < map.Width; x++)
				for (int y = 0; y < map.Height; y++)
				{
					MapSquare square = map.Squares[x][y];
					XElement elemRow = new XElement("tile", new XAttribute("x", x), new XAttribute("y", y),
						new XAttribute("type", square.Tile.Type));
					if (square.Tile.IsDriveable)
					{
						elemRow.Add(new XAttribute("direction", square.Tile.Direction));
						if (square.StopSigns != MapSquare.STOP_SIGNS.NONE)
							elemRow.Add(new XAttribute("stop-sign", square.StopSigns));
						if (square.SignalDirection != MapSquare.SIGNAL_DIRECTION.NONE)
							elemRow.Add(new XAttribute("signal", true));
					}
					elemMap.Add(elemRow);
				}

			// my powerups
			XElement elemPowerups = new XElement("powerups");
			elemRoot.Add(elemPowerups);
			foreach (PowerUp pwrUpOn in me.PowerUpsDeck)
			{
				XElement elemCard = new XElement("powerup", new XAttribute("name", pwrUpOn.Name), new XAttribute("card", pwrUpOn.Card.ToString()));
				elemPowerups.Add(elemCard);
				if (pwrUpOn.Company != null)
					elemCard.Add(new XAttribute("company", pwrUpOn.Company.Name));
				if (pwrUpOn.Passenger != null)
					elemCard.Add(new XAttribute("passenger", pwrUpOn.Passenger.Name));
			}

			// all players (including me)
			XElement elemPlayers = new XElement("players");
			elemRoot.Add(elemPlayers);
			foreach (Player plyrOn in players)
				elemPlayers.Add(new XElement("player", new XAttribute("guid", plyrOn.Guid),
					new XAttribute("language", plyrOn.Language),
					new XAttribute("school", plyrOn.School),
					new XAttribute("name", plyrOn.Name),
					new XAttribute("limo-x", plyrOn.Limo.Location.TilePosition.X),
					new XAttribute("limo-y", plyrOn.Limo.Location.TilePosition.Y),
					new XAttribute("limo-angle", plyrOn.Limo.Location.Angle)));

			// all coffee stores
			XElement elemCoffeeStores = new XElement("stores");
			elemRoot.Add(elemCoffeeStores);
			foreach (CoffeeStore storeOn in stores)
				elemCoffeeStores.Add(new XElement("store", new XAttribute("name", storeOn.Name),
					new XAttribute("bus-stop-x", storeOn.BusStop.X), new XAttribute("bus-stop-y", storeOn.BusStop.Y)));

			// all companies
			XElement elemCompanies = new XElement("companies");
			elemRoot.Add(elemCompanies);
			foreach (Company cmpnyOn in companies)
				elemCompanies.Add(new XElement("company", new XAttribute("name", cmpnyOn.Name),
					new XAttribute("bus-stop-x", cmpnyOn.BusStop.X), new XAttribute("bus-stop-y", cmpnyOn.BusStop.Y)));

			// all passengers
			XElement elemPassengers = new XElement("passengers");
			elemRoot.Add(elemPassengers);
			foreach (Passenger psngrOn in passengers)
			{
				XElement elemPassenger = new XElement("passenger", new XAttribute("name", psngrOn.Name),
					new XAttribute("points-delivered", psngrOn.PointsDelivered));
				// if due to a re-start, these can be null
				if (psngrOn.Lobby != null)
					elemPassenger.Add(new XAttribute("lobby", psngrOn.Lobby.Name));
				if (psngrOn.Destination != null)
					elemPassenger.Add(new XAttribute("destination", psngrOn.Destination.Name));
				foreach (Company route in psngrOn.Route)
					elemPassenger.Add(new XElement("route", route.Name));
				foreach (Passenger enemy in psngrOn.Enemies)
					elemPassenger.Add(new XElement("enemy", enemy.Name));
				elemPassengers.Add(elemPassenger);
			}

			framework.tcpServer.SendMessage(TcpGuid, doc.ToString());
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
			if (TcpGuid == null)
				return;

			if (xmlMessage == null)
				xmlMessage = BuildMessageXml(players, passengers);
			XAttribute attr = xmlMessage.Root.Attribute("status");
			if (attr == null)
				xmlMessage.Root.Add(new XAttribute("status", status));
			else
				attr.Value = status.ToString();

			attr = xmlMessage.Root.Attribute("player-guid");
			if (attr == null)
				xmlMessage.Root.Add(new XAttribute("player-guid", about.Guid));
			else
				attr.Value = about.Guid;

			StringBuilder buf = new StringBuilder();
			Player player = framework.GameEngine.Players.Find(pl => pl.TcpGuid == TcpGuid);
			if (player != null)
				foreach (Point ptOn in player.Limo.PathTiles)
					buf.Append(Convert.ToString(ptOn.X) + ',' + Convert.ToString(ptOn.Y) + ';');

			XElement elem = xmlMessage.Root.Element("path");
			if (elem == null)
			{
				elem = new XElement("path");
				xmlMessage.Root.Add(elem);
				elem.Value = buf.ToString();
			}
			else
				elem.Value = buf.ToString();

			if (player != null)
			{
				buf.Clear();
				foreach (Passenger psngr in player.PickUp)
					buf.Append(psngr.Name + ';');
			}
			elem = xmlMessage.Root.Element("pick-up");
			if (elem == null)
			{
				elem = new XElement("pick-up");
				xmlMessage.Root.Add(elem);
				elem.Value = buf.ToString();
			}
			else
				elem.Value = buf.ToString();

			framework.tcpServer.SendMessage(TcpGuid, xmlMessage.ToString());
		}

		/// <summary>
		/// Call the AI with a powerup status.
		/// </summary>
		/// <param name="player">The player who played the card.</param>
		/// <param name="status">The powerup status.</param>
		/// <param name="powerUp">The powerup played.</param>
		public void GameStatus(Player player, PlayerAIBase.STATUS status, PowerUp powerUp)
		{
			if (TcpGuid == null)
				return;

			XDocument doc = new XDocument();
			XElement elemRoot = new XElement("powerup-status",
				new XAttribute("played-by", player.Guid),
				new XAttribute("status", status));
			AddCard(elemRoot, "card", powerUp);
			doc.Add(elemRoot);

			// pass over the full power-up status
			XElement elemDeck = new XElement("cards-deck");
			elemRoot.Add(elemDeck);
			foreach (PowerUp pu in me.PowerUpsDeck)
				AddCard(elemDeck, "card", pu);
			XElement elemHand = new XElement("cards-hand");
			elemRoot.Add(elemHand);
			foreach (PowerUp pu in me.PowerUpsDrawn)
				AddCard(elemHand, "card", pu);

			framework.tcpServer.SendMessage(TcpGuid, doc.ToString());
		}

		public static XDocument BuildMessageXml(List<Player> players, List<Passenger> passengers)
		{
			XDocument doc = new XDocument();
			XElement elemRoot = new XElement("status");
			doc.Add(elemRoot);

			// all players (including me)
			XElement elemPlayers = new XElement("players");
			elemRoot.Add(elemPlayers);
			foreach (Player plyrOn in players)
			{
				XElement elemPlayer = new XElement("player", new XAttribute("guid", plyrOn.Guid),
					new XAttribute("score", plyrOn.Score),
					new XAttribute("total-score", plyrOn.Scoreboard.Sum()),
					new XAttribute("coffee-servings", plyrOn.Limo.CoffeeServings),
					new XAttribute("cards-max", plyrOn.MaxPowerUpsInHand),
					new XAttribute("limo-x", plyrOn.Limo.Location.TilePosition.X),
					new XAttribute("limo-y", plyrOn.Limo.Location.TilePosition.Y),
					new XAttribute("limo-angle", plyrOn.Limo.Location.Angle));
				if (plyrOn.Passenger != null)
					elemPlayer.Add(new XAttribute("passenger", plyrOn.Passenger.Name));
				if (plyrOn.PassengersDelivered.Count > 0)
					elemPlayer.Add(new XAttribute("last-delivered",
						plyrOn.PassengersDelivered[plyrOn.PassengersDelivered.Count - 1].Name));
				if (plyrOn.PowerUpNextBusStop != null)
					AddCard(elemPlayer, "next-bus-stop", plyrOn.PowerUpNextBusStop);
				if (plyrOn.PowerUpThisTransit != null)
					AddCard(elemPlayer, "transit", plyrOn.PowerUpThisTransit);
				elemPlayers.Add(elemPlayer);
			}

			// all passengers
			XElement elemPassengers = new XElement("passengers");
			elemRoot.Add(elemPassengers);
			foreach (Passenger psngrOn in passengers)
			{
				XElement elemPassenger = new XElement("passenger", new XAttribute("name", psngrOn.Name));
				if (psngrOn.Destination != null)
					elemPassenger.Add(new XAttribute("destination", psngrOn.Destination.Name));
				if (psngrOn.Car != null)
				{
					elemPassenger.Add(new XAttribute("status", "travelling"));
					elemPassenger.Add(new XAttribute("limo-driver", players.First(p =>p.Limo == psngrOn.Car).Name));
				}
				else if (psngrOn.Lobby != null)
				{
					elemPassenger.Add(new XAttribute("lobby", psngrOn.Lobby.Name));
					elemPassenger.Add(new XAttribute("status", "lobby"));
				}
				else
				{
					Trap.trap();
					elemPassenger.Add(new XAttribute("status", "done"));
				}
				if (psngrOn.Route.Count > 0)
					elemPassenger.Add(new XAttribute("route", string.Join(";", psngrOn.Route)));

				elemPassengers.Add(elemPassenger);
			}

			return doc;
		}

		private static void AddCard(XElement parent, string part, PowerUp powerup)
		{

			XElement element = new XElement(part,
				new XAttribute("card", powerup.Card),
				new XAttribute("ok-to-play", powerup.OkToPlay));
			if (powerup.Company != null)
				element.Add(new XAttribute("company", powerup.Company.Name));
			if (powerup.Passenger != null)
				element.Add(new XAttribute("passenger", powerup.Passenger.Name));
			if (powerup.Player != null)
				element.Add(new XAttribute("player", powerup.Player.Name));
			parent.Add(element);
		}

		/// <summary>
		///     Post an order to the engine. This can be called from a thread other than the UI thread.
		/// </summary>
		/// <param name="myPlayerGuid">The player this order is for.</param>
		/// <param name="path">The new path for this player's limo. Count == 0 for no path change.</param>
		/// <param name="pickUp">The new pick-up list for this player's limo. Count == 0 for no pick-up change.</param>
		public void PostOrder(string myPlayerGuid, List<Point> path, List<Passenger> pickUp)
		{
			sendOrders(myPlayerGuid, path, pickUp);
		}
	}
}