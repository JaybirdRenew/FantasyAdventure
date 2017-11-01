using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TextAdventure;
using TextAdventure.Items;
using TextAdventure.Locations;
using static System.Console;
using TextAdventure.NPCs;
using System.Linq;
using unitTestAdventure.ConsoleRedirect;

namespace unitTestAdventure
{
	/// <summary>
	/// Tests the functionality of the Parser class.
	/// </summary>
	[TestClass]
	public class ParserUnitTests
	{
		List<string> oneWordInputs;
		List<string> twoWordInputs;
		Dictionary<string, string> parseDict;

		/// <summary>
		/// Create needed objects to test the function of the parser.
		/// </summary>
		[TestInitialize]
		public void CreateObjects()
		{
            Program.GetWindowWidth = () => 500;

            string basePath = Directory.GetCurrentDirectory() + @"/../../../TextAdventure/Data";
			string parseFile = Path.Combine(basePath, "parseDictionary.json");
			parseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(parseFile));

			Program.parseDict = parseDict;

			oneWordInputs = new List<string>()
			{
				"", // empty string
				"about", // about case
				"again", // again case
				"music", // boombox case
				"quit", // exit case
				"reload", // load case
				"examine", // look case
				"help", // help case
				"inventory", // inventory case
				"bitch", // naughty case (one word)
				"save", // save case
				"pass", // pass case
				"kibble" // default case (bad input)
			};

			twoWordInputs = new List<string>()
			{
				"talk snow queen", // ask case; snow queen handler; ask not present character
				"talk bigfoot", // non-existing character
				"drop passport", // drop item; drop item not held
				"drop kibble", // drop non-existing item;
				"give tarot", // give case; give to wrong character; 
				"go north", // go case; go to unavailable location; go to non-existing location
				"look crone", // look case at NPC; look at non-present character.
				"look passport", // look case at item; passport look handler
				"look bigfoot", // non-existing look object
				"go shit", // naughty case
				"dance broom", // special case
				"dance tarot", // special case, not target object
				"take tarot", // take case, take case item not present, take case have item already
				"use passport", // passport checker, game over
				"use key", // unlock checker, use case item not in correct location
				"use carrot", // use case no target
				"hit aladdin", // violence case
				"kiss bigfoot", // default case (bad input)
			};

			// create object dictionaries for the program and set them.
			string jsonLocationFile = Path.Combine(basePath, "locations.json");
			string jsonItemFile = Path.Combine(basePath, "Items.json");
			string jsonNPCFile = Path.Combine(basePath, "NPCs.json");
			string jsonPlayerFile = Path.Combine(basePath, "Players.json");

			Program.locations = JsonConvert.DeserializeObject<List<Location>>(File.ReadAllText(jsonLocationFile)).ToDictionary(location => location.Name);

			Program.items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(jsonItemFile)).ToDictionary(item => item.Name);

			Program.nPCs = JsonConvert.DeserializeObject<List<NPC>>(File.ReadAllText(jsonNPCFile)).ToDictionary(nPC => nPC.Name);

			// create new Player and make it Program.player
			Dictionary<string, string> specialActions = new Dictionary<string, string>() { { "dance", "No dancing!" } };

			Item cellPhone = new Item("cell phone", "Cell Phone", "Initial cell phone text", "Describe cell phone", "Ice Castle", "Snow Queen", specialActions);

			Dictionary<string, Item> inventory = new Dictionary<string, Item>();
			inventory.Add("cellphone", cellPhone);

			Program.player = new Player("Anna", "0", "Start", inventory, true, true, "0", "0", true);

		}

		/// <summary>
		/// Test the regex validator
		/// </summary>
		[TestMethod]
		public void TestIsValid()
		{
			bool result1 = Parser.IsValid(twoWordInputs[0]);
			bool result2 = Parser.IsValid(oneWordInputs[0]);
			bool result3 = Parser.IsValid(@"fish-sticks and pie!!");

			Assert.AreEqual(result1, true);
			Assert.AreEqual(result2, false);
			Assert.IsFalse(result3);
		}

		/// <summary>
		/// Test method that insures that "snow" and "queen" will be combined if found.
		/// </summary>
		[TestMethod]
		public void TestCheckForTheQueen()
		{
			List<string> result1 = Parser.CheckForTheQueen(new List<string>() { "test", "for", "the", "snow", "queen" });

			Assert.IsTrue(result1.Contains("snow queen"));
			Assert.IsFalse(result1.Contains("snow"));

			List<string> result2 = Parser.CheckForTheQueen(new List<string>() { "test", "for", "the", "missing", "queen" });

			Assert.AreEqual(5, result2.Count);
		}

		// ONE WORD COMMAND TESTS

		/// <summary>
		/// Test parser against empty string input.
		/// </summary>
		[TestMethod]
		public void TestParseInputEmptyString()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
                redirect.SetUserInput(oneWordInputs[0]);
                Parser.ParseInput(oneWordInputs[0]);
				Assert.AreEqual<string>(" Sorry, I didn't understand that. Type \"Help\" for some possible commands you can use. (BadInput)", redirect.ToString());
			}
		}

		/// <summary>
		/// Test parser single input about case. 
		/// </summary>
		[TestMethod]
		public void TestParseInputAboutCase()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(oneWordInputs[1]);
				redirect.SetUserInput(oneWordInputs[1]);
				Assert.IsTrue(redirect.ToString().Contains("****************************"));
			}
        }

		//// TODO: figure out how to handle multiple inputs.
		/////<summary> Test parser again case. </summary>
		//[TestMethod]
  //      public void TestParseInputCaseAgain()
  //      {
  //          using (ConsoleRedirector redirect = new ConsoleRedirector())
  //          {
  //              Parser.ParseInput(oneWordInputs[2]);
  //              redirect.SetUserInput(oneWordInputs[2]);
  //              WriteLine("y");
  //              ReadLine();
  //              Assert.AreEqual<string>("Sorry, I didn't understand that. Type \"Help\" for some possible commands you can use. (BadInput)", redirect.ToString());
  //          }
  //      }
  
		/// <summary>
		/// Test parser single input boombox case, both turning music off and turning it on.
		/// </summary>
        [TestMethod]
        public void TestParseInputBoomboxCase()
        {
            using (ConsoleRedirector redirect = new ConsoleRedirector())
            {
                Parser.ParseInput(oneWordInputs[3]);
                redirect.SetUserInput(oneWordInputs[3]);

                Assert.AreEqual<string>(" Sweet silence now reigns...", redirect.ToString());
                Assert.IsFalse(Program.player.WantsMusic);

            }

            using (ConsoleRedirector redirect = new ConsoleRedirector())
            {
                Parser.ParseInput(oneWordInputs[3]);
                redirect.SetUserInput(oneWordInputs[3]);

                Assert.AreEqual<string>(" Let the music play!", redirect.ToString());
                Assert.IsTrue(Program.player.WantsMusic);
                Parser.ParseInput(oneWordInputs[3]); // Stops the music from playing!
            }
        }

        ///<summary>
        ///Test parser single input look case.</summary>
        [TestMethod]
		public void TestParseInputLookCaseSingleInput()
		{
			Program.player.CurrentLocation = Program.locations["Agrahar Market"];

			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(oneWordInputs[6]);
				redirect.SetUserInput(oneWordInputs[6]);
				Assert.IsTrue(redirect.ToString().Contains("You are in a bustling market,"));
			}
		}

		///<summary>
		///Test parser single input help case.</summary>
		[TestMethod]
		public void TestParseInputHelpCase()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(oneWordInputs[7]);
				redirect.SetUserInput(oneWordInputs[7]);
				Assert.IsTrue(redirect.ToString().Contains("Do things in the world by typing in two (or so) word commands."));
			}
		}

		///<summary>
		///Test parser inventory case.</summary>
		[TestMethod]
		public void TestParseInputInventoryCase()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(oneWordInputs[8]);
				redirect.SetUserInput(oneWordInputs[8]);
				Assert.IsTrue(redirect.ToString().Contains("Your current inventory:"));
				Assert.AreEqual(" Your current inventory: \r\n\r\n Cell Phone\r\n\r\n Passport", redirect.ToString());
			}
		}

		///<summary>
		///Test parser single input help case.</summary>
		[TestMethod]
		public void TestParseInputNaughtyCase()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(oneWordInputs[9]);
				redirect.SetUserInput(oneWordInputs[9]);
				Assert.AreEqual(" That's not appropriate. Your fairy godmother judges you.", redirect.ToString());
			}
		}

		// TODO: figure out how to deal with multiple inputs and implement test for save.

		// TWO WORD COMMAND TESTS

		/// <summary>
		/// Test 'ask' case with existing but not present character, with present character, and with non-existing character.
		/// </summary>
		[TestMethod]
		public void TestParseInputAskCase()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(twoWordInputs[0]);
				redirect.SetUserInput(twoWordInputs[0]);

				Assert.AreEqual(" You don't see Snow Queen here.", redirect.ToString());
			}

			Program.player.CurrentLocation = Program.locations["Ice Castle"];

			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput(twoWordInputs[0]);
				redirect.SetUserInput(twoWordInputs[0]);

				Assert.IsTrue(redirect.ToString().Contains("Welcome to my castle!"));
			}

			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Parser.ParseInput($"ask bigfoot");
				redirect.SetUserInput($"ask bigfoot");

				Assert.AreEqual($" Who's that? I don't recognise that name", redirect.ToString());
			}
		}
	}

}
