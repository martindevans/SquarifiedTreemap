using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SquarifiedTreemap.Model.Input
{
    public class Tree<T>
        : ITree<T>
        where T : ITreemapNode
    {
        public INode<T> Root { get; }

        public Tree(Node root)
        {
            Root = root;
        }

        public class Node
            : INode<T>
        {
            public T Value { get; }

            private readonly List<INode<T>> _children = new List<INode<T>>();

            public int Count => _children.Count;
            public INode<T> this[int index] => _children[index];

            private float? _area;
            public float Area
            {
                get
                {
                    if (!_area.HasValue)
                        _area = Value?.Area ?? _children.Sum(a => a.Area);
                    return _area.Value;
                }
            }

            public Node(T value)
            {
                Value = value;
            }

            public Node()
            {
                Value = default(T);
            }

            public void Add(INode<T> node)
            {
                _children.Add(node);
            }

            #region ienumerable
            public IEnumerator<INode<T>> GetEnumerator()
            {
                return _children.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_children).GetEnumerator();
            }
            #endregion
        }
    }
}
