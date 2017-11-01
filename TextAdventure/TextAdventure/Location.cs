using System;
using System.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure.Items;
using TextAdventure.NPCs;
using static System.Console;
using System.IO;

namespace TextAdventure.Locations
{
	/// <summary>
	/// Used to create Loation objects. The class also controls display of the text descriptions for each location, and of the items and characters currently in that location.
	/// </summary>
	[Serializable]
	internal class Location
	{
		// LOCATION PROPERTIES

		internal string Name { get; set; }

		/// <summary>Text that is displayed if the user tries to go in a direction that is not available.</summary>
		internal string DangerMsg { get; set; }

		/// <summary>Text that is displayed below the game title when the game starts or the player enters a key location.</summary>
		internal string WelcomeMsg { get; set; }

		/// <summary>Text that is displayed the FIRST time a player enters a new location and Visited is false.</summary>
		internal string IntroMsg { get; set; }

		/// <summary>Text that is displayed when the 'Look' method is called. Provides more detail than the standard description.</summary>
		internal string LookMsg { get; set; }

		/// <summary>Text that is displayed when the user enters a location and Visited is true. Not as detailed at IntroMsg or LookMsg.</summary>
		internal string Description { get; set; }

		/// <summary>Location's horizontal grid location.</summary>
		internal int XCoord { get; set; }

		/// <summary>Location's vertical grid location.</summary>
		internal int YCoord { get; set; }

		/// <summary>Whether or not the player has been to this location during the current game.</summary>
		internal bool Visited { get; set; } = false;

		/// <summary>Determines whether or not the headers should reset on entering the location. Should be true only in locations with NPCs present.</summary>
		internal bool IsKeyLocation { get; set; }

		/// <summary>Determines whether or not a location is available for the player to travel into.</summary>
		internal bool IsAvailable { get; set; }

        public string MidiFilePath { get; set; }
        
		/// <summary> Location constructor.</summary>
		public Location(string name, string dangerMsg, string welcomeMsg, string introMsg, string lookMsg, string description, int x_cord, int y_cord, bool isKeyLocation, bool isAvailable, string midiFileName)

		{
			Name = name;
			DangerMsg = dangerMsg;
			WelcomeMsg = welcomeMsg;
			IntroMsg = introMsg;
			LookMsg = lookMsg;
			Description = description;
			XCoord = x_cord;
			YCoord = y_cord;
			IsKeyLocation = isKeyLocation;
			IsAvailable = isAvailable;
            MidiFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../MidiMusic", midiFileName);
		}

        // TODO: Move to player object
        /// <summary> Calls the midi to play music </summary>
        internal void PlayMusic()
        {
            string filePath = MidiFilePath;
            string openCommand = "open " + filePath + " alias music";
            MidiOutCaps.Mci(openCommand);
            MidiOutCaps.Mci("play music");
        }

        // TODO: Move to player object
        /// <summary> Stops the music </summary>
        internal void StopMusic() => MidiOutCaps.Mci("close music");

        /// <summary> Displays location intro, text, and music based on whether the player has already visited this location </summary>
        /// <param name="player">The current player. Needed to determine if music should play and the player.IsTraveling boolean.</param>
        internal void DisplayLocation(Player player)
		{
			if (player.WantsMusic)
			{
                StopMusic();
                PlayMusic();
			}

            if (!Visited)
			{
				Intro(player);
			}
			else if (player.IsTraveling)
			{
				EnterLocation(player);
			}
		}

		/// <summary> This method is called the first time this location is visited </summary> 
		/// <param name="player">The current player. Needed to effect the player.IsTraveling property. Is passed into helper methods.</param>
		/// <seealso cref="DisplayHeaders(Player)"/>
		/// <seealso cref="ShowItems(StringBuilder, Player)"/>
		internal virtual void Intro(Player player)
		{
			if (IsKeyLocation)
			{
				DisplayHeaders(player);
			}

			Visited = true;

			StringBuilder sb = new StringBuilder(IntroMsg);

			Program.WordWrap(sb.ToString(), Program.ActionTextColor);

			sb.Clear();
			ShowCharacters(sb);
			ShowItems(sb, player);

			player.IsTraveling = false;

			if (sb.Length > 0)
			{
				string outString = sb.ToString();
				Program.WordWrap(outString, Program.ActionTextColor);
				WriteLine();
			}
		}

		/// <summary> This method is called every time this location is visited </summary> 
		/// <param name="player">The current player. Needed to effect the player.IsTraveling property. Is passed into helper methods.</param>
		/// <seealso cref="DisplayHeaders(Player)"/>
		/// <seealso cref="ShowItems(StringBuilder, Player)"/>
		internal void EnterLocation(Player player)
		{
            if (player.WantsMusic)
            {
                StopMusic();
                PlayMusic();
            }
            else // ! player.WantsMusic
            {
                StopMusic();
            }

			if (IsKeyLocation)
			{
				DisplayHeaders(player);
			}

			StringBuilder sb = new StringBuilder(Description);

			if (player.IsTraveling)
			{
				Program.WordWrap(sb.ToString(), Program.ActionTextColor);

				sb.Clear();
				ShowCharacters(sb);
				ShowItems(sb, player);

				if (sb.Length > 0)
				{
					Program.WordWrap(sb.ToString(), Program.ActionTextColor);
					WriteLine();
				}
				player.IsTraveling = false;
			}
		} // End EnterLocation

		/// <summary>Called from the Parser when the player inputs 'Look' without specifying an object. Displays the text of the LookMsg property, as well as any NPCs or Items in the location.</summary>
		/// <param name="player">The current player. Passed into helper method.</param>
		/// <seealso cref="ShowItems(StringBuilder, Player)"/>
		internal void Look(Player player)
		{
			StringBuilder sb = new StringBuilder(LookMsg);
			Program.WordWrap(sb.ToString(), Program.ActionTextColor);

			sb.Clear();

			ShowCharacters(sb);
			ShowItems(sb, player);

			if (sb.Length > 0)
			{
				Program.WordWrap(sb.ToString(), Program.ActionTextColor);
				WriteLine();
			}
		}

		// HELPER METHODS

		/// <summary>Shows all characters in the current location. Description varies based on whether character has the hero item. </summary>
		/// <param name="builder">The string builder that was created outside this helper method to hold the text string that will be shown to the user.</param>
		internal void ShowCharacters(StringBuilder builder)
		{
			foreach (NPC character in Program.nPCs.Values)
			{
				if (character.Location == Name)
				{
					if (character.HasHeroItem)
					{
						builder.Append($"{character.DescriptionAfter}");
					}
					else
					{
						builder.Append($"{character.DescriptionBefore}");
					}
				}
			}
		}

		/// <summary>Shows all items in the current location.</summary>
		/// <param name="builder">The string builder that was created outside this helper method to hold the text string that will be shown to the user.</param>
		/// <param name="player">The current player. Used to determine the player's current location, and display only items that are in that location.</param>
		internal void ShowItems(StringBuilder builder, Player player)
		{
			foreach (Item item in Program.items.Values)
			{
				if (item.CurrentLocation == player.CurrentLocation.Name && item.CurrentLocation == item.InitialLocation)
				{
					builder.Append(item.InitialText);
				}
				else if (item.CurrentLocation == player.CurrentLocation.Name && item.CurrentLocation != item.InitialLocation)
				{
					builder.Append($" You see the {item.DisplayName.ToUpper()} here.");
				}
			}
		}

		/// <summary> Displays a title and a welcome message </summary>
		/// <param name="player">The current player. Passed to DisplayTitle.</param>
		/// <seealso cref="Program.DisplayTitle(Player)"/>
		internal void DisplayHeaders(Player player)
		{
			Program.DisplayTitle(player);
			Program.WordWrap($"{WelcomeMsg}, {Program.player.Name}!", Program.ConfirmationColor);
		}

		/// <summary> Takes in a dictionary and displays text from that dictionary in different colors.</summary>
		/// <param name="sb">The string builder holding the string being created for display to the user.</param>
		/// <param name="textDict">The dictionary that holds the text to be displayed (key) and color in which it should be displayed (value).</param>
		internal void InteractionResult(Dictionary<string, string> textDict, StringBuilder sb)
		{
			foreach (KeyValuePair<string, string> pair in textDict)
			{
				ConsoleColor color;

				switch (pair.Value)
				{
					case "cyan":
						color = Program.ActionTextColor;
						break;
					case "green":
						color = Program.DialogColor;
						break;
					case "yellow":
						color = Program.HighlightColor;
						break;
					default:
						color = Program.BoldTextColor;
						break;
				}

				Program.WordWrap($"{pair.Key}", color);
			}
		}
	} // End class Location
}
