using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SquarifiedTreemap.Model.Input;

namespace SquarifiedTreemap.Test.Model.Input
{
    [TestClass]
    public class TreeTest
    {
        [TestMethod]
        public void AssertThat_TreeNode_IsConstructableWithCollectionInitializers()
        {
            var t = new Tree<TestData>(new Tree<TestData>.Node(new TestData("1")) {
                new Tree<TestData>.Node(new TestData("2")),
                new Tree<TestData>.Node(new TestData("3")) {
                    new Tree<TestData>.Node(new TestData("4"))
                }
            });

            Assert.AreEqual("1", t.Root.Value.Value);
            Assert.AreEqual("2", t.Root.First().Value.Value);
            Assert.AreEqual("3", t.Root.Skip(1).First().Value.Value);
            Assert.AreEqual("4", t.Root.Skip(1).First().First().Value.Value);
        }

        [TestMethod]
        public void AssertThat_NodeArea_IsSetArea_WhenSet()
        {
            var t = new Tree<TestData>(new Tree<TestData>.Node(new TestData("1", 1)) {
                new Tree<TestData>.Node(new TestData("2", 2)),
                new Tree<TestData>.Node(new TestData("3", 3)),
            });

            Assert.AreEqual(1, t.Root.Area);
        }

        [TestMethod]
        public void AssertThat_NodeArea_IsSumOfChildren_WhenNotSet()
        {
            var t = new Tree<TestData>(new Tree<TestData>.Node {
                new Tree<TestData>.Node(new TestData("2", 2)),
                new Tree<TestData>.Node(new TestData("3", 3)),
            });

            Assert.AreEqual(5, t.Root.Area);
        }
    }
}
