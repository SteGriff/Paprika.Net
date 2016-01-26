using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paprika.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paprika.Net.Tests
{
    [TestClass()]
    public class CoreTests
    {
        Core core;
        public CoreTests()
        {
            core = new Core();
        }

        [TestMethod()]
        public void AHatIsAHat()
        {
            string input = "[a] [hat/hat]";
            string expected = "a hat";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void SlashTagWithTwoOptions()
        {
            string input = "[cat/dog]";

            int CatCount = 0;
            int DogCount = 0;
            for (int i = 0; i < 100; i++)
            {
                string actual = core.Parse(input);
                if (actual == "cat")
                {
                    CatCount++;
                }
                else if (actual == "dog")
                {
                    DogCount++;
                }
                else
                {
                    Assert.Fail("Output must be either cat or dog!");
                }
            }

            //Must produce both options with more than 1:100 chance
            Assert.IsTrue(CatCount > 1);
            Assert.IsTrue(DogCount > 1);
        }

        [TestMethod()]
        public void SlashTagWithOneOptionTwice()
        {
            string input = "[dog/dog]";
            
            string actual = core.Parse(input);
            string expected = "dog";

            Assert.AreEqual(expected, actual);
        }

    }
}