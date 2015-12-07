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
        public Node<T> Root { get; private set; }

        private Treemap(BoundingRectangle startSpace)
        {
            StartSpace = startSpace;
        }

        #region bounds generation
        internal void GenerateBounds()
        {
            GenerateBounds(StartSpace, new[] { Root }, Root.SplitVertical);
        }

        internal static void GenerateBounds(BoundingRectangle space, IEnumerable<Node<T>> nodes, bool verticalSplit)
        {
            //No explicit base case - it's just the case where the foreach loop doesn't loop i.e. node has no children

            float total = 0;
            foreach (var node in nodes)
            {
                //Calculate bounds for this node
                node.Bounds = verticalSplit
                    ? new BoundingRectangle(new Vector2(total, space.Min.Y), new Vector2(total + node.Length, space.Max.Y))
                    : new BoundingRectangle(new Vector2(space.Min.X, total), new Vector2(space.Max.X, total + node.Length));

                //Increase total to offset next box
                total += node.Length;

                //Recursively generate bounds for child nodes
                GenerateBounds(node.Bounds, node, node.SplitVertical);
            }
        }
        #endregion

        #region building treemap
        public static Treemap<T> Build(BoundingRectangle space, ITree<T> data)
        {
            //Create the output
            var map = new Treemap<T>(space);

            //Create root node with unspecified direction and length
            var root = new Node<T>(map, data.Root.Value, -1);
            map.Root = root;

            //Create the map, this will initialize the direction of the root node
            DivideNode(space, data.Root, root, new ArrayPool<float>(), map);

            //Initialize the length of the root node
            root.Length = Size(space, !root.SplitVertical);

            return map;
        }

        private static void DivideNode(BoundingRectangle space, INode<T> input, Node<T> output, ArrayPool<float> pool, Treemap<T> map)
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

                SplitHorizontal(space, input, output, h, pool, map);
            }
            else
            {
                //Free the unused array
                pool.Free(h);

                SplitVertical(space, input, output, v, pool, map);
            }
        }

        private static void ApplySplit(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool, Treemap<T> map)
        {
            //Create child nodes and add them to the parent
            var index = 0;
            foreach (var child in input)
                output.Add(new Node<T>(map, child.Value, sizes[index++]));

            //Now that we're done with the array, free it
            pool.Free(sizes);

            //Recursively subdivide each child
            for (var i = 0; i < output.Count; i++)
            {
                var iNode = input[i];
                var oNode = output[i];

                //Calculate bounds for this node (the bounds are not in the right place, they are the right *size* which is the only thing we care about here)
                var box = output.SplitVertical
                    ? new BoundingRectangle(new Vector2(0, space.Min.Y), new Vector2(oNode.Length, space.Max.Y))
                    : new BoundingRectangle(new Vector2(space.Min.X, 0), new Vector2(space.Max.X, oNode.Length));

                //Recursion
                DivideNode(box, iNode, oNode, pool, map);
            }
        }

        private static void SplitHorizontal(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool, Treemap<T> map)
        {
            output.SplitVertical = false;
            ApplySplit(space, input, output, sizes, pool, map);
        }

        private static void SplitVertical(BoundingRectangle space, INode<T> input, Node<T> output, float[] sizes, ArrayPool<float> pool, Treemap<T> map)
        {
            output.SplitVertical = true;
            ApplySplit(space, input, output, sizes, pool, map);
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
        #endregion
    }
}
