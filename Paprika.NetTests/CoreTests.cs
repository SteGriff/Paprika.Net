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

        [TestMethod()]
        public void NewCoreHasNoGrammar()
        {
            var core = new Core();
            Assert.AreEqual(0, core.Grammar.Count);
        }

        [TestMethod()]
        public void CoreLoadsFromConfiguredDirectory()
        {
            var core = new Core();
            core.LoadConfiguredManifest();

            Assert.AreNotEqual(0, core.Grammar.Count);
            Assert.IsNotNull(core.Grammar);
        }

        [TestMethod()]
        public void CoreLoadsFromPassedDirectory()
        {
            var core = new Core();

            string rootDirectory = "C:/Users/Ste/OneDrive/Documents/Programming/data/paprika-grammar/";
            core.LoadManifest(rootDirectory);

            Assert.AreNotEqual(0, core.Grammar.Count);
            Assert.IsNotNull(core.Grammar);
        }

        [TestMethod()]
        public void SlashTagWithTwoOptions()
        {
            var core = new Core();

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
            var core = new Core();
            string input = "[dog/dog]";
            
            string actual = core.Parse(input);
            string expected = "dog";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void PickRandomlyFromCategoryOfThree()
        {
            var core = new Core();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "cat", "dog", "mouse" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[animal]";

            int CatCount = 0;
            int DogCount = 0;
            int MouseCount = 0;
            for (int i = 0; i < 1000; i++)
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
                else if (actual == "mouse")
                {
                    MouseCount++;
                }
                else
                {
                    Assert.Fail("Output did not come from dictionary: " + actual);
                }
            }

            //Must produce all options a fair amount (1 in 100)
            Assert.IsTrue(CatCount > 10);
            Assert.IsTrue(DogCount > 10);
            Assert.IsTrue(MouseCount > 10);
        }

        [TestMethod()]
        public void SameTagTwiceGetsSameResult()
        {
            var core = new Core();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "dog", "cat" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[animal] [animal]";
            
            string actual = core.Parse(input);

            Assert.IsTrue(actual == "dog dog" || actual == "cat cat");
        }

        [TestMethod()]
        public void SameTagTwiceWithMarkersCanHaveSeparateResults()
        {
            var core = new Core();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "dog", "cat" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[animal#1] [animal#2]";

            bool gotOne = false;
            for(int i = 0; i < 200; i++)
            {
                string actual = core.Parse(input);
                if (actual == "dog cat" || actual == "cat dog")
                {
                    gotOne = true;
                }
            }

            Assert.IsTrue(gotOne);
        }

    }
}