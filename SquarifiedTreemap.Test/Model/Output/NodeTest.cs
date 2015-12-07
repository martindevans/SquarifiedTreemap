using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SquarifiedTreemap.Model.Output;

namespace SquarifiedTreemap.Test.Model.Output
{
    [TestClass]
    public class NodeTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AssertThat_AddingChildToLeafNode_Throws()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Node<TestData>(null, new TestData("1"), 10)
            {
                new Node<TestData>(null, null, 1)
            };
        }

        [TestMethod]
        public void AssertThat_AddingChildToLeafNode_AddsToCollection()
        {
            var a = new Node<TestData>(null, new TestData("1"), true, 10)
            {
                new Node<TestData>(null, null, 1)
            };

            Assert.AreEqual(1, a.Count());
        }
    }
}
