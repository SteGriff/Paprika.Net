using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paprika.Net;

namespace Paprika.NetTests
{
    [TestClass]
    public class AnAndATests
    {
        Core core;
        public AnAndATests()
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
        public void AOstrichIsAnOstrich()
        {
            string input = "[a] [ostrich/ostrich]";
            string expected = "an ostrich";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void AnExtraIsAnExtra()
        {
            string input = "[an] [extra/extra]";
            string expected = "an extra";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void AnDogIsADog()
        {
            string input = "[an] [dog/dog]";
            string expected = "a dog";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MultipleATagsCanHaveDifferentResults()
        {
            string input = "[a] [dog/dog] and [a] [ostrich/ostrich]";
            string expected = "a dog and an ostrich";
            string actual = core.Parse(input);

            Assert.AreEqual(expected, actual);
        }
    }
}
