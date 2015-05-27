using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VolcaSampleCmdLineTool;

namespace UnitTestProject1
{
    [TestClass]
    public class ParseSlotsTests
    {
        [TestMethod]
        public void ParseSlots_Range()
        {
            const string input = "0-99";
            var result = Program.ParseSlots(input);
            Assert.IsTrue(result.Count == 100);
        }

        [TestMethod]
        public void ParseSlots_Single()
        {
            const string input = "0";
            var result = Program.ParseSlots(input);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0] == 0);
        }


        [TestMethod]
        public void ParseSlots_Csv()
        {
            const string input = "0,1,2,3,4";
            var result = Program.ParseSlots(input);
            Assert.IsTrue(result.Count == 5);
            Assert.IsTrue(result[0] == 0);
            Assert.IsTrue(result[1] == 1);
            Assert.IsTrue(result[2] == 2);
            Assert.IsTrue(result[3] == 3);
            Assert.IsTrue(result[4] == 4);
        }
    }
}
