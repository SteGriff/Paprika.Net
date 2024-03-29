﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paprika.Net;
using Paprika.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
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
            var core = new PaprikaEngine();
            Assert.AreEqual(0, core.Grammar.Count);
        }

        [TestMethod()]
        public void CoreLoadsFromPassedDirectory()
        {
            var core = new PaprikaEngine();

            // You gotta pull down submodules for this to work
            // git submodule update --init --recursive
            var executingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            string rootDirectory = executingDirectory.Parent.Parent.Parent.FullName + @"\paprika-grammar";

            core.LoadManifest(rootDirectory);

            Assert.AreNotEqual(0, core.Grammar.Count);
            Assert.IsNotNull(core.Grammar);
        }

        [TestMethod()]
        public void SlashTagWithTwoOptions()
        {
            var core = new PaprikaEngine();

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
            var core = new PaprikaEngine();
            string input = "[dog/dog]";

            string actual = core.Parse(input);
            string expected = "dog";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void PickRandomlyFromCategoryOfThree()
        {
            var core = new PaprikaEngine();
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
            var core = new PaprikaEngine();
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
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "dog", "cat" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[animal#1] [animal#2]";

            bool gotOne = false;
            for (int i = 0; i < 200; i++)
            {
                string actual = core.Parse(input);
                if (actual == "dog cat" || actual == "cat dog")
                {
                    gotOne = true;
                }
            }

            Assert.IsTrue(gotOne);
        }


        [TestMethod]
        public void NestedTagEvaluates()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "canine" } },
                { "canine", new List<string> { "dog" } },
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "One such [animal] is the [[animal]]";
            string expected = "One such canine is the dog";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HiddenTagDoesNotDisplay()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "canine", "feline" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[!animal]";
            string expected = "";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HiddenTagPopulatesNestedInstance()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "canine" } },
                { "canine", new List<string> { "dog" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[!animal][[animal]]";
            string expected = "dog";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(BracketResolutionException))]
        public void NestedTagWithNoEarlyCallThrowsBracketResolutionException()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "canine" } },
                { "canine", new List<string> { "dog" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[[animal]]";

            //We expect a BracketResolutionException using the ExpectedException attribute
            string actual = core.Parse(input);

        }

        [TestMethod]
        public void FairDistributionOfNestedTagResults()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "animal", new List<string> { "canine", "feline" } },
                { "canine", new List<string> { "dog", "wolf" } },
                { "feline", new List<string> { "cat", "lion" } }
            };

            core.LoadThisGrammar(sampleDictionary);
            string input = "[animal] [[animal]]";

            int Dog = 0;
            int Wolf = 0;
            int Cat = 0;
            int Lion = 0;
            for (int i = 0; i < 1000; i++)
            {
                string actual = core.Parse(input);
                if (actual == "canine dog")
                {
                    Dog++;
                }
                else if (actual == "canine wolf")
                {
                    Wolf++;
                }
                else if (actual == "feline cat")
                {
                    Cat++;
                }
                else if (actual == "feline lion")
                {
                    Lion++;
                }
                else
                {
                    Assert.Fail("Output did not come from dictionary: " + actual);
                }
            }

            //Must produce all options a fair amount (1 in 100)
            Assert.IsTrue(Dog > 10);
            Assert.IsTrue(Wolf > 10);
            Assert.IsTrue(Cat > 10);
            Assert.IsTrue(Lion > 10);
        }

        [TestMethod]
        public void InjectedValueUsedInsteadOfRandom()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "letter", new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" } },
                { "word b", new List<string> { "banana" } }
            };

            core.LoadThisGrammar(sampleDictionary);

            string input = "[!letter][word [letter]]";
            var injection = new Dictionary<string, string> { { "letter", "b" } };

            string expected = "banana";
            string actual = core.Parse(input, injection);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MultipleInjectedValuesUsedInsteadOfRandom()
        {
            var core = new PaprikaEngine();
            var sampleDictionary = new Dictionary<string, List<string>>
            {
                { "letter", new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" } },
                { "word b", new List<string> { "banana" } },
                { "role", new List<string> { "fruit", "food", "meal", "sweet", "item", "snack", "gun", "prop" } },
            };

            core.LoadThisGrammar(sampleDictionary);

            string input = "[!letter][word [letter]] [role]";

            var injection = new Dictionary<string, string> {
                { "letter", "b" },
                { "role", "fruit" }
            };

            string expected = "banana fruit";
            string actual = core.Parse(input, injection);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, ExpectedException(typeof(GrammarLoadingException))]
        public void AddingSameCategoryTwiceThrowsGrammarLoadingException()
        {
            var core = new PaprikaEngine();

            string grammarString = @"
*letter
a
b
c

*thing
fruit
animal

*letter
d
e
f
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            core.LoadGrammarFromString(lines);
        }

        [TestMethod]
        public void PickRandomFromZeroOptionsDoesNotCrash()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* something
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);
            string expected = "";
            var actual = core.Parse("[something]");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TripleNestedBrackets()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
#Toplevel
* thing
food
animal

#Midlevel
* food
fruit
vegetable

* animal
bird
fish

#Bottomlevel
* fruit
apple
orange

* vegetable
onion
carrot

* bird
sparrow
hawk

* fish
carp
trout
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);

            var possibilities = new List<string>() { "apple", "orange", "onion", "carrot", "sparrow", "hawk", "carp", "trout" };

            for (int i = 0; i < 50; i++)
            {
                var actual = core.Parse("[!thing][![thing]][[[thing]]]");
                CollectionAssert.Contains(possibilities, actual);
            }
        }

        [TestMethod]
        public void CommentIsIgnoredInEntryList()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* something
dog
#cat
mouse
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);

            var possibilities = new List<string>() { "dog", "mouse" };
            for (int i = 0; i < 50; i++)
            {
                var actual = core.Parse("[something]");
                if (actual == "cat") Assert.Fail();
                CollectionAssert.Contains(possibilities, actual);
            }
        }

        [TestMethod]
        public void CommentCanBeEscaped()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* hashtag
\#Party
\#Yay
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);

            var possibilities = new List<string>() { "#Party", "#Yay" };
            for (int i = 0; i < 50; i++)
            {
                var actual = core.Parse("[hashtag]");
                CollectionAssert.Contains(possibilities, actual);
            }
        }

        [TestMethod]
        public void OutputIsTrimmed()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* phrase
hello [/world]
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);

            var possibilities = new List<string>() { "hello", "hello world" };
            for (int i = 0; i < 100; i++)
            {
                var actual = core.Parse("[phrase]");
                CollectionAssert.Contains(possibilities, actual);
                Assert.AreNotEqual("hello ", actual, "Shouldn't have trailing space");
            }
        }

        [TestMethod]
        public void QuestionMarkCanHideOutput()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* phrase
hello [?place]

* place
world
southport
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);

            var helloCount = 0;
            var helloWorldCount = 0;
            var helloSouthportCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                var actual = core.Parse("[phrase]");
                if (actual == "hello") helloCount++;
                else if (actual == "hello world") helloWorldCount++;
                else if (actual == "hello southport") helloSouthportCount++;
                else Assert.Fail(actual);
            }

            Assert.IsTrue(helloCount > 10, "helloCount");
            Assert.IsTrue(helloWorldCount > 10, "helloWorldCount");
            Assert.IsTrue(helloSouthportCount > 10, "helloSouthportCount");
        }

        [TestMethod]
        [ExpectedException(typeof(InputException))]
        public void FullyRecursiveGrammarFinishesProcessing()
        {
            var core = new PaprikaEngine();
            string grammarString = @"
* recursion
ooh [recursion]
hey [recursion]
nice [recursion]
[recursion] dog
";
            var lines = grammarString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            core.LoadGrammarFromString(lines);
            
            var actual = core.Parse("[recursion]");
            //Expect InputException (See annotation)
        }
    }
}