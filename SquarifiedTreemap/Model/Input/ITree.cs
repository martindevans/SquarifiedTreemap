using System.Collections.Generic;

namespace SquarifiedTreemap.Model.Input
{
    public interface ITree<T>
        where T : ITreemapNode
    {
        INode<T> Root { get; }
    }

    public interface INode<T>
        : IReadOnlyList<INode<T>>
        where T : ITreemapNode
    {
        T Value { get; }

        /// <summary>
        /// Returns the area required by this node (sum of all child areas if Value is unset)
        /// </summary>
        float Area { get; }

        void Add(INode<T> item);
    }
}
