using System;
using System.Collections;
using System.Collections.Generic;
using SquarifiedTreemap.Extensions;
using SwizzleMyVectors.Geometry;

namespace SquarifiedTreemap.Model.Output
{
    public class Node<T>
        : IReadOnlyList<Node<T>>, ITreeNode<Node<T>> 
        where T : ITreemapNode
    {
        private readonly Treemap<T> _map;

        /// <summary>
        /// The value associated with this output node
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Indicates if this node is a leaf
        /// </summary>
        public bool IsLeaf { get; private set; }

        private BoundingRectangle? _boundsCache;
        /// <summary>
        /// The space assigned to this node
        /// </summary>
        public BoundingRectangle Bounds
        {
            get
            {
                //Read cache, lazily generate rectangle if none
                if (!_boundsCache.HasValue)
                    _map.GenerateBounds();
                if (!_boundsCache.HasValue)
                    throw new InvalidOperationException("Failed to assigned bounds to treemap node");
                return _boundsCache.Value;
            }
            internal set { _boundsCache = value; }
        }

        private bool _splitVertical;
        /// <summary>
        /// Indicates if the current rectangle is being split vertically, or horizontally (to layout the children)
        /// </summary>
        public bool SplitVertical
        {
            get { return _splitVertical; }
            internal set
            {
                _splitVertical = value;
                IsLeaf = false;
            }
        }

        /// <summary>
        /// Indicates how long this node is, perpendicular to the parent split line (width is always the full length of the split line)
        /// </summary>
        public float Length { get; internal set; }

        public Node<T> Parent { get; internal set; }

        private readonly List<Node<T>> _children = new List<Node<T>>();  

        internal Node(Treemap<T> map, T value, bool splitVertical, float length)
        {
            _map = map;

            Value = value;
            SplitVertical = splitVertical;
            IsLeaf = false;

            Length = length;
        }

        internal Node(Treemap<T> map, T value, float length)
        {
            _map = map;
            Value = value;
            SplitVertical = false;
            IsLeaf = true;

            Length = length;
        }

        internal void Add(Node<T> child)
        {
            if (IsLeaf)
                throw new InvalidOperationException("Cannot add child to leaf node");
            if (child.Parent != null)
                throw new ArgumentException("child already has a parent", nameof(child));

            child.Parent = this;
            _children.Add(child);
        }

        /// <summary>
        /// Swap the positions of two child nodes
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public void Swap(int item1, int item2)
        {
            //Get the items
            var a = _children[item1];
            var b = _children[item2];

            //Swap them
            _children[item1] = b;
            _children[item2] = a;

            //Clear the bounds caches of both subtrees
            foreach (var node in a.WalkTreeBottomUp())
                node._boundsCache = null;
            foreach (var node in b.WalkTreeBottomUp())
                node._boundsCache = null;
        }

        public int Count => _children.Count;

        #region explicit implementation of ITreeNode
        Node<T> ITreeNode<Node<T>>.Parent { get { return Parent; } }
        IEnumerable<Node<T>> ITreeNode<Node<T>>.Children { get { return _children; } }
        #endregion

        public Node<T> this[int index] => _children[index];

        public IEnumerator<Node<T>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
