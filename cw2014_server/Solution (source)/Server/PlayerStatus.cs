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
using System.Windows.Forms;
using Windwardopolis2Library.units;

namespace Windwardopolis2
{
	/// <summary>
	///     The window that displays a player's status.
	/// </summary>
	internal partial class PlayerStatus : UserControl
	{
		private static readonly Bitmap[] avatars = {Avatars.avatar1, Avatars.avatar2, Avatars.avatar3, Avatars.avatar4};
		private static int nextAvatar;

		private Passenger passengerOn;
		private Company nextBusStopOn;
		private bool firstTime = true;

		private Dictionary<PowerUp.CARD, Bitmap> powerUpIcons; 

		/// <summary>
		///     Create the window.
		/// </summary>
		/// <param name="player">The player this window is for.</param>
		public PlayerStatus(Player player)
		{
			InitializeComponent();
			pictWinner.Visible = false;
			Player = player;

			// get the bitmaps
			powerUpIcons = PowerUp.allBitmaps();
		}

		/// <summary>
		///     The player this is showing status for.
		/// </summary>
		public Player Player { get; private set; }

		/// <summary>
		///     Redraw this window. Call when status has changed.
		/// </summary>
		public void UpdateStats()
		{
			labelScore.Text = Player.Score.ToString("0.##");

			pictNoConnection.Visible = ! Player.IsConnected;
			pictWinner.Visible = Player.isWinner;

			if (Player.Passenger != passengerOn || Player.NextBusStop != nextBusStopOn || firstTime)
			{
				firstTime = false;

				if (Player.Passenger == null)
				{
					labelPassenger.Text = @"{none}";
					pictPassenger.Image = null;
					if (Player.NextBusStop != null)
					{
						labelDestination.Text = Player.NextBusStop.Name;
						pictDestination.Image = Player.NextBusStop.Logo;
					}
					else
					{
						labelDestination.Text = @"{none}";
						pictDestination.Image = null;
					}
				}
				else
				{
					labelPassenger.Text = Player.Passenger.Name;
					pictPassenger.Image = Player.Passenger.Logo;
					labelDestination.Text = Player.Passenger.Destination.Name;
					pictDestination.Image = Player.Passenger.Destination.Logo;
				}
				passengerOn = Player.Passenger;
				nextBusStopOn = Player.NextBusStop;
			}
			Invalidate(true);
		}

		private void PlayerStatus_Load(object sender, EventArgs e)
		{
//			BackColor = Player.SpriteColor;
			if ((Player.Avatar != null) && (Player.Avatar.Width == 32) && (Player.Avatar.Height == 32))
				pictureBoxAvatar.Image = Player.Avatar;
			else
			{
				pictureBoxAvatar.Image = avatars[nextAvatar++];
				if (nextAvatar >= avatars.Length)
					nextAvatar = 0;
			}
			pictureBoxRobot.Image = Player.Limo.VehicleBitmap;
			labelName.Text = Player.Name;

			if (Player.SpriteColor.R > 250 & Player.SpriteColor.G > 250)
				labelName.BackColor = Player.SpriteColor;
			else
				labelName.ForeColor = Player.SpriteColor;
		}

		private void PlayerStatus_Paint(object sender, PaintEventArgs pe)
		{
			// Delivered passenger avatars - 97, 4 - space = 3
			for (int ind = 0; ind < Player.PassengersDelivered.Count; ind++)
			{
				Bitmap bmp = Player.PassengersDelivered[ind].Logo;
				int width, height;
				if (bmp.Width >= bmp.Height)
				{
					width = 24;
					height = (bmp.Height*24)/bmp.Width;
				}
				else
				{
					height = 24;
					width = (bmp.Width*24)/bmp.Height;
				}
				pe.Graphics.DrawImage(bmp, new Rectangle(4 + ind*(24 + 6) + (24 - width)/2, 82 + (24 - height), width, height));
			}

			// coffee onboard
			int x = 4;
			for (int ind = 0; ind < Player.Limo.CoffeeServings; ind++)
			{
				pe.Graphics.DrawImage(Status.mug, new Rectangle(x, 40, 16, 16));
				x += 20;
			}

			// power-ups in action
			if (Player.Limo.IsFlatTire)
			{
				pe.Graphics.DrawImage(powerUpIcons[PowerUp.CARD.STOP_CAR], new Rectangle(x, 40, 16, 16));
				x += 20;
			}
			if (Player.Limo.IsQuarterSpeed)
			{
				pe.Graphics.DrawImage(powerUpIcons[PowerUp.CARD.ALL_OTHER_CARS_QUARTER_SPEED], new Rectangle(x, 40, 16, 16));
				x += 20;
			}
			if (Player.PowerUpThisTransit != null)
			{
				pe.Graphics.DrawImage(Player.PowerUpThisTransit.Logo, new Rectangle(x, 40, 16, 16));
				x += 20;
			}
			foreach (PowerUp.CARD cardOn in Player.DisplayCards)
			{
				pe.Graphics.DrawImage(powerUpIcons[cardOn], new Rectangle(x, 40, 16, 16));
				x += 20;
			}
		}
	}
}