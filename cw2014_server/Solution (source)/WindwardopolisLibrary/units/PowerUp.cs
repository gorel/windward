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

namespace Windwardopolis2Library.units
{
	/// <summary>
	/// The power-ups available to cars.
	/// </summary>
	public class PowerUp
	{
		private Passenger passenger;
		private Player player;

		/// <summary>
		/// The specific power of this powerUp.
		/// </summary>
		public enum CARD
		{
			/// <summary>Will move a passenger (not in a car) to a random bus stop (can play anytime).</summary>
			MOVE_PASSENGER,

			/// <summary>Change destination for the passenger in an opponent’s car to a random company (can play anytime).
			/// You set the Player, not the Passenger for this powerup.</summary>
			CHANGE_DESTINATION,

			/// <summary>Delivery is 1.5X points, but your car travels at 1/4 speed.</summary>
			MULT_DELIVERY_QUARTER_SPEED,

			/// <summary>Drop all other cars to 1/4 speed for 30 seconds (can play anytime).</summary>
			ALL_OTHER_CARS_QUARTER_SPEED,

			/// <summary>Can make a specific car stop for 30 seconds (tacks on road) (can play anytime).</summary>
			STOP_CAR,

			/// <summary>Relocate all cars (including yours) to random locations (can play anytime).</summary>
			RELOCATE_ALL_CARS,

			/// <summary>Relocate all passengers at bus stops to random locations (can play anytime).</summary>
			RELOCATE_ALL_PASSENGERS,

			/// <summary>1.2X multiplier for delivering a specific person (we have one card for each passenger).</summary>
			MULT_DELIVERING_PASSENGER,

			/// <summary>1.2X multiplier for delivering at a specific company (we have one card for each company).</summary>
			MULT_DELIVER_AT_COMPANY,
		}

		private PowerUp(string name, CARD card, Bitmap logo)
		{
			Name = name;
			Card = card;
			Logo = logo;
		}

		private PowerUp(string name, CARD card, Bitmap logo, Passenger passenger)
		{
			Name = string.Format(name, passenger.Name);
			Card = card;
			Logo = logo;
			Passenger = passenger;
		}

		private PowerUp(string name, CARD card, Bitmap logo, Company company)
		{
			Name = string.Format(name, company.Name);
			Card = card;
			Logo = logo;
			Company = company;
		}

		public PowerUp(PowerUp src)
		{
			passenger = src.passenger;
			player = src.player;
			Name = src.Name;
			Card = src.Card;
			Logo = src.Logo;
			Company = src.Company;
			OkToPlay = src.OkToPlay;
		}

		/// <summary>
		/// The name of the power-up.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The power-up card.
		/// </summary>
		public CARD Card { get; private set; }

		/// <summary>
		///     The passenger logo.
		/// </summary>
		public Bitmap Logo { get; private set; }

		/// <summary>
		/// The passenger affected for MOVE_PASSENGER, MULT_DELIVERING_PASSENGER.
		/// </summary>
		public Passenger Passenger
		{
			get { return passenger; }
			set
			{
				if (Card == CARD.CHANGE_DESTINATION)
					throw new ApplicationException("set the Player for CHANGE_DESTINATION");
				Name = string.Format(Name, value.Name);
				passenger = value;
			}
		}

		/// <summary>
		/// The player affected for CHANGE_DESTINATION, STOP_CAR
		/// </summary>
		public Player Player
		{
			get { return player; }
			set
			{
				Name = string.Format(Name, value.Name);
				player = value;
			}
		}

		/// <summary>
		/// The company affected for MULT_DELIVER_AT_COMPANY.
		/// </summary>
		public Company Company { get; set; }

		/// <summary>
		/// It's ok to play this card. This is false until a card is drawn and the limo then visits a stop.
		/// </summary>
		public bool OkToPlay { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// These are placed at all stores, but removed from all remaining stores when selected at a store.
		/// </summary>
		/// <returns>a list of the powerups.</returns>
		public static List<PowerUp> atAllStores()
		{
			List<PowerUp> powerups = new List<PowerUp>
			{
				new PowerUp("Move passenger {0} to random bus stop.", CARD.MOVE_PASSENGER, Sprites.businessman_add),
				new PowerUp("Change destination for player {0} passenger to random company.", CARD.CHANGE_DESTINATION, Sprites.office_building),
				new PowerUp("Double delivery score, limo runs at 1/4 speed.", CARD.MULT_DELIVERY_QUARTER_SPEED, Sprites.oldtimer),
				new PowerUp("Drop all cars to 1/4 speed for 30 seconds.", CARD.ALL_OTHER_CARS_QUARTER_SPEED, Sprites.police_car),
				new PowerUp("Stop limo {0} for 30 seconds.", CARD.STOP_CAR, Sprites.policeman_usa),
				new PowerUp("Relocate all cars to random locations.", CARD.RELOCATE_ALL_CARS, Sprites.atom),
				new PowerUp("Relocate all passengers waiting at companies to random companies.", CARD.RELOCATE_ALL_PASSENGERS, Sprites.houses)
			};
			return powerups;
		}

		/// <summary>
		/// These are spread randomly among the stores, each placed at just one store.
		/// </summary>
		/// <returns>a list of the powerups.</returns>
		public static List<PowerUp> atOneStore(IList<Passenger> passengers, IList<Company> companies)
		{
			List<PowerUp> powerups = passengers.Where(p => p.PointsDelivered == 1).Select(psngr => new PowerUp(string.Format("1.2X delivery score for passenger {0}", psngr.Name), 
								CARD.MULT_DELIVERING_PASSENGER, Sprites.sports_car, psngr)).ToList();
			powerups.AddRange(companies.Select(cmpy => new PowerUp(string.Format("1.2X delivery score to destination {0}", cmpy.Name), 
								CARD.MULT_DELIVER_AT_COMPANY, Sprites.hotel, cmpy)));
			return powerups;
		}

		public static Dictionary<CARD, Bitmap> allBitmaps()
		{
			Dictionary<CARD, Bitmap> bitmaps = new Dictionary<CARD, Bitmap>
			{
				{CARD.MOVE_PASSENGER, Sprites.businessman_add},
				{CARD.CHANGE_DESTINATION, Sprites.office_building},
				{CARD.MULT_DELIVERY_QUARTER_SPEED, Sprites.oldtimer},
				{CARD.ALL_OTHER_CARS_QUARTER_SPEED, Sprites.police_car},
				{CARD.STOP_CAR, Sprites.policeman_usa},
				{CARD.RELOCATE_ALL_CARS, Sprites.atom},
				{CARD.RELOCATE_ALL_PASSENGERS, Sprites.houses},
				{CARD.MULT_DELIVERING_PASSENGER, Sprites.sports_car},
				{CARD.MULT_DELIVER_AT_COMPANY, Sprites.hotel},
			};
			return bitmaps;

		} 
	}
}
