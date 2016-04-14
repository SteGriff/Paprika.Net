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
        public void TwoOptionsInSlashHaveTwoPossibilities()
        {
            string query = "[hello/hi]";

            var core = new Core();

            int expected = 2;
            int actual = core.NumberOfOptions(query);

            Assert.AreEqual(expected, actual);
        }
    }
}
