using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextAdventure.Locations;
using TextAdventure.Items;
using TextAdventure.NPCs;
using static System.Console;

namespace TextAdventure
{
	[Serializable]
	/// <summary>Controls creation of the GameWorld that that is serialized when a game is saved, and controls the saving and loading of games.</summary>
	public class GameWorld
	{
		internal string Name { get; set; }
		internal Player Player { get; set; }
		internal Dictionary<string, Location> Locations { get; set; }
		internal Dictionary<string, Item> Items { get; set; }
		internal Dictionary<string, NPC> Npcs { get; set; }

		GameWorld(Player player, Dictionary<string, Location> locations, Dictionary<string, Item> items, Dictionary<string, NPC> npcs)
		{
			Name = player.Name;
			Player = player;
			Locations = locations;
			Items = items;
			Npcs = npcs;
		}

		/// <summary>
		/// Saves the game: Creates a gameworld object from the currently running gave and saves it to the SavedGames folder.</summary>
		internal static void SaveGame()
		{
			string directory = Directory.GetCurrentDirectory();
			string[] splitStr = new string[] { @"bin" };
			StringSplitOptions options = new StringSplitOptions();
			string path = directory.Split(splitStr, options)[0];
			path = Path.Combine(path, @"Release\");

			GameWorld gameToSave = new GameWorld(Program.player, Program.locations, Program.items, Program.nPCs);

			Directory.CreateDirectory(path + @"SavedGames");

			string gameFile = path + @"SavedGames\" + gameToSave.Name.ToLower() + ".bin";

			bool previousGame = false;

			if (File.Exists(gameFile))
			{
				ForegroundColor = Program.AlertColor;
				bool response = Parser.YesNoCheck($" A saved game already exists with the name '{gameToSave.Name}', and will be overwritten. Are you sure you want to continue?  y/n", Program.AlertColor);
				if (response)
				{
					previousGame = false;

				}
				else
				{
					previousGame = true;
					ResetColor();
					Program.WordWrap($"Save game cancelled.");
				}
			}

			if (!previousGame)
			{
				try
				{
					BinarySerializer.WriteToFile(gameFile, gameToSave);
					Program.WordWrap($"You find yourself drowsy and search for a place to lay your head. A soft pile of hay becons with golden fingers. 'Goodnight, {gameToSave.Name},' it whispers. Your game has been saved!", Program.ConfirmationColor);
					ResetColor();
				}
				catch (Exception)
				{
					Program.WordWrap("A curse has been cast over your adventure. A place to save your game could not be found!", Program.AlertColor);
					ResetColor();
					ReadKey();
				}
			}
		}

		/// <summary>Deserializes a GameWorld object and sets the values of the running game to the GameWorld object's values, based on the game a player selects to reload.</summary>
		internal static void LoadGame()
		{
			string directory = Directory.GetCurrentDirectory();
			string[] splitStr = new string[] { @"bin" };
			StringSplitOptions options = new StringSplitOptions();
			string path = directory.Split(splitStr, options)[0];
			path = Path.Combine(path, @"Release\");

			Directory.CreateDirectory(path + @"SavedGames");

			string pathName = path + @"SavedGames\";
			string[] savedGames = Directory.GetFiles(pathName);
			string[] splitStrgame = new string[] { @"\SavedGames\" };

			if (savedGames.Length > 0)
			{
				Program.WordWrap("You have the following saved games:");
				foreach (string game in savedGames)
				{
					Program.WordWrap(
						game.Split(splitStrgame, options)[1].
						Split(new char[] { '.' })[0].PadLeft(2)
						);
				}

				string selection = Program.ValidateString(" Enter the name of the game you want to load.\n > ");

				// NOTE: Make sure when a game is saved, the saved game is all lower case.
				string filePath = pathName + selection.ToLower() + @".bin";

				if (File.Exists(filePath))
				{
					GameWorld reloadedGame = BinarySerializer.ReadFromFile<GameWorld>(filePath);
					Program.player = reloadedGame.Player;
					Program.locations = reloadedGame.Locations;
					Program.items = reloadedGame.Items;
					Program.nPCs = reloadedGame.Npcs;
					Program.WordWrap("Game loaded! Press any key to continue your current game", Program.DialogColor);
					ReadKey();
					Program.DisplayTitle(Program.player);
					Program.player.CurrentLocation.DisplayHeaders(Program.player);
				}
				else
				{
					Program.WordWrap("That's not a valid saved game. Press any key to continue your current game.", Program.AlertColor);
					ReadKey();
				}
			}
			else
			{
				Program.WordWrap($"You don't have any saved games to load!", Program.AlertColor);
			}
		}
	}
}
