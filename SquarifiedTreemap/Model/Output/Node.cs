using System;
using System.Collections;
using System.Collections.Generic;

namespace SquarifiedTreemap.Model.Output
{
    public class Node<T>
        : IReadOnlyList<Node<T>> 
        where T : ITreemapNode
    {
        /// <summary>
        /// The value associated with this output node
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Indicates if this node is a leaf
        /// </summary>
        public bool IsLeaf { get; private set; }

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

        private readonly List<Node<T>> _children = new List<Node<T>>();  

        internal Node(T value, bool splitVertical, float length)
        {
            Value = value;

            SplitVertical = splitVertical;
            IsLeaf = false;

            Length = length;
        }

        internal Node(T value, float length)
        {
            Value = value;

            SplitVertical = false;
            IsLeaf = true;

            Length = length;
        }

        internal void Add(Node<T> child)
        {
            if (IsLeaf)
                throw new InvalidOperationException("Cannot add child to leaf node");

            _children.Add(child);
        }

        public int Count => _children.Count;
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
