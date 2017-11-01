using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure.Locations;
using TextAdventure.Items;
using System.Runtime.CompilerServices;
using System.Collections;

[assembly: InternalsVisibleTo(assemblyName: "UnitTestAdventure")]

namespace TextAdventure.NPCs
{
	/// <summary>
	/// Used to create NPC (non-player character) objects. The class also controls the results of game actions that are performed by an NPC.
	/// </summary>
	[Serializable]
	internal class NPC
	{
		// PROPERTIES

		internal string Name { get; set; }

		/// <summary>
		/// Current location of the character. This must match the Name property of a Location.
		/// </summary>
		internal string Location { get; set; }

		/// <summary>
		/// Short description of the character before receiving their hero item.
		/// </summary>
        internal string DescriptionBefore { get; set; }

		/// <summary>
		/// Short description of the character after receiving their hero item.
		/// </summary>
        internal string DescriptionAfter { get; set; }

		/// <summary>
		/// Response text for 'look (character)' before receiving hero item. The key is the text to display, the value is the text color.
		/// </summary>
        internal Dictionary<string, string> LookTextBefore { get; set; }

		/// <summary>
		/// Response text for 'look (character)' after receiving hero item. The key is the text to display, the value is the text color.
		/// </summary>
		internal Dictionary<string, string> LookTextAfter { get; set; }

		/// <summary>
		/// Response text for 'ask (character)' before receiving hero item. The key is the text to display, the value is the text color.
		/// </summary>
		internal Dictionary<string, string> TalkBefore { get; set; }

		/// <summary>
		/// Response text for 'ask (character)' after receiving hero item. The key is the text to display, the value is the text color.
		/// </summary>
		internal Dictionary<string, string> TalkAfter { get; set; }

		/// <summary>
		/// Whether or not the character has been given their hero item. Used to determine which character-related text is displayed.
		/// </summary>
		internal bool HasHeroItem { get; set; } = false;

		/// <summary>
		/// Response text when a character is given their hero item. The key is the text to display, the value is the text color.
		/// </summary>
		internal Dictionary<string, string> GainHeroItemText { get; set; }

		/// <summary>NPC Constructor.</summary>
        public NPC(string name, 
                   string location, 
                   string descriptionBefore, 
                   string descriptionAfter, 
                   Dictionary<string, string> lookTextBefore, 
                   Dictionary<string, string> lookTextAfter, 
                   Dictionary<string, string> talkBefore, 
                   Dictionary<string, string> talkAfter, 
                   bool hasHeroItem, 
                   Dictionary<string, string> gainHeroItemText)
		{
			Name = name;
			Location = location;
			DescriptionBefore = descriptionBefore;
			DescriptionAfter = descriptionAfter;
			LookTextBefore = lookTextBefore;
			LookTextAfter = lookTextAfter;
			TalkBefore = talkBefore;
			TalkAfter = talkAfter;
			HasHeroItem = hasHeroItem;
			GainHeroItemText = gainHeroItemText;
		}

		/// <summary>Called from the Parser when the player uses the 'give (item)' command with the character's hero item.</summary>
		/// <param name="item">Name of the item being given to the NPC.</param>
		internal void TakeItem(string item)
		{
			UseItem(item);

			switch (item)
			{
				case "gumbo":
					Program.nPCs["snowman"].Location = null;
					Program.nPCs["puss in boots"].Location = "Start";
					break;
				case "ribbon":
					break;
				case "trumpet":
					Program.player.Inventory.Add("key", Program.items["key"]);
					break;
				default:
					Program.player.PassportStamps++;
					Program.WordWrap("Passport stamped!", Program.HighlightColor);
					break;
			}
		}

		/// <summary>Called from the Parser when the player uses the 'use (item)' command with a non-living character's hero item.</summary>
		/// <param name="item">Name of the item being used.</param>
		internal void UseItem(string item)
		{
			Program.player.Inventory.Remove(item);
			HasHeroItem = true;
			InteractionResult(GainHeroItemText);

			if (item == "key")
			{
				Program.locations["Bottom of the Beanstalk"].IsAvailable = true;
			}
		}

		/// <summary> Called from the Parser when the player uses the 'ask (character) command.</summary>
		/// <param name="hasHeroItem">Boolean used to determind which text is displayed.</param>
		internal void Ask(bool hasHeroItem)
		{
			if (TalkAfter == null && TalkBefore == null)
			{
				Program.WordWrap($"You talk to the {Name}, but it doesn't answer.");
			}
			else
			{
				if (hasHeroItem)
				{

                    InteractionResult(TalkAfter);
				}
				else
				{
                    InteractionResult(TalkBefore);
				}
			}
		} // End Ask()

		/// <summary> Called from the Parser when the player uses the 'look (character)' command.</summary>
		internal void Look()
		{
			if (Program.player.CurrentLocation.Name == Location)
			{
				if (HasHeroItem)
				{
                    InteractionResult(LookTextAfter);
				}
				else
				{
                    InteractionResult(LookTextBefore);
				}
			}
			//else
			//{
			//	Program.WordWrap($"\nYou don't see that person here.", Program.AlertColor);
			//}
		}

		/// <summary> Takes in a dictionary and displays text from that dictionary in different colors.</summary>
		/// <param name="textDict">The dictionary that holds the text to be displayed (key) and color in which it should be displayed (value).</param>
		internal void InteractionResult(Dictionary<string, string> textDict)
		{
			foreach (KeyValuePair<string, string> pair in textDict)
			{
				ConsoleColor color;

				switch (pair.Value)
				{
					case "cyan":
						color = ConsoleColor.Cyan;
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
		} // End InteractionResult()
	} // End class NPC
}
