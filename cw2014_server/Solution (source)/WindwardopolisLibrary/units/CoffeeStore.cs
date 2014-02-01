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

namespace Windwardopolis2Library.units
{
	/// <summary>
	/// Originally the limos stopped to get beer. But there were worries that some would object to this so it's now coffee.
	/// You may still see liquor in comments in places.
	/// </summary>
	public class CoffeeStore
	{
		/// <summary>
		/// The name of the coffee store.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The map tile with the coffee store's bus stop.
		/// </summary>
		public Point BusStop { get; private set; }

		/// <summary>
		/// The coffee logo.
		/// </summary>
		public Bitmap Logo { get; private set; }

		private CoffeeStore(string name, Point busStop)
		{
			Name = name;
			BusStop = busStop;
		}

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

		public static void GenerateStores(List<Point> coffeeStopLocations, out List<CoffeeStore> stores)
		{
			stores = new List<CoffeeStore>();
			int index = 0;
			foreach (var stop in coffeeStopLocations)
				stores.Add(new CoffeeStore(StoreNames[Math.Min(index++, StoreNames.Length-1)], stop));
		}

		private static readonly string[] StoreNames = { "Starbucks", "Costa Coffee", "The Coffee Bean", "Gloria Jean’s Coffees", "Caribou Coffee", "Peet’s Coffee and Tea", "Tully’s Coffee" };
	}
}
