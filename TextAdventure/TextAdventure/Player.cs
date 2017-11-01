using Newtonsoft.Json;
using System;
using System.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextAdventure.Locations;
using TextAdventure.Items;
using TextAdventure.NPCs;
using static System.Console;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TextAdventure
{
	/// <summary>
	/// Holds player information and controls the results of game actions that are performed by the player.
	/// </summary>
	[Serializable]
	internal class Player
	{
		// PROPERTIES

		internal string Name { get; set; }
		internal int CommandCount { get; set; }
		internal Location CurrentLocation { get; set; }
		internal Dictionary<string, Item> Inventory { get; set; }
		internal bool IsTraveling { get; set; }
		internal bool IsPlaying { get; set; }
		internal int MoveCount { get; set; }
		internal int PassportStamps { get; set; }
		internal bool WantsMusic { get; set; }

		/// <summary> Constructor is coded to take parameters from a .json file </summary>
		public Player(string name, string commandCount, string currentLocation, Dictionary<string, Item> inventory, bool isTraveling, bool isPlaying, string moveCount, string passportStamps, bool wantsMusic)
		{
			Name = name;
			CommandCount = Convert.ToInt32(commandCount);
			CurrentLocation = Program.locations[currentLocation];
			Inventory = inventory;
			Inventory.Add("passport", Program.items["passport"]);
			Program.items.Remove("passport");
			IsTraveling = isTraveling;
			IsPlaying = isPlaying;
			MoveCount = Int32.Parse(moveCount);
			PassportStamps = Int32.Parse(passportStamps);
			WantsMusic = wantsMusic;
		}

		/// <summary> Called from the Parser. This method allows user to move around the game world. </summary>
		internal void Move(string direction)
		{
			int desired_x = this.CurrentLocation.XCoord;
			int desired_y = this.CurrentLocation.YCoord;

			switch (direction)
			{
				case "north":
					desired_y++;
					break;
				case "east":
					desired_x++;
					break;
				case "south":
					desired_y--;
					break;
				case "west":
					desired_x--;
					break;
				case "up":
					if (CurrentLocation.Name == "Bottom of the Beanstalk")
					{
						desired_y++;
						break;
					}
					else
					{
						Program.WordWrap("There's nothing to climb up here.", Program.AlertColor);
						break;
					}
				case "down":
					if (CurrentLocation.Name == "Top of the Beanstalk")
					{
						desired_y--;
						break;
					}
					else
					{
						Program.WordWrap("There's nothing to climb down here.", Program.AlertColor);
						break;
					}
				default:
					Program.WordWrap("I don't understand where you want to go.", Program.AlertColor);
					break;
			}

			foreach (Location location in Program.locations.Values)
			{
				if (desired_x == location.XCoord && desired_y == location.YCoord && location.IsAvailable)
				{
					CurrentLocation = location;
					IsTraveling = true;
					MoveCount++;
					Program.UpdateHeader(this);
					location.DisplayLocation(this);
					return;
				}
			}

			if (this.IsTraveling != true)
			{
				Program.WordWrap(this.CurrentLocation.DangerMsg, Program.AlertColor);
			}

		} // End Move()

		/// <summary> Called from the Parser when a user "drops" an item. </summary>
		internal void Drop(string itemToDrop)
		{
			if (itemToDrop != "passport")
			{
				Item droppedItem = Inventory[itemToDrop];
				Inventory.Remove(itemToDrop);
				droppedItem.CurrentLocation = CurrentLocation.Name;
				Program.items.Add(itemToDrop, droppedItem);
				Program.WordWrap($"You've dropped the {droppedItem.DisplayName}.", Program.HighlightColor);
			}
			else
			{
				Program.WordWrap($"You drop your passport onto the ground. As soon as it lands, it disappears in a puff of golden glitter! You feel in your pocket and realize that the passport is now safely back where it belongs.", Program.HighlightColor);
			}
		}

		/// <summary> Called from the Parser when a user "gives" an item to an NPC. </summary>
		internal void Give(string item, string target)
		{
			Program.nPCs[target].TakeItem(item);
		}

		internal void Use(string itemToUse, string target)
		{
			Program.nPCs[target].UseItem(itemToUse);
		}

		/// <summary> Determines whether or not the user has an item. </summary>
		internal bool HasItem(string item)
		{
			// For each key in the dictionary
			foreach (string i in this.Inventory.Keys)
			{
				//if that item has an action that is the desired action
				if (i == item)
				{
					return true;
				}
			}
			return false;
		}
	} // End class Player
}


