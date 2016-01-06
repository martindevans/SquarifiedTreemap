using System.Collections.Generic;

namespace SquarifiedTreemap
{
    public interface ITreeNode<out TSelf>
        where TSelf : ITreeNode<TSelf>
    {
        TSelf Parent { get; }

        IEnumerable<TSelf> Children { get; }
    }
}
