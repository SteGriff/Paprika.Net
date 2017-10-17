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
            var actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual.UpperBound);
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
            var actual = core.NumberOfOptions(query);
            
            Assert.AreEqual(expected, actual.UpperBound);
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
            var actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual.UpperBound);
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
                var actual = core.NumberOfOptions(query);

                Assert.AreEqual(expected, actual.UpperBound);
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
            var actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual.UpperBound);
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
            var actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual.UpperBound);
        }
        
        [TestMethod()]
        public void Counting_MixedDynamicAndStatic()
        {
            //There are actually 3 options to this grammar result
            // but we are not good enough at counting inside core
            // (and it's a hard problem because it would mean recursively
            //  solving every branch) so we will leave this for now

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

            //True number = 3:
            //hello
            //nice hat
            //nice dog
            var actual = core.NumberOfOptions(query);

            //Don't care
            // Just check LB <= UB
            Assert.IsTrue(actual.LowerBound <= actual.UpperBound);
        }
    }
}
