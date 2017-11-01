using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TextAdventure;
using TextAdventure.Items;
using TextAdventure.Locations;
using TextAdventure.NPCs;
using static System.Console;

namespace unitTestAdventure
{
	/// <summary>
	/// Test for instantiation from JSON files and contents of those files once instantiated.
	/// </summary>
	[TestClass]
	public class JSONUnitTests
	{
		Dictionary<string, Location> locations;
		Dictionary<string, Item> items;
		Dictionary<string, NPC> npcs;

		/// <summary>
		/// Create game-related objects needed for tests.
		/// </summary>
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

		/// <summary> Tests that Locations load from JSON. </summary>
		[TestMethod]
		public void TestLocationLoad()
		{
			Assert.IsInstanceOfType(locations, typeof(Dictionary<string, Location>));
			Assert.AreEqual(25, locations.Count);
			Assert.AreEqual("Agrahar Market", locations["Agrahar Market"].Name);
			Assert.AreEqual("Watch out! Don't walk into the swamp.", locations["Azure Bayou"].DangerMsg);
			Assert.AreEqual("Welcome to Charming Cottage", locations["Charming Cottage"].WelcomeMsg);
			Assert.AreEqual("You are in a deep forest. Tall trees rise up majestically around you, so tall they seem to brush the sky. Birds are singing musically above you. Shafts of bright sunlight break through the trees here and there, illuminating your path.", locations["Forest1"].IntroMsg);
			Assert.AreEqual("The forest is thinner here. You see a village in the distance to the west.", locations["Forest2"].Description);
			Assert.AreEqual(0, locations["Forest3"].XCoord);
			Assert.AreEqual(2, locations["Forest4"].YCoord);

            Assert.AreEqual(Path.Combine(Directory.GetCurrentDirectory(), "../../MidiMusic", "Start.mid"), locations["Start"].MidiFilePath);
		}

		/// <summary> Tests that NPCs load from JSON. </summary>
		[TestMethod]
		public void TestNPCLoad()
		{
			Assert.IsInstanceOfType(npcs, typeof(Dictionary<string, NPC>));
		}

		/// <summary>Tests that Items load from JSON.</summary>
		[TestMethod]
		public void TestItemLoad()
		{
			Assert.IsInstanceOfType(items, typeof(Dictionary<string, Item>));
		}
	}
}
