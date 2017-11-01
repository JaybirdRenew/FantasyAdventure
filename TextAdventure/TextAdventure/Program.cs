using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Media;
using TextAdventure.Locations;
using TextAdventure.Items;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Console;
using Newtonsoft.Json;
using TextAdventure.NPCs;
using System.Runtime.InteropServices;

namespace TextAdventure
{
	class Program
	{
        /// <summary> These static collections and a static player allow them to be called from any class. </summary>
        internal static Dictionary<string, Location> locations;
		public Dictionary<string, Location> Locations { get; set; }

		internal static Dictionary<string, Item> items;
		internal static Dictionary<string, NPC> nPCs;
		internal static Dictionary<string, Player> players;
		internal static Dictionary<string, string> parseDict;
		internal static Player player;
		public Player Player { get; set; }

		// TODO: Make all color assignments conform to these so it's easier to change colors of game text globally.
		internal static ConsoleColor ActionTextColor { get; set; } = ConsoleColor.Cyan;
		internal static ConsoleColor DialogColor { get; set; } = ConsoleColor.Green;
        internal static ConsoleColor AlertColor { get; set; } = ConsoleColor.Red;
		internal static ConsoleColor ConfirmationColor { get; set; } = ConsoleColor.Magenta;
		internal static ConsoleColor HighlightColor { get; set; } = ConsoleColor.Yellow;
		internal static ConsoleColor SubtitleColor { get; set; } = ConsoleColor.Magenta;
		internal static ConsoleColor BoldTextColor { get; set; } = ConsoleColor.White;

        internal static Func<int> GetWindowWidth = () => Console.WindowWidth;

		/// <summary> Game starts here. </summary>
		internal static void Main(string[] args)
		{
			WindowHeight = LargestWindowHeight - 10;
			WindowWidth = LargestWindowWidth - 10;
			Title = "Fantasy Game";
			LoadGameData();
			player = Welcome(players);
			Play();
			ReadKey(); // Pauses the program so that user/dev can read console.
		}

        // TODO: Move to player object.
        /// <summary> Togggles the music on or off. </summary>
        internal static void ToggleMusic()
        {
            if (player.WantsMusic)
            {
                player.WantsMusic = false;
                player.CurrentLocation.StopMusic();
                WordWrap("Sweet silence now reigns...");                               
            }
            else
            {
                player.WantsMusic = true;
                player.CurrentLocation.PlayMusic();
                WordWrap("Let the music play!");                
            }
        }

        /// <summary> Gets a name for the user and welcomes them. </summary>
        private static Player Welcome(Dictionary<string, Player> players)
		{
			DisplayTitle(players["Current Player"]);
			string playerName = AskName();
			players["Current Player"].Name = playerName;
			Program.UpdateHeader(players["Current Player"]);
			WordWrap($"{players["Current Player"].CurrentLocation.WelcomeMsg}!", SubtitleColor);
			players["Current Player"].CurrentLocation.DisplayLocation(players["Current Player"]);
			return players["Current Player"];
		}

		/// <summary> Loads info from .json files and music files. Music must be loaded separately. </summary>
		public static void LoadGameData()
		{
			string basePath = Directory.GetCurrentDirectory();
			string jsonLocationFile = basePath + @"../../../Data/locations.json";
			string jsonItemFile = basePath + @"../../../Data/Items.json";
			string jsonNPCFile = basePath + @"../../../Data/NPCs.json";
			string jsonPlayerFile = basePath + @"../../../Data/Players.json";
			string parseFile = basePath + @"../../../Data/parseDictionary.json";

			locations = JsonConvert.DeserializeObject<List<Location>>(File.ReadAllText(jsonLocationFile)).ToDictionary(location => location.Name);
			items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(jsonItemFile)).ToDictionary(item => item.Name);
			nPCs = JsonConvert.DeserializeObject<List<NPC>>(File.ReadAllText(jsonNPCFile)).ToDictionary(nPC => nPC.Name);
			players = JsonConvert.DeserializeObject<List<Player>>(File.ReadAllText(jsonPlayerFile)).ToDictionary(player => player.Name);

			parseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(parseFile));

			// Put Jack in a random starting location in the forest
			Random rn = new Random();
			int randomJack = rn.Next(1, 16);
			string jacksPlace = "Forest" + randomJack.ToString();
			nPCs["jack"].Location = jacksPlace;

	    } // End LoadGameData()

		/// <summary> This method is called at the end of the game. </summary>
		internal static void GameEnd(Player player)
		{
			About(player.Name);
			WordWrap("Type 'exit' to exit and 'again' to play again.", ConfirmationColor);
			Parser.ParseInput(ValidateString(" > "));
		}

		/// <summary> This method is called to ask the user's name. </summary>
		public static string AskName()
		{
			return ValidateString(" Enter your name: ");
		}

        /// <summary> This method validates user input for a string. It reprompts the user until the string is non-null and non-empty. </summary>
        public static string ValidateString(string prompt)
        {
            string input = string.Empty;

            do
            {
                Write(prompt);

                try
                {
                    input = ReadLine().Trim();
                }
                catch (NullReferenceException)
                {
                    input = string.Empty;
                }
                catch (Exception)
                {
                    input = string.Empty;
                }
            } while (input.Length < 1 || input.Length > 50);

            return input;
        }

        /// <summary> This method contains the play loop of the game. </summary>
        public static void Play()
		{
			while (Program.player.IsPlaying)
			{
				Parser.ParseInput(ValidateString(" What would you like to do?\n > "));
				Program.player.CommandCount++;
                UpdateHeader(Program.player);
			}
		} // End Play()

		/// <summary> Default method that makes text white. </summary>
		public static void WordWrap(string paragraph)
		{
			WordWrap(paragraph, BoldTextColor);
		}

        /// <summary> 
        /// Overload of WorldWrap() above. 
        /// Called so that lines do not get wrapped in the middle of a word. 
        /// Adapted from https://rianjs.net/2016/03/line-wrapping-at-word-boundaries-for-console-applications-in-csharp
        /// </summary>
        public static void WordWrap(string str, ConsoleColor paragraphColor)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return;
            }

            string[] splitOn = { $"\n\n" };
            string[] splitParagraph = str.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);

            foreach (string para in splitParagraph)
            {
                int approxLineCount = para.Length / GetWindowWidth() - 1;
                StringBuilder lines = new StringBuilder(para.Length + (approxLineCount * 4));

                for (var i = 0; i < para.Length;)
                {
                    int grabLimit = Math.Min(GetWindowWidth() - 1, para.Length - i);
                    string line = para.Substring(i, grabLimit);
                    string keywordSeparator = "{";

                    bool isLastChunk = grabLimit + i == para.Length;

                    if (isLastChunk && line.Contains(keywordSeparator))
                    {
                        i = i + grabLimit;
                        
                        SplitByKeyword($" {line}", paragraphColor);
                    }
                    else if (isLastChunk) // && line does not contain "{"
                    {
                        i = i + grabLimit;
                        ColorizeLine(line.PadLeft(1), paragraphColor);
                    }
                    else if (line.Contains(keywordSeparator)) // && is not last chunck
                    {
                        int lastSpace = line.LastIndexOf(" ", StringComparison.Ordinal);
                        // Line needs a break after lastSpace
                        line = line.Substring(0, lastSpace) + "\n";
                        
                        SplitByKeyword($" {line}", paragraphColor);

                        //Trailing spaces needn't be displayed as the first character on the new line
                        i = i + lastSpace + 1;
                    }
                    else // is not last chunk && line does not contain "{" 
                    {
                        int lastSpace = line.LastIndexOf(" ", StringComparison.Ordinal);
                        ColorizeLine(line.Substring(0, lastSpace), paragraphColor);
                        i = i + lastSpace + 1;
                    } // End if/else
                } // End for loop
				WriteLine();
			} // End foreach loop
		} // End WordWrap()

		/// <summary> This method splits a line by its key words. Key words are located throughout .json files, delimited by {}.
		private static void SplitByKeyword(string line, ConsoleColor paragraphColor)
        {
            char[] separators = { '{', '}' };
            string[] splitLine = line.Split(separators);
            splitLine[0].PadLeft(1);

            for (int piece = 0; piece < splitLine.Length; ++piece)
            {
                if (piece % 2 == 1) // odd-indexed piece 
                {
                    ColorizePiece(splitLine[piece], HighlightColor);
                }
                else // don't keyword-color the even-indexed pieces, just write 'em with paragraph color.
                {
                    ColorizePiece(splitLine[piece], paragraphColor);
                }
            }
        }

        /// <summary> Writes line with color </summary>
        private static void ColorizeLine(string line, ConsoleColor color)
        {
            ForegroundColor = color;
			WriteLine($" {line}");
            ResetColor();
        }

        /// <summary> Colors key words so that the user can discover them! </summary>
        private static void ColorizePiece(string line, ConsoleColor color)
        {
            ForegroundColor = color;
            Write(line);
            ResetColor();
        }

        /// <summary> 
        /// Called at certain locations. 
        /// TODO: Make this method not depend on a player!
        /// </summary>
        public static void DisplayTitle(Player player)
		{
			Clear();
			UpdateHeader(player);

			string title = @"
   .-.                          ___                                                                                  
  /    \                       (   )                                                                                 
  | .`. ;    .---.   ___ .-.    | |_       .---.      .--.     ___  ___      .--.     .---.   ___ .-. .-.     .--.   
  | |(___)  / .-, \ (   )   \  (   __)    / .-, \   /  _  \   (   )(   )    /    \   / .-, \ (   )   '   \   /    \  
  | |_     (__) ; |  |  .-. .   | |      (__) ; |  . .' `. ;   | |  | |    ;  ,-. ' (__) ; |  |  .-.  .-. ; |  .-. ; 
 (   __)     .'`  |  | |  | |   | | ___    .'`  |  | '   | |   | |  | |    | |  | |   .'`  |  | |  | |  | | |  | | | 
  | |       / .'| |  | |  | |   | |(   )  / .'| |  _\_`.(___)  | '  | |    | |  | |  / .'| |  | |  | |  | | |  |/  | 
  | |      | /  | |  | |  | |   | | | |  | /  | | (   ). '.    '  `-' |    | |  | | | /  | |  | |  | |  | | |  ' _.' 
  | |      ; |  ; |  | |  | |   | ' | |  ; |  ; |  | |  `\ |    `.__. |    | '  | | ; |  ; |  | |  | |  | | |  .'.-. 
  | |      ' `-'  |  | |  | |   ' `-' ;  ' `-'  |  ; '._,' '    ___ | |    '  `-' | ' `-'  |  | |  | |  | | '  `-' / 
 (___)     `.__.'_. (___)(___)   `.__.   `.__.'_.   '.___.'    (   )' |     `.__. | `.__.'_. (___)(___)(___) `.__.'  
                                                                ; `-' '     ( `-' ;                                  
                                                                 .__.'       `.__.                                   
";
			ForegroundColor = HighlightColor;
			WriteLine(title);
			ResetColor();
		}

		/// <summary> Header gets updated with music/no music, player name, visa count, move count, command count, inventory count. </summary>
		public static void UpdateHeader(Player player)
		{
			int top = CursorTop;
			int left = CursorLeft;
			SetCursorPosition(0, 0);
			BackgroundColor = ConsoleColor.DarkMagenta;
			ForegroundColor = BoldTextColor;
			string musicStatus = "On";
			if (!(player.WantsMusic))
			{
				musicStatus = "Off";
			}
			string headerText = $" Player: {player.Name}      Visa Count: {player.PassportStamps.ToString()}      Moves: {player.MoveCount.ToString()}      Commands: {player.CommandCount.ToString()}      Inventory Items: {player.Inventory.Count}      Music: {musicStatus}";
			WriteLine(headerText.PadRight(WindowWidth));
			ResetColor();
			SetCursorPosition(left, top);
		}

		/// <summary> This method gets called at the end of the game or when user enters "about". </summary>
		internal static void About(string name)
		{
			WordWrap($"The authors of this game congratulate you for winning, {name}!", HighlightColor);

			WordWrap("*****************************************************************", ConfirmationColor);
			WordWrap("Version 2 by the Jaybird Renew Team:", ActionTextColor);
			WordWrap("Sara Jade:      https://www.linkedin.com/in/sara-jade/", ActionTextColor);
			WordWrap("Erica Winberry: https://www.linkedin.com/in/ericawinberry/", ActionTextColor);
			WriteLine();
			WordWrap("Special thanks to the original creators:", ActionTextColor);
			WordWrap("Alana Franklin: https://www.linkedin.com/in/alanafranklin/", ActionTextColor);
			WordWrap("Ashwini Rao:    https://github.com/orgs/Calz-One/people/ashkrao", ActionTextColor);
			WordWrap("Drew Biehle:    https://www.linkedin.com/in/drew-biehle-844a2b15/", ActionTextColor);
			WordWrap("La Januari:     https://www.linkedin.com/in/%E0%BD%A3januari/", ActionTextColor);
			WordWrap("*****************************************************************", ConfirmationColor);
		} // End About()
	} // End class Program
}
