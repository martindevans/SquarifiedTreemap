using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SquarifiedTreemap.Model;
using SquarifiedTreemap.Model.Input;
using SquarifiedTreemap.Model.Output;
using SwizzleMyVectors.Geometry;
using PrimitiveSvgBuilder;

namespace SquarifiedTreemap.Test.Model.Output
{
    [TestClass]
    public class TreemapTest
    {
        public static void RecursiveAssert<T>(Node<T> root, Action<Node<T>, Node<T>> assert) where T : ITreemapNode
        {
            var todo = new Stack<Node<T>>();
            todo.Push(root);

            while (todo.Count > 0)
            {
                var parent = todo.Pop();
                foreach (var child in parent)
                {
                    //Queue up more work
                    todo.Push(child);

                    assert(parent, child);
                }
            }
        }

        public static void RecursiveAssert<T>(Node<T> root, Action<Node<T>> assert) where T : ITreemapNode
        {
            var todo = new Stack<Node<T>>();
            todo.Push(root);

            while (todo.Count > 0)
            {
                var parent = todo.Pop();
                assert(parent);

                foreach (var child in parent)
                {
                    //Queue up more work
                    todo.Push(child);
                }
            }
        }

        private static void DrawTreemap<T>(Treemap<T> map) where T : ITreemapNode
        {
            DrawTreemap(map.Root);
        }

        private static void DrawTreemap<T>(Node<T> root) where T : ITreemapNode
        {
            var svg = new SvgBuilder(50);

            RecursiveAssert(root, p => {
                svg.Outline(p.Bounds.GetCorners());
            });

            Console.WriteLine(svg.ToString());
        }

        [TestMethod]
        public void AssertThat_MultiLevelTree_AssignsCorrectAreas()
        {
            var tree = new Tree<TestData>(new Tree<TestData>.Node {
                new Tree<TestData>.Node(new TestData("1", 1)),
                new Tree<TestData>.Node(new TestData("2", 2)),
                new Tree<TestData>.Node(new TestData("3")) {
                    new Tree<TestData>.Node(new TestData("3a", 0.8f)),
                    new Tree<TestData>.Node(new TestData("3b", 1)),
                    new Tree<TestData>.Node(new TestData("3c", 1.2f))
                }
            });

            var result = Treemap<TestData>.Build(new BoundingRectangle(new Vector2(0, 0), new Vector2(3, 2)), tree);

            DrawTreemap(result);

            //Check all the areas
            RecursiveAssert(result.Root, (p, c) => {
                if (p.Value?.Area != null)
                    Assert.AreEqual(p.Value.Area.Value, p.Bounds.Area(), 0.0001f);
            });
        }

        [TestMethod]
        public void AssertThat_MultiLevelTree_MaintainsParentChildRelationship()
        {
            var tree = new Tree<TestData>(new Tree<TestData>.Node(new TestData("root")) {
                new Tree<TestData>.Node(new TestData("1", 1)),
                new Tree<TestData>.Node(new TestData("2", 2)),
                new Tree<TestData>.Node(new TestData("3")) {
                    new Tree<TestData>.Node(new TestData("3a", 0.8f)),
                    new Tree<TestData>.Node(new TestData("3b", 1)),
                    new Tree<TestData>.Node(new TestData("3c", 1.2f))
                }
            });

            var result = Treemap<TestData>.Build(new BoundingRectangle(new Vector2(0, 0), new Vector2(3, 2)), tree);

            DrawTreemap(result);

            //Check that all child bounds are contained within parent bounds
            RecursiveAssert(result.Root, (p, c) => {
                Assert.IsTrue(p.Bounds.Contains(c.Bounds));
            });
        }

        [TestMethod]
        public void AssertThat_SingleLevelTree_AssignsCorrectArea()
        {
            //Construct a single level tree with a hardcoded area
            var tree = new Tree<TestData>(new Tree<TestData>.Node(new TestData("a", 6)));

            var result = Treemap<TestData>.Build(new BoundingRectangle(new Vector2(0, 0), new Vector2(3, 2)), tree);
            DrawTreemap(result);

            //Check that the resulting area is correct
            Assert.AreEqual(6, result.Root.Bounds.Area(), 0.0001f);
        }

        [TestMethod]
        public void AssertThat_GenerateBounds_ProducesSpecifiedRectangles_WithRootNode_WithTwoChildren()
        {
            var a = new Node<TestData>(null, new TestData("0"), 4);
            var b = new Node<TestData>(null, new TestData("1"), 6);

            Treemap<TestData>.GenerateBounds(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] { a, b, }, true);

            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 10)), a.Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(4, 0), new Vector2(10, 10)), b.Bounds);
        }

        [TestMethod]
        public void AssertThat_GenerateBounds_ProducesSpecifiedRectangles_WithRootNode_WithTwoChildren_WithNonZeroRectangle()
        {
            var a = new Node<TestData>(null, new TestData("0"), 4);
            var b = new Node<TestData>(null, new TestData("1"), 6);

            Treemap<TestData>.GenerateBounds(new BoundingRectangle(new Vector2(-7, -5), new Vector2(3, 5)), new[] { a, b, }, true);

            Assert.AreEqual(new BoundingRectangle(new Vector2(-7, -5), new Vector2(-3, 5)), a.Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(-3, -5), new Vector2(3, 5)), b.Bounds);
        }

        [TestMethod]
        public void AssertThat_GenerateBounds_ProducesAlternatelyAlignedRectangle_WithDepth3()
        {
            //Create a tree with hand calculated split values
            var tree = new Node<TestData>(null, null, true, 10) {
                new Node<TestData>(null, new TestData("0"), false, 4) {
                    new Node<TestData>(null, new TestData("1"), 2),
                    new Node<TestData>(null, new TestData("2"), 8),
                },
                new Node<TestData>(null, new TestData("3"), 6),
            };

            //Generate the boundaries for this tree
            Treemap<TestData>.GenerateBounds(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new [] { tree }, true);

            DrawTreemap(tree);

            //Does the root take all the available space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), tree.Bounds);

            //Do the inner nodes take the correct space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 10)), tree.First().Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(4, 0), new Vector2(10, 10)), tree.Skip(1).First().Bounds);

            //Do the leaves take the correct space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 2)), tree.First().First().Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 2), new Vector2(4, 10)), tree.First().Skip(1).First().Bounds);
        }

        [TestMethod]
        public void AssertThat_SwapIndices_RegeneratesBounds()
        {
            //Create a tree with hand calculated split values
            var tree = new Node<TestData>(null, null, true, 10) {
                new Node<TestData>(null, new TestData("0"), false, 4) {
                    new Node<TestData>(null, new TestData("1"), 2),
                    new Node<TestData>(null, new TestData("2"), 8),
                },
                new Node<TestData>(null, new TestData("3"), 6),
            };

            //Generate the boundaries for this tree
            Treemap<TestData>.GenerateBounds(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] { tree }, true);

            DrawTreemap(tree);

            //Swap indices
            tree.Swap(0, 1);

            //Regenerate boundaries
            Treemap<TestData>.GenerateBounds(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] { tree }, true);

            DrawTreemap(tree);

            //Does the root take all the available space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), tree.Bounds);

            //Do the inner nodes take the correct space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(6, 10)), tree.First().Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(6, 0), new Vector2(10, 10)), tree.Skip(1).First().Bounds);

            //Do the leaves take the correct space?
            Assert.AreEqual(new BoundingRectangle(new Vector2(6, 0), new Vector2(10, 2)), tree.Skip(1).First().First().Bounds);
            Assert.AreEqual(new BoundingRectangle(new Vector2(6, 2), new Vector2(10, 10)), tree.Skip(1).First().Skip(1).First().Bounds);
        }
    }
}
