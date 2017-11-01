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
using System.Threading;

namespace unitTestAdventure
{
	/// <summary>
	/// Test binary serialization and deserialization of objects.
	/// </summary>
	[TestClass()]
	public class SerializationUnitTests
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

			Program.locations = locations;
			Program.items = items;
			Program.nPCs = npcs;

			// create new Player and make it Program.player
			Dictionary<string, string> specialActions = new Dictionary<string, string>() { { "dance", "No dancing!" } };

			Item cellPhone = new Item("cell phone", "Cell Phone", "Initial cell phone text", "Describe cell phone", "Ice Castle", "Snow Queen", specialActions);

			Dictionary<string, Item> inventory = new Dictionary<string, Item>();
			inventory.Add("cellphone", cellPhone);

			Program.player = new Player("Anna", "0", "Start", inventory, true, true, "0", "0", true);

		}

		/// <summary>Test if an item serializes by checking that a file exists in the appropriate location.</summary>
		[TestMethod()]
		public void TestItemSerialization()
		{
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\" + items["ribbon"].Name + @".bin";
			BinarySerializer.WriteToFile(filePath, items["ribbon"]);

            Assert.IsTrue(File.Exists(filePath));
        }

        [TestMethod]
		public void TestItemDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\ribbon.bin";

			Item ribbon = BinarySerializer.ReadFromFile<Item>(filePath);

			Assert.IsInstanceOfType(ribbon, typeof(Item));
		}

		[TestMethod]
		/// <summary>Test if a location serializes by checking that a file exists in the appropriate location.</summary>
		public void TestLocationSerialization()
		{
			Location location = locations["Start"];

			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\location.bin";
			BinarySerializer.WriteToFile(filePath, location);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a single Location.
		/// </summary>
		[TestMethod]
		public void TestLocationDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\location.bin";

			Location location = BinarySerializer.ReadFromFile<Location>(filePath);

			Assert.IsInstanceOfType(location, typeof(Location));
		}

		/// <summary>
		/// Test serialization of a single NPC.
		/// </summary>
		[TestMethod]
		public void TestNPCSerialization()
		{
			NPC aladdin = npcs["aladdin"];

			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\aladdin.bin";
			BinarySerializer.WriteToFile(filePath, aladdin);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a single NPC.
		/// </summary>
		[TestMethod]
		public void TestNPCDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\aladdin.bin";

			NPC aladdin = BinarySerializer.ReadFromFile<NPC>(filePath);

			Assert.IsInstanceOfType(aladdin, typeof(NPC));
			Assert.AreEqual(aladdin.Name, "aladdin");
			Assert.AreEqual(aladdin.GainHeroItemText["'You found my lamp! Thank you so much!'"], "green");
		}

		/// <summary>
		/// Test serialization of a Dictionary of Locations.
		/// </summary>
		[TestMethod]
		public void TestLocationCollectionSerialization()
		{
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\locations.bin";
			BinarySerializer.WriteToFile(filePath, locations);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a Dictionary of Locations.
		/// </summary>
		[TestMethod]
		public void TestLocationCollectionDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\locations.bin";

			Dictionary<string, Location> locations = BinarySerializer.ReadFromFile<Dictionary<string, Location>>(filePath);

			Assert.IsInstanceOfType(locations, typeof(Dictionary<string, Location>));
			Assert.AreEqual(locations["Azure Bayou"].WelcomeMsg, "Welcome to the Azure Bayou");
		}
		/// <summary>
		/// Test serialization of a Dictionary of Items.
		/// </summary>
		[TestMethod]
		public void TestItemCollectionSerialization()
		{
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\items.bin";
			BinarySerializer.WriteToFile(filePath, items);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a Dictionary of Items.
		/// </summary>
		[TestMethod]
		public void TestItemCollectionDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\items.bin";

			Dictionary<string, Item> items = BinarySerializer.ReadFromFile<Dictionary<string, Item>>(filePath);

			Assert.IsInstanceOfType(items, typeof(Dictionary<string, Item>));
			Assert.AreEqual(items["carrot"].InitialText, " A big orange {carrot} is laying on the ground.");
		}

		/// <summary>
		/// Test serialization of a Dictionary of NPCs
		/// </summary>
		[TestMethod]
		public void TestNPCCollectionSerialization()
		{
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\characters.bin";
			BinarySerializer.WriteToFile(filePath, npcs);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a Dictionary of NPCs.
		/// </summary>
		[TestMethod]
		public void TestNPCCollectionDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\characters.bin";

			Dictionary<string, NPC> characters = BinarySerializer.ReadFromFile<Dictionary<string, NPC>>(filePath);

			Assert.IsInstanceOfType(characters, typeof(Dictionary<string, NPC>));
			Assert.AreEqual(characters["crone"].GainHeroItemText["'The cards say that you have a new stamp added to your passport.'"], "green");
		}

		/// <summary>
		/// Test serialization of a Player Object
		/// </summary>
		[TestMethod]
		public void TestPlayerSerialization()
		{
			string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\" + Program.player.Name + ".bin";
			BinarySerializer.WriteToFile(filePath, Program.player);

			Assert.IsTrue(File.Exists(filePath));
		}

		/// <summary>
		/// Test deserialization of a player object.
		/// </summary>
		[TestMethod]
		public void TestPlayerDeserialization()
		{
            Thread.Sleep(500); // Allows time for serialization test to write a file.
            string basePath = Directory.GetCurrentDirectory();
			string filePath = basePath + @"\TestData\anna.bin";
			Player anna = BinarySerializer.ReadFromFile<Player>(filePath);

			Assert.IsInstanceOfType(anna, typeof(Player));
		}

		/// <summary>Test serialization of a GameWorld object.</summary>
		[TestMethod]
		public void TestGameWorldSerialization()
		{
			Thread.Sleep(500); // Allows time for serialization test to write to file.
		}
	}
}
