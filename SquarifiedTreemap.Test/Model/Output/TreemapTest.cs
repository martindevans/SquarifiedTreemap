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

            var rectangles = result.Rectangles(false);
            Assert.AreEqual(1, rectangles.Single(a => a.Value?.Value == "1").Key.Area(), 0.0001f);
            Assert.AreEqual(2, rectangles.Single(a => a.Value?.Value == "2").Key.Area(), 0.0001f);
            Assert.AreEqual(0.8f, rectangles.Single(a => a.Value?.Value == "3a").Key.Area(), 0.0001f);
            Assert.AreEqual(1, rectangles.Single(a => a.Value?.Value == "3b").Key.Area(), 0.0001f);
            Assert.AreEqual(1.2f, rectangles.Single(a => a.Value?.Value == "3c").Key.Area(), 0.0001f);
        }

        [TestMethod]
        public void AssertThat_SingleLevelTree_AssignsCorrectArea()
        {
            var tree = new Tree<TestData>(new Tree<TestData>.Node(new TestData("a", 6)));

            var result = Treemap<TestData>.Build(new BoundingRectangle(new Vector2(0, 0), new Vector2(3, 2)), tree);

            DrawTreemap(result);

            var rectangles = result.Rectangles(false);
            Assert.AreEqual(6, rectangles.Single(a => a.Value?.Value == "a").Key.Area(), 0.0001f);
        }

        private static void DrawTreemap<T>(Treemap<T> map) where T : ITreemapNode
        {
            DrawTreemap(map.Rectangles(true));
        }

        private static void DrawTreemap<T>(IEnumerable<KeyValuePair<BoundingRectangle, T>> rectangles) where T : ITreemapNode
        {
            var svg = new SvgBuilder(50);
            foreach (var rectangle in rectangles)
                svg.Outline(rectangle.Key.GetCorners());

            Console.WriteLine(svg.ToString());
        }

        [TestMethod]
        public void AssertThat_ToRectangles_ProducesFullRectangle_WithRootNode_WithNoChildren()
        {
            var results = Treemap<TestData>.Rectangles(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] {  new Node<TestData>(new TestData("100"), 10) }, true, false);

            DrawTreemap(results);

            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), results.Single().Key);
        }

        [TestMethod]
        public void AssertThat_ToRectangles_ProducesSpecifiedRectangles_WithRootNode_WithTwoChildren()
        {
            var result = Treemap<TestData>.Rectangles(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] {
                new Node<TestData>(null, true, 10) {
                    new Node<TestData>(new TestData("0"), 4),
                    new Node<TestData>(new TestData("1"), 6),
                }
            }, true, false);

            DrawTreemap(result);

            var root = result.Single(a => a.Value == null);
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), root.Key);

            var l = result.Single(a => a.Value?.Value == "0");
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 10)), l.Key);

            var r = result.Single(a => a.Value?.Value == "1");
            Assert.AreEqual(new BoundingRectangle(new Vector2(4, 0), new Vector2(10, 10)), r.Key);
        }

        [TestMethod]
        public void AssertThat_ToRectangles_ProducesAlternatelyAlignedRectangle_WithDepth3()
        {
            var result = Treemap<TestData>.Rectangles(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), new[] {
                new Node<TestData>(null, true, 10) {
                    new Node<TestData>(new TestData("0"), false, 4) {
                        new Node<TestData>(new TestData("1"), 2),
                        new Node<TestData>(new TestData("2"), 8),
                    },
                    new Node<TestData>(new TestData("3"), 6),
                }
            }, true, false);

            DrawTreemap(result);

            //Does the root take all the available space?
            var root = result.Single(a => a.Value == null);
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(10, 10)), root.Key);

            var b = result.Single(a => a.Value?.Value == "0");
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 10)), b.Key);

            var c = result.Single(a => a.Value?.Value == "1");
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 0), new Vector2(4, 2)), c.Key);

            var d = result.Single(a => a.Value?.Value == "2");
            Assert.AreEqual(new BoundingRectangle(new Vector2(0, 2), new Vector2(4, 10)), d.Key);
        }
    }
}
