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
using System.Linq;
using System.Windows.Forms;
using Windwardopolis2Library.units;

namespace Windwardopolis2
{
	public partial class PlayCardForm : Form
	{
		private readonly List<Passenger> passengers;
		private readonly List<Player> players;

		public PlayCardForm(IList<Company> companies, IList<Passenger> passengers, IEnumerable<Player> players)
		{
			InitializeComponent();

			// distinct lists so our sorts does not sort the passed in list's order
			this.passengers = new List<Passenger>(passengers);
			this.players = new List<Player>(players);
			this.passengers.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
			this.players.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

			// populate cards
			List<PowerUp> listPowerUps = PowerUp.atAllStores();
			listPowerUps.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
			foreach (PowerUp powerUp in listPowerUps)
				comboBoxCards.Items.Add(new ItemWrapper(powerUp.Card.ToString(), powerUp));
			listPowerUps = PowerUp.atOneStore(passengers, companies);
			listPowerUps.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
			foreach (PowerUp powerUp in listPowerUps)
				comboBoxCards.Items.Add(new ItemWrapper(powerUp.Name, powerUp));

			// populate the played by list
			foreach (Player plyrOn in this.players)
				comboBoxPlayedBy.Items.Add(new ItemWrapper(plyrOn.Name, plyrOn));

			DetermineEnables();
		}

		public PowerUp Card
		{
			get
			{
				PowerUp powerup = new PowerUp((PowerUp) ((ItemWrapper)comboBoxCards.SelectedItem).item) 
							{OkToPlay = true};
				ItemWrapper item = comboBoxCompanyPassengerPlayer.SelectedItem as ItemWrapper;
				if (item == null) 
					return powerup;
				Company cmpy = item.item as Company;
				if (cmpy != null)
					powerup.Company = cmpy;
				Passenger psngr = item.item as Passenger;
				if (psngr != null)
					powerup.Passenger = psngr;
				Player plyr = item.item as Player;
				if (plyr != null)
					powerup.Player = plyr;
				return powerup;
			}
		}

		public Player PlayedBy
		{
			get
			{
				return (Player) ((ItemWrapper) comboBoxPlayedBy.SelectedItem).item;
			}
		}

		private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
		{
			PowerUp pu = (PowerUp)((ItemWrapper)comboBoxCards.SelectedItem).item;
			comboBoxCompanyPassengerPlayer.Items.Clear();
			switch (pu.Card)
			{
				case PowerUp.CARD.MOVE_PASSENGER:
					labelCompanyPassengerPlayer.Text = "Passenger:";
					foreach (Passenger psngrOn in passengers.Where(p => p.Car == null))
						comboBoxCompanyPassengerPlayer.Items.Add(new ItemWrapper(psngrOn.Name, psngrOn));
					break;

				case PowerUp.CARD.CHANGE_DESTINATION:
					labelCompanyPassengerPlayer.Text = "Player (passenger):";
					foreach (Player plyrOn in players.Where(p => p.Passenger != null))
						comboBoxCompanyPassengerPlayer.Items.Add(new ItemWrapper(string.Format("{0} ({1})", plyrOn.Name, plyrOn.Passenger.Name), plyrOn));
					break;

				case PowerUp.CARD.STOP_CAR:
					labelCompanyPassengerPlayer.Text = "Player:";
					foreach (Player plyrOn in players)
						comboBoxCompanyPassengerPlayer.Items.Add(new ItemWrapper(plyrOn.Name, plyrOn));
					break;

				case PowerUp.CARD.MULT_DELIVERING_PASSENGER:
					labelCompanyPassengerPlayer.Text = "Passenger:";
					comboBoxCompanyPassengerPlayer.Items.Add(new ItemWrapper(pu.Passenger.Name, pu.Passenger));
					break;

				case PowerUp.CARD.MULT_DELIVER_AT_COMPANY:
					labelCompanyPassengerPlayer.Text = "Company:";
					comboBoxCompanyPassengerPlayer.Items.Add(new ItemWrapper(pu.Company.Name, pu.Company));
					break;

				case PowerUp.CARD.MULT_DELIVERY_QUARTER_SPEED:
				case PowerUp.CARD.ALL_OTHER_CARS_QUARTER_SPEED:
				case PowerUp.CARD.RELOCATE_ALL_CARS:
				case PowerUp.CARD.RELOCATE_ALL_PASSENGERS:
					break;
			}

			if (comboBoxCompanyPassengerPlayer.Items.Count == 1)
				comboBoxCompanyPassengerPlayer.SelectedIndex = 0;

			DetermineEnables();
		}

		private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			DetermineEnables();
		}

		private void DetermineEnables()
		{
			comboBoxCompanyPassengerPlayer.Enabled = comboBoxCompanyPassengerPlayer.Items.Count > 0;
			labelCompanyPassengerPlayer.Visible = comboBoxCompanyPassengerPlayer.Enabled;
			
			btnOk.Enabled = comboBoxCards.SelectedIndex != -1 && comboBoxPlayedBy.SelectedIndex != -1 &&
			                (comboBoxCompanyPassengerPlayer.Enabled == false ||
			                 comboBoxCompanyPassengerPlayer.SelectedIndex != -1);
		}

		private class ItemWrapper
		{
			private readonly string text;
			public readonly object item;

			public ItemWrapper(string text, object item)
			{
				this.text = text;
				this.item = item;
			}

			public override string ToString()
			{
				return text;
			}
		}
	}
}
