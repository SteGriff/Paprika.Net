using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paprika.Net;

namespace Paprika.NetTests
{
    /// <summary>
    /// Summary description for CountingTests
    /// </summary>
    [TestClass]
    public class CountingTests
    {
        public CountingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Counting_TwoOptionsInSlashHaveTwoPossibilities()
        {
            var core = new Core() { CountingIsImportant = true };

            string query = "[hello/hi]";

            int expected = 2;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void Counting_ThreeOptionsInOneTagFromGrammar()
        {
            var core = new Core() { CountingIsImportant = true };
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "cat", "dog", "mouse" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string query = "[animal]";

            int expected = 3;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void Counting_ThreeTimesTwoOptions()
        {
            var core = new Core() { CountingIsImportant = true };
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "cat", "dog", "mouse" } },
                { "colour", new List<string> { "red", "blue" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string query = "the [colour] [animal]";

            int expected = 6;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Counting_AdditionalOptionWhenQuestionMarkUsed()
        {
            var core = new Core() { CountingIsImportant = true };
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "cat", "dog", "mouse" } },
                { "colour", new List<string> { "red", "blue" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string query = "the [?colour] [animal]";
            // colour now has effectively 3 options (red/blue/blank) and animal has 3
            // so total is 9

            int expected = 9;

            //This test was originally non-deterministic so let's do it a few times to make sure 
            // the result is always the same
            for (int i = 0; i < 20; i++)
            { 
                int actual = core.NumberOfOptions(query);
                Assert.AreEqual(expected, actual);
            }
        }
        
        [TestMethod()]
        public void Counting_MixOfGrammarAndSlashTags()
        {
            var core = new Core();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "cat", "dog", "mouse" } },
            };

            core.LoadThisGrammar(sampleDictionary);
            string query = "the [big/small] [animal]";

            int expected = 6;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Counting_WithNestedGrammarLookups()
        {
            var core = new Core();
            var grammarContent = @"
* phrase
[verb] [thing to [verb]]

* verb
be
ask

* thing to be
good
nice

* thing to ask
questions
nicely
";
            var grammarLines = grammarContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            core.LoadGrammarFromString(grammarLines);
            string query = "[phrase]";

            //be good
            //be nice
            //ask questions
            //ask nicely
            int expected = 4;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void Counting_MixedDynamicAndStatic()
        {
            var core = new Core();
            var grammarContent = @"
* phrase
hello
nice [thing]

* thing
hat
dog

";
            var grammarLines = grammarContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            core.LoadGrammarFromString(grammarLines);
            string query = "[phrase]";

            //hello
            //nice hat
            //nice dog
            int expected = 3;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }
    }
}
