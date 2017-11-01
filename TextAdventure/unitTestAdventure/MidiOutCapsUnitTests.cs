using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure;
using TextAdventure.Locations;

namespace unitTestAdventure
{
    [TestClass]
    public class MidiOutCapsUnitTests
    {
        [TestMethod]
        public void TestMci()
        {
            string command = "close music";
            Assert.AreEqual(string.Empty, MidiOutCaps.Mci(command));

            command = "open C:\\Users\\wisco\\Source\\Repos\\JayBirdRenew\\TextAdventure\\TextAdventure\\bin\\Debug\\../../MidiMusic\\Start.mid alias music";
            Assert.AreEqual("1", MidiOutCaps.Mci(command));

            command = "play music";
            Assert.AreEqual(string.Empty, MidiOutCaps.Mci(command));
        }
    } // End class MidiOutCapsUnitTests
}
