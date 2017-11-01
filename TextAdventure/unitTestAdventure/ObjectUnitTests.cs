using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventure;
using TextAdventure.Items;
using TextAdventure.Locations;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using static System.Console;
using TextAdventure.NPCs;
using System.Linq;
using unitTestAdventure.ConsoleRedirect;

namespace TextAdventure.ObjectUnitTests
{
	[TestClass]
	public class ObjectUnitTests
	{
		Dictionary<string, Location> locations;
		Dictionary<string, Item> items;
		Dictionary<string, NPC> npcs;

		[TestInitialize]
		public void CreateObjects()
		{
			string basePath = Directory.GetCurrentDirectory() + @"/../../../TextAdventure/Data";
			string jsonLocationFile = Path.Combine(basePath, "locations.json");
			string jsonItemFile = Path.Combine(basePath, "Items.json");
			string jsonNPCFile = Path.Combine(basePath, "NPCs.json");
			string jsonPlayerFile = Path.Combine(basePath, "Players.json");

			locations = JsonConvert.DeserializeObject<List<Location>>(File.ReadAllText(jsonLocationFile)).ToDictionary(location => location.Name);

			items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(jsonItemFile)).ToDictionary(item => item.Name);

			npcs = JsonConvert.DeserializeObject<List<NPC>>(File.ReadAllText(jsonNPCFile)).ToDictionary(nPC => nPC.Name);
		}

		/// <summary> This blank test ensures that the unit test .cs and project are connected to the TextAdventure project. </summary>
		[TestMethod]
		public void TestUnitTestProjectConnection()
		{
		}

		/// <summary> Check that the Item constructor correctly creates a new Item object.</summary>
		[TestMethod]
		public void TestItemInstantiation()
		{
			Item item = new Item("An item", "A description of an item", "The very beginning", "Look! It's an item!", "At the beginning", "item target", null);
			Assert.IsInstanceOfType(item, typeof(Item));
		}

        /// <summary> Check that the NPC constructor correctly creates a new NPC object. </summary>
        [TestMethod]
        public void TestNPCInstantiation()
        {
            NPC npc = new NPC("Aladdin", "Agrahar Market", "My before description", "My after description", new Dictionary<string, string> {{ "My look before", "red" }}, new Dictionary<string, string> { { "My look after", "yellow" } }, new Dictionary<string, string> { { "My before talk", "green" } }, new Dictionary<string, string> { { "My after talk", "cyan" } }, false, new Dictionary<string, string>() { { "Thanks!", "green" }, { "More thanks!", "cyan" } });
			Assert.IsInstanceOfType(npc, typeof(NPC));
		}

		/// <summary>
		/// Testing the class that Dan Muldrew wrote to test Console output to make sure it's functioning properly. Test was written by Dan.
		/// </summary>
		[TestMethod()]
		public void RedirectTest()
		{
			using (ConsoleRedirector redirect = new ConsoleRedirector())
			{
				Console.Write("my awesome output");
				redirect.SetUserInput("my awesome input");
				string answer = Console.ReadLine();
				Assert.AreEqual<string>("my awesome input", answer);
				Assert.AreEqual<string>("my awesome output", redirect.ToString());
			}
		}

        /// <summary> Check that the Location constructor correctly creates a new Location object.</summary>
		[TestMethod]
        public void TestLocationInstantiation()
        {
            Location location = new Location("Start", null, "Welcome to the beginning", "You are here for the first time", "You are on a hill", "You are in a clearing", 0, 0, false, true, "startMusic");
            Assert.IsInstanceOfType(location, typeof(Location));
        }
    } // End class ObjectUnitTests
}
