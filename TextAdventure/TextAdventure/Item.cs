using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextAdventure.Locations;
using TextAdventure.NPCs;

[assembly: InternalsVisibleTo(assemblyName: "UnitTestAdventure")]

namespace TextAdventure.Items
{
	/// <summary>
	/// Used to create Item objects. The class also controls the results of game actions that are performed on an item.
	/// </summary>
	[Serializable]
	internal class Item
	{
		// PROPERTIES

		internal string Name { get; set; }

		/// <summary>
		/// Text that is displayed in the user's inventory, and when an item is dropped in a location that is not its starting location.</summary>
		internal string DisplayName { get; set; }

		// TODO: Fix this so that, for example, the icicle is not magically unbroken if the user breaks it and drops it in the Ice Castle.
		/// <summary>
		/// Text that is displayed the first time a user enters a location, or when an item is dropped in its original location.</summary>
		internal string InitialText { get; set; }

		/// <summary>
		/// Text that is displayed when the user 'Looks' at an item.</summary>
		internal string Description { get; set; }

		internal string CurrentLocation { get; set; }

		internal string InitialLocation { get; set; }

		/// <summary>
		/// The name of the NPC that the item is intended for. eg. 'crone' for the tarot card. Used to determine how an NPC reacts to the 'Give' command.</summary>
		internal string Target { get; set; }

		///<summary>
		///The text for handling special cases for each item.</summary>
		internal Dictionary<string, string> SpecialActions { get; set; }

		/// <summary>
		/// Constructor is coded to take parameters from a .json file </summary>
		public Item(string name, string displayName, string initialText, string description, string initialLocation, string target, Dictionary<string, string> specialActions)
		{
			Name = name;
			DisplayName = displayName;
			InitialText = initialText;
			Description = description;
			InitialLocation = initialLocation;
			CurrentLocation = InitialLocation;
			Target = target;
			SpecialActions = specialActions;
		}

		/// <summary>
		/// Checks if an item's location matches the player's current location. </summary>
		internal bool IsHere()
		{
			if (CurrentLocation == Program.player.CurrentLocation.Name)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Called from the Parser when a user uses the 'look' command, followed by the name of an item.</summary>
		internal void Look()
		{
			if (Program.player.CurrentLocation.Name == CurrentLocation || Program.player.Inventory.ContainsKey(Name))
			{
				Program.WordWrap($"{Description}", Program.ActionTextColor);
			}
			else
			{
				Program.WordWrap($"You don't see the {DisplayName} here.", Program.AlertColor);
			}
		}

		/// <summary>
		/// Called from the Parser when the user attempts to "take" an item. </summary>
		internal void Take()
		{
			if (!Program.player.Inventory.ContainsKey(Name))
			{
				Program.items.Remove(Name);
				Program.player.Inventory.Add(Name, this);
				Program.WordWrap($"The {DisplayName} added to inventory.", Program.HighlightColor);
			}
		}
	}
}
