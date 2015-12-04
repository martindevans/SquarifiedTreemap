using System;
using System.Collections.Generic;
using System.Numerics;
using SquarifiedTreemap.Model.Input;
using SwizzleMyVectors.Geometry;

namespace SquarifiedTreemap.Model.Output
{
    public class Treemap<T>
        where T : ITreemapNode
    {
        public BoundingRectangle StartSpace { get; }
        public Node<T> Root { get; set; }

        private Treemap(BoundingRectangle startSpace, Node<T> root)
        {
            StartSpace = startSpace;
            Root = root;
        }

        public IEnumerable<KeyValuePair<BoundingRectangle, T>> Rectangles(bool onlyLeaves)
        {
            return Rectangles(StartSpace, (Node<T>)Root, onlyLeaves);
        }

        internal static IEnumerable<KeyValuePair<BoundingRectangle, T>> Rectangles(BoundingRectangle space, Node<T> root, bool onlyLeaves)
        {
            return Rectangles(space, new[] {
                root
            }, root.SplitVertical, onlyLeaves);
        }

        internal static IEnumerable<KeyValuePair<BoundingRectangle, T>> Rectangles(BoundingRectangle space, IEnumerable<Node<T>> nodes, bool verticalSplit, bool onlyLeaves)
        {
            float total = 0;
            foreach (var node in nodes)
            {
                //Calculate bounds for this node
                BoundingRectangle box;
                if (verticalSplit)
                    box = new BoundingRectangle(new Vector2(total, space.Min.Y), new Vector2(total + node.Length, space.Max.Y));
                else
                    box = new BoundingRectangle(new Vector2(space.Min.X, total), new Vector2(space.Max.X, total + node.Length));

                //Increase total to offset next box
                total += node.Length;

                //return it (if necessary)
                if (!onlyLeaves || node.IsLeaf)
                    yield return new KeyValuePair<BoundingRectangle, T>(box, node.Value);

                //Recurse
                foreach (var n in Rectangles(box, (IEnumerable<Node<T>>)node, node.SplitVertical, onlyLeaves))
                    yield return n;
            }
        }

        public static Treemap<T> Build(BoundingRectangle space, Input.ITree<T> data)
        {
            //Create root node with unspecified direction and length
            var root = new Node<T>(data.Root.Value, -1);

            //Create the map, this will initialize the direction of the root node
            DivideNode(space, data.Root, root, new ArrayPool<float>());

            //Initialize the length of the root node
            root.Length = Size(space, !root.SplitVertical);

            return new Treemap<T>(space, root);
        }

        private static void DivideNode(BoundingRectangle space, INode<T> input, Node<T> output, ArrayPool<float> pool)
        {

            //Recursion base case
            if (input.Count == 0)
                return;

            var h = pool.Allocate(input.Count);
            var v = pool.Allocate(input.Count);

            //Take the split with the least bad aspect ratio
            if (MeasureSizes(false, space, input, h) < MeasureSizes(true, space, input, v))
            {
                //Free the unused array
                pool.Free(v);

                SplitHorizontal(space, input, output, h, pool);
            }
            else
            {
                //Free the unused array
                pool.Free(h);

                SplitVertical(space, input, output, v, pool);
            }
        }

        private static void ApplySplit(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool)
        {
            //Create child nodes and add them to the parent
            var index = 0;
            foreach (var child in input)
                output.Add(new Node<T>(child.Value, sizes[index++]));

            //Now that we're done with the array, free it
            pool.Free(sizes);

            //Recursively subdivide each child
            float total = 0;
            for (int i = 0; i < output.Count; i++)
            {
                var iNode = input[i];
                var oNode = output[i];

                //Calculate bounds for this node
                BoundingRectangle box;
                if (output.SplitVertical)
                    box = new BoundingRectangle(new Vector2(total, space.Min.Y), new Vector2(total + oNode.Length, space.Max.Y));
                else
                    box = new BoundingRectangle(new Vector2(space.Min.X, total), new Vector2(space.Max.X, total + oNode.Length));

                //Recursion
                DivideNode(box, iNode, oNode, pool);
            }
        }

        private static void SplitHorizontal(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool)
        {
            output.SplitVertical = false;
            ApplySplit(space, input, output, sizes, pool);
        }

        private static void SplitVertical(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool)
        {
            output.SplitVertical = true;
            ApplySplit(space, input, output, sizes, pool);
        }

        /// <summary>
        /// Measure the sizes of the nodes, return the worst aspect ratio with this split strategy
        /// </summary>
        /// <param name="vertical">Direction of the split line (vertical or horizontal)</param>
        /// <param name="space">The rectangle to fill in</param>
        /// <param name="nodes">The nodes to put in this space</param>
        /// <param name="sizes">Output of the sizes for each node (in smae order as nodes enumeration)</param>
        /// <returns></returns>
        private static float MeasureSizes(bool vertical, BoundingRectangle space, IReadOnlyCollection<INode<T>> nodes, float[] sizes)
        {
            if (sizes != null && nodes.Count > sizes.Length)
                throw new ArgumentException("more nodes to output size than array to put sizes into", nameof(sizes));

            //Measure the rectangle across the split line
            var across = Size(space, vertical);

            //Lay out the child nodes and keep track of the worst aspect ratio
            var worstAspectRatio = float.NegativeInfinity;

            var index = 0;
            foreach (var node in nodes)
            {
                //We must fill go completely across the space, so how far along must we go to get the correct area?
                var l = node.Area / across;
                var aspect = across / l;

                //Save this size
                if (sizes != null)
                    sizes[index++] = l;

                //Keep a running record of the worst aspect ratio we've found
                if (aspect > worstAspectRatio)
                    worstAspectRatio = aspect;
            }

            return worstAspectRatio;
        }

        private static float Size(BoundingRectangle bounds, bool vertical)
        {
            //Measure the rectangle across the split line
            var size = bounds.Max - bounds.Min;
            return vertical ? size.Y : size.X;
        }
    }
}
