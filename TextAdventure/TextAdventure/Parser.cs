using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextAdventure.Items;
using TextAdventure.NPCs;
using static System.Console;

namespace TextAdventure
{
	/// <summary>
	/// Handles taking in player input and responding to it.
	/// </summary>
	internal static class Parser
	{
		/// <summary>
		/// Deserialize JSON file containing the dictionary of synonyms for the available command keywords. Also includes the words for violence and swearing that are not used.</summary>
		internal static Dictionary<string, string> ParseDict = Program.parseDict;
		
		/// <summary>This list is used to remove extraneous words from the user input.</summary> 
		private static List<string> skippedWords = new List<string> { "a", "as", "an", "and", "at", "on", "or", "the", "to", "with" };

		/// <summary>Helper method to make sure user input is valid. It checks for [any number of letters] followed by [either a space or any number of letters multiple times].</summary>
		/// <param name="input">A string that is the text the user inputs at the prompt.</param>
		/// <returns>A boolean representing whether or not the user's input matches the given Regex pattern.</returns>
		internal static bool IsValid(string input)
		{
			Regex regex = new Regex(@"^[a-zA-Z]+[a-zA-Z\s]*\z");
			return regex.IsMatch(input);
		}

		/// <summary>Takes the input given by the player, calls IsValid and determines how the input will be handled based on the input. If input is valid, it is split into an array of strings on a whitespace character, and each of those strings is checked against the skippedWords list. Words that are not skipped are added to the result List of strings. If result[0] is a key in the parseDict Dictionary (meaning that it matches one of the available synonyms for the command keywords), result[0] is reassigned to the value associated with that key. The length of the result List is passed to a switch statement based on the length of the result List, which determines how the result values are handled.</summary>
		/// <param name="input">The text entered by the user, generated from the Program.ValidateString method.</param>
		/// <param name="player">The current player. Needed to make some of the command keyword methods work.</param>
		internal static void ParseInput(string input)
		{
			WriteLine();
			input = input.ToLower().Trim();
			List<string> result;
			if (IsValid(input))
			{
				// Split user input into two+ strings at whitespace character.
				string[] words = input.Split(new char[] { ' ' });

				result = new List<string>() { };

				// Check each word against skippedWords, add to result if not in skippedWords
				foreach (string word in words)
				{
					if (!skippedWords.Contains(word))
					{
						result.Add(word);
					}
				}

				if (result.Count > 2) {
					result = CheckForTheQueen(result);
					result = CheckForTheCat(result);
				}
				result = CheckForUnlock(result);
				if (result.Contains("climb"))
					result = ClimbHandler(result);
			}
			else
			{
				BadInput();
				result = new List<string>();
				return;
			}

			// Handle player input that matches one of the standard command verbs.
			string verb = "";
			if (result.Count() > 0 && ParseDict.Keys.Contains(result[0]))
			{
				verb = ParseDict[result[0]];
			}

			if (verb != "go" && result.Count() > 2)
				result = ReduceLongInput(result, verb);

			int resultLength = result.Count();

			// TODO: modify parser to check lengths > 2 for words that are keywords in the game, so commands like "give tarot card to crone" or "get tarot card" are correctly handled.
			switch (resultLength)
			{
				case 0:
					Program.WordWrap("Sorry, I don't understand. This place is magical... but not THAT magical! (Length 0)", Program.AlertColor);
					break;
				case 1:
					OneWordCommands(verb);
					break;
				case 2:
					TwoWordCommands(verb, result[0], result[1]);
					break;
				default:
					Program.WordWrap($"Sorry, I don't understand. Type \"Help\" to learn about what commands you can use. (Default Length)", Program.AlertColor);
					break;
			}
		}

		/// <summary>Takes in a single word from user input and executes the appropriate action method based on that word.</summary>
		/// <param name="command">The word to be switched against.</param>
		public static void OneWordCommands(string command)
		{
			switch (command)
			{
				case "about":
					Program.About(Program.player.Name);
					break;

				case "again":
					{
						bool againResponse = YesNoCheck($"Are you sure you want to start over? y/n", Program.AlertColor);
						if (againResponse)
						{
							ProcessStartInfo info = new ProcessStartInfo(Environment.GetCommandLineArgs()[0]);
							Process.Start(info);
							Environment.Exit(0);
							break;
						}
						else
						{
							Program.WordWrap($"Game restart cancelled.", Program.AlertColor);
							break;
						}
					}

				case "boombox":
					Program.ToggleMusic();
					break;

				case "cheat":
					Program.WordWrap("Magical Cheat-Zor!", Program.AlertColor);
					Program.player.PassportStamps = 4;
					Program.locations["Bottom of the Beanstalk"].IsAvailable = true;
					Program.nPCs["rosie"].HasHeroItem = true;
					break;

				case "cheatgo":
					Program.WordWrap("Magical Travel Cheat-Zor!", Program.AlertColor);
					WriteLine("You can go to one of the following places:");
					foreach (string name in Program.locations.Keys)
					{
						WriteLine(name);
					}
					Program.WordWrap("Where would you like to go? :", Program.AlertColor);
					Write(@" > ");
					string response = ReadLine().Trim();
					if (Program.locations.ContainsKey(response))
					{
						Program.locations[response].IsAvailable = true;
						Program.player.CurrentLocation = Program.locations[response];
						Program.player.CurrentLocation.DisplayLocation(Program.player);
					}
					else
					{
						Program.WordWrap("Sorry...I didn't get that location.");
					}
					break;

				case "exit":
					{
						bool exitResponse = YesNoCheck($"Are you sure you want to stop playing the game? y/n", Program.AlertColor);
						if (exitResponse)
						{
							bool askToSave = YesNoCheck($"Would you like to save your game before you exit?  y/n", Program.AlertColor);
							if (askToSave)
							{
								GameWorld.SaveGame();
							}
							Program.player.IsPlaying = false;
							Program.WordWrap("Press any key to exit...");
							ReadKey();
							Environment.Exit(0);
							break;
						}
						else
						{
							Program.WordWrap($"{ToTitleCase(command)} game cancelled.", Program.AlertColor);
							ResetColor();
							break;
						}
					}

				case "load":
					GameWorld.LoadGame();
					Program.player.IsTraveling = true;
					break;

				case "look":
					Program.player.CurrentLocation.Look(Program.player);
					break;

				case "help":
					Help();
					break;

				case "inventory":
					Program.WordWrap("Your current inventory: ");
					foreach (Item i in Program.player.Inventory.Values)
					{
						Program.WordWrap(i.DisplayName, Program.HighlightColor);
					}
					break;

				case "naughty":
					BadWords();
					break;

				case "save":
					GameWorld.SaveGame();
					Program.player.IsTraveling = true;
					break;

				case "pass":
					break;

				default:
					BadInput();
					break;
			}
		}
		/// <summary>Takes in two words from user input and executes the appropriate action method based on that word.</summary>
		/// <param name="verb">The word to be switched against.</param>
		/// <param name="originalCommand">The verb as input by the user.</param>
		/// <param name="noun">Used to determine what in-game item is being acted upon.</param>
		public static void TwoWordCommands(string verb, string originalCommand, string noun)
		{
			// TODO: Move checking for swearing into a helper method that is called in the main ParseInput method.
			if (ParseDict.Keys.Contains(noun))
			{
				if (ParseDict[noun] == "naughty")
					verb = "naughty";
			}

			switch (verb)
			{
				case "ask":
					if (Program.nPCs.ContainsKey(noun))
					{

						if (Program.nPCs[noun].Location == Program.player.CurrentLocation.Name)
						{
							if (Program.nPCs[noun].HasHeroItem)
							{
								Program.nPCs[noun].Ask(true);
							}
							else
							{
								Program.nPCs[noun].Ask(false);
							}
						}
						else
						{
							Program.WordWrap($"You don't see {ToTitleCase(noun)} here.", Program.AlertColor);
						}
					}
					else
					{
						Program.WordWrap("Who's that? I don't recognise that name", Program.AlertColor);
					}
					break;

				case "cheat":
					Program.WordWrap("Magical World Cheat-Zor!", Program.AlertColor);
					if (Program.items.ContainsKey(noun))
					{
						Program.player.Inventory.Add(noun, Program.items[noun]);
						Program.items.Remove(noun);
					}
					else if (Program.nPCs.ContainsKey(noun))
					{
						Program.nPCs[noun].HasHeroItem = true;
						if (Program.player.PassportStamps < 4)
						{
							Program.player.PassportStamps += 1;

						}
						if (noun == "puss in boots" || noun == "snowman")
						{
							Program.nPCs["snowman"].Location = null;
							Program.nPCs["puss in boots"].Location = Program.locations["Start"].Name;
						}
						foreach (Item item in Program.items.Values)
						{
							if (item.Target == noun)
							{
								Program.items.Remove(item.Name);
								break;
							}
						}
						foreach (Item item in Program.player.Inventory.Values)
						{
							if (item.Target == noun)
							{
								Program.items.Remove(item.Name);
								break;
							}
						}
					}
					break;

				case "drop":
					if (Program.player.HasItem(noun))
					{
						Program.player.Drop(noun);
					}
					else
					{
						Program.WordWrap($"You don't have that item in your inventory. Type \"inventory\" to see what you're currently carrying.");
					}
					break;

				case "give":
					if (Program.player.HasItem(noun))
					{
						string giveTarget = Program.player.Inventory[noun].Target;

						if (giveTarget == "rosie" && noun == "passport")
						{
							ParseInput("use passport");
							break;
						}

						if (giveTarget != null)
						{
							if (Program.nPCs[giveTarget].Location == Program.player.CurrentLocation.Name)
							{
								Program.player.Give(noun, giveTarget);
							}
							else
							{
								foreach (NPC character in Program.nPCs.Values)
								{
									if (character.Location == Program.player.CurrentLocation.Name)
									{
										Program.WordWrap($"{ToTitleCase(character.Name)} isn't interested in the {noun}.");
									}
								}
							}
						}
						else
						{
							foreach (NPC character in Program.nPCs.Values)
							{
								if (character.Location == Program.player.CurrentLocation.Name)
								{
									Program.WordWrap($"{ToTitleCase(character.Name)} isn't interested in the {noun}.");
								}
							}
							Program.WordWrap($"There's no one here to give the {noun} to!");
						}
					}
					else
					{
						Program.WordWrap($"It's nice that you want to give something, but you don't have anything like that.", Program.AlertColor);
					}
					break;

				case "go":
					Program.player.Move(noun);
					break;

				case "look":
					if (Program.items.ContainsKey(noun))
					{
						Program.items[noun].Look();
						break;
					}
					else if (Program.player.Inventory.ContainsKey(noun))
					{
						if (noun == "passport")
						{
							LookAtPassportHandler();
							break;
						}
						else
						{
							Program.player.Inventory[noun].Look();
							break;
						}
					}
					else if (Program.nPCs.ContainsKey(noun))
					{
						Program.nPCs[noun].Look();
						break;
					}
					else
					{
						Program.WordWrap($"I understand that you want to look at something, but I don't understand {noun}.", Program.AlertColor);
					}
					break;

				case "naughty":
					BadWords();
					break;

				case "special":
					UseSpecialCommand(originalCommand, noun);
					break;

				case "take":
					Item target = null;
					foreach (KeyValuePair<string, Item> item in Program.items)
					{
						if (item.Key == noun)
						{
							target = item.Value;
							break;
						}
					}
					if (Program.player.HasItem(noun)) // if the item exists in the inventory dictionary
					{
						Program.WordWrap($"You already have the {Program.player.Inventory[noun].DisplayName}.", Program.ConfirmationColor);
						break;
					}
					// if the item exisits in the wordItems AND it is in the player's current location
					else if (Program.items.ContainsValue(target) && target.CurrentLocation == Program.player.CurrentLocation.Name)
					{
						target.Take();
					}
					// indicate that the item isn't available.
					else if (Program.nPCs.ContainsKey(noun) && Program.nPCs[noun].Location == Program.player.CurrentLocation.Name)
					{
						Program.WordWrap($"You can't {originalCommand} {ToTitleCase(Program.nPCs[noun].Name)}. How rude.");
					}
					else
					{
						Program.WordWrap($"You don't see anything like that here.");
					}
					break;

				case "use":
					if (Program.player.HasItem(noun))
					{
						Dictionary<string, Item> inventory = Program.player.Inventory;
						string useTarget = inventory[noun].Target;

						if (useTarget == "rosie" && noun == "passport")
						{
							bool conditionsMet = CheckPassport();
							if (!conditionsMet)
							{
								Program.WordWrap($"Rosie takes your passport and looks through it, then hands it back to you.", Program.ActionTextColor);
								Program.WordWrap($"I'm afraid you can't use your passport until you have all four visa stamps.", Program.DialogColor);
								break;
							}
							else
							{
								Program.WordWrap($"Rosie takes you passport and looks through it.", Program.ActionTextColor);
								Program.WordWrap($"'Looks like you have all of your visas. Good for you. Hop into the balloon boat and it will take you where you need to go.'", Program.DialogColor);
								Program.WordWrap($"You climb into the boat, and Rosie waves goodbye. The boat whisks you off into the clouds, and takes you home.", Program.ActionTextColor);
								Program.WordWrap("You won the game!", Program.HighlightColor);
								WriteLine();
								Program.WordWrap($"Press a key...", ConsoleColor.Gray);
								ReadKey();
								Program.GameEnd(Program.player);
							}
						}

						if (useTarget != null)
						{
							if (Program.nPCs[useTarget].Location == Program.player.CurrentLocation.Name)
							{
								Program.player.Use(noun, useTarget);
							}
							else
							{
								Program.WordWrap($"There's nothing here you can {originalCommand} with the {inventory[noun].DisplayName}).");
							}
						}
						else
						{
							Program.WordWrap($"You can't do that with the {inventory[noun].DisplayName}!");
						}
					}
					break;

				case "violence":
					Violence();
					break;

				default:
					BadInput();
					break;
			}
		}

		//	ACTION METHODS

		/// <summary>
		/// Method run when the parser detects swear words from the parseDict Dictionary in the user input.
		/// </summary>
		internal static void BadWords()
		{
			Program.WordWrap("That's not appropriate. Your fairy godmother judges you.", Program.AlertColor);
		}

		/// <summary>
		/// Method run when the user types 'help'.
		/// </summary>
		internal static void Help()
		{
			int width = Program.GetWindowWidth();
			string divider = new string('*', width - 2);
			Program.WordWrap($"{ divider}", Program.HighlightColor);
			Program.WordWrap($"Do things in the world by typing in two (or so) word commands. Items and characters that you can interact with have a special color.", Program.ActionTextColor);
			Program.WordWrap($"Here are some examples:", Program.HighlightColor);
			Program.WordWrap($"To move around in the world, type 'Go' followed by the direction you want to go, like 'go north' or 'go south'.", Program.ActionTextColor);
			Program.WordWrap($"To interact with the characters you meet, try words like 'give' or 'talk'.", Program.ActionTextColor);
			Program.WordWrap($"To interact with items you see, try words like 'take' or 'use'.", Program.ActionTextColor);

			Program.WordWrap($"Some other useful commands: ", Program.HighlightColor);
			Program.WordWrap($"To see what you're carrying, type 'inventory' or 'i'.", Program.ActionTextColor);
			Program.WordWrap($"Type 'save' to save your game.", Program.ActionTextColor);
			Program.WordWrap($"Type 'load' to load a saved game.", Program.ActionTextColor);
			Program.WordWrap($"Type 'boombox' to toggle music.", Program.ActionTextColor);
			Program.WordWrap($"{ divider}", Program.HighlightColor);

		}

		/// <summary>Method run when the parser detects violence words from the parseDict Dictionaryin the user input.</summary>
		internal static void Violence()
		{
			Program.WordWrap($"Violence isn't the answer, {ToTitleCase(Program.player.Name)}.", Program.AlertColor);
		}
		/// <summary>
		/// Method run when the parser is not able to run a method based on one of the words from the parseDict Dictionary.
		/// </summary>
		internal static void BadInput()
		{
			Program.WordWrap($"Sorry, I didn't understand that. Type \"Help\" for some possible commands you can use. (BadInput)", Program.AlertColor);
		}

		// HELPER METHODS
		/// <summary>Checks the result List to determine if 'snow' and 'queen' are successive elements in the List. If so, combines them into 'snow queen' and replaces the index of 'snow' with the combination. Then deletes the 'queen' element. Needed to be able to successfully interact with the Snow Queen, since the user input is split on a whitespace character.</summary>
		/// <param name="result">The result List created in ParseInput based on the user input.</param>
		/// <returns>The result list, modified if necessary.</returns>
		internal static List<string> CheckForTheQueen(List<string> result)
		{
			int snowIndex = result.IndexOf("snow", 1);
			if (snowIndex > 0 && result[snowIndex + 1] == "queen")
			{
				result[snowIndex] = "snow queen";
				result.RemoveAt(snowIndex + 1);
			}
			return result;
		}

		/// <summary>
		/// Handle input dealing with puss in boots.
		/// </summary>
		/// <returns></returns>
		private static List<string> CheckForTheCat(List<string> result)
		{
			int catIndex = result.IndexOf("puss", 1);
			if (catIndex > 0 && result[catIndex + 1] == "boots")
			{
				result[catIndex] = "puss in boots";
				result.RemoveAt(catIndex + 1);
			}
			return result;
		}

		/// <summary>
		/// Verifies that the player has four passport stamps before they can use the boat.
		/// </summary>
		/// <returns>Whether or not victory conditions are met.</returns>
		private static bool CheckPassport()
		{
			if (Program.player.PassportStamps != 4)
			{
				return false;
			}
			if (Program.player.CurrentLocation.Name != "Cloud Dock")
			{
				Program.WordWrap($"You can't use your passport here. There's no way to travel anywhere!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Handles the special cases that occur when the player tries to climb something.
		/// </summary>
		/// <returns>A (potentially) modified result List.</returns>
		/// <param name="result">The initial result List from the ParseInput method.</param>
		private static List<string> ClimbHandler(List<string> result)
		{
			if (result[1] == "up" || result[1] == "down")
			{
				if (result.Contains("beanstalk"))
				{
					result.Remove("beanstalk");
				}
				return result;
			}
			else if (result.Contains("beanstalk"))
			{
				if (Program.player.CurrentLocation.Name == "Bottom of the Beanstalk")
				{
					return new List<string>() { "go", "up" };
				}
				else
				{
					return new List<string>() { "go", "down" };
				}
			}
			else if (result.Contains("tree") || result.Contains("trees"))
			{
				Program.WordWrap($"You don't see any trees around here that would be easy to climb.");
				return new List<string> { "pass" };
			}
			{
				Program.WordWrap($"You can't climb on that.");
				return new List<string> { "pass" };
			}

		}

		/// <summary>
		/// Handles special cases that occur when the player locks or unlocks the gate in Forest3.
		/// </summary>
		/// <returns>A modified result List.</returns>
		/// <param name="result">The initial result List from ParseInput.</param>
		private static List<string> CheckForUnlock(List<string> result)
		{

			string[] unlockWords = new string[] { "unlock", "open" };

			if (unlockWords.Contains(result[0]))
				if (Program.player.Inventory.ContainsKey("key"))
				{
					result = new List<string> { "use", "key" };
				}
				else
				{
					result[0] = "use";
				}
			return result;
		}

		// TODO: refactor to use a ternary operator, instead.
		///<summary>
		///Handles special case when the player looks at their passport so that the response is dynamic.
		///</summary> 
		private static void LookAtPassportHandler()
		{
			string text;
			switch (Program.player.PassportStamps)
			{
				case 0:
					text = $"no pages are";
					break;
				case 1:
					text = $"one page is";
					break;
				default:
					text = $"{Program.player.PassportStamps.ToString()} pages are";
					break;
			}
			Program.WordWrap($"The passport has a very nice picture of you drawn in the front, along with your name in fancy gold letters. It has space for four visa stamps inside. You leaf through the passport and notice that {text} stamped.", Program.ActionTextColor);
		}

		/// <summary>Attempts to reduce the list of strings generated from the user input into a two word command. If the second index matches an NPC, checks for a game item in the remaining indicies. If the second index matches an item, checks for an npc name in the remaining indices.</summary>
		/// <returns>A two item List if a character or item is identified past the second index, or the original List if not.</returns>
		/// <param name="result">The initial result List from ParseInput.</param>
		/// <param name="keyword">The string result from parseDict[result[0]].</param>
		private static List<string> ReduceLongInput(List<string> result, string keyword)
		{
			string[] itemActions = new string[] { "take", "use" };
			string[] npcActions = new string[] { "ask" };
			string[] playerActions = { "give", "drop" };

			if (itemActions.Contains(keyword))
			{
				for (int i = 1; i < result.Count(); i++)
				{
					if (Program.items.ContainsKey(result[i]))
					{
						return new List<string>() { keyword, result[i] };
					}
				}
			}
			else if (npcActions.Contains(keyword))
			{
				for (int i = 1; i < result.Count(); i++)
				{
					if (Program.nPCs.ContainsKey(result[i]))
					{
						return new List<string>() { keyword, result[i] };
					}
				}
			}
			else if (playerActions.Contains(keyword))
			{
				for (int i = 1; i < result.Count(); i++)
				{
					if (Program.player.Inventory.ContainsKey(result[i]))
					{
						return new List<string>() { keyword, result[i] };
					}
				}
			}
			else if (keyword == "look" || keyword == "special")
			{
				if (Program.items.ContainsKey(result[1]) || Program.nPCs.ContainsKey(result[1]) || Program.player.Inventory.ContainsKey(result[1]))
				{
					return new List<string>() { keyword, result[1] };
				}
				else
				{
					result.RemoveAt(1);
					if (result.Count() > 1)
					{
						ReduceLongInput(result, keyword);
					}
				}
			}
			else
			{
				result.RemoveAt(1);
				if (result.Count() > 1)
				{
					ReduceLongInput(result, keyword);
				}
			}
			return result;
		}

		/// <summary>Capitalize first letter of each word in a string.</summary>
		/// <param name="str">The string to be put into title case.</param>
		/// <returns>The string with the first letter of each word capitalized.</returns>
		private static string ToTitleCase(string str)
		{
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			string titleCase = textInfo.ToTitleCase(str);
			return titleCase;
		}

		/// <summary>Handles user input for specialized interactions that are available only on certain objects.</summary> 
		internal static void UseSpecialCommand(string command, string targetName)
		{
			if (Program.player.Inventory.ContainsKey(targetName))
			{
				Item target = Program.player.Inventory[targetName];
				if (target.SpecialActions.ContainsKey(command))
				{
					Program.WordWrap($"{target.SpecialActions[command]}");
				}
				else
				{
					Program.WordWrap($"You can't {command} the {target.DisplayName}.");
				}
			}
			else
			{
				if (Program.items.ContainsKey(targetName))
				{
					string displayName = Program.items[targetName].DisplayName;
					Program.WordWrap($"You don't have the {displayName}.");
				}
				else
				{
					Program.WordWrap($"There's no {targetName} here.");
				}
			}
		}

		/// <summary> Prompts the user for the answer to a yes/no question.</summary>
		/// <param name="prompt">The question to ask the user.</param>
		/// <returns></returns>
		internal static bool YesNoCheck(string prompt, ConsoleColor color)
		{
			Program.WordWrap(prompt, color);
			string response = Program.ValidateString(@" > ").ToLower();
			if (response == "y" || response == "yes")
			{
				return true;
			}
			else if (response == "n" || response == "no")
			{
				return false;
			}
			else
			{
				Program.WordWrap($"I'm sorry, I don't understand that.");
				return YesNoCheck(prompt, color);
			}
		}
	}
}


