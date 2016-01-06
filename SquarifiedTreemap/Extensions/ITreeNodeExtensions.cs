using System.Collections.Generic;

namespace SquarifiedTreemap.Extensions
{
    public static class ITreeNodeExtensions
    {
        /// <summary>
        /// Recursively walk all the nodes in a tree (bottom up)
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<TNode> WalkTreeBottomUp<TNode>(this TNode root)
            where TNode : ITreeNode<TNode>
        {
            foreach (var node in root.Children)
                foreach (var child in WalkTreeBottomUp(node))
                    yield return child;

            yield return root;
        }

        /// <summary>
        /// Recursively walk down all the nodes in a tree (top down)
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<TNode> WalkTreeTopDown<TNode>(this TNode root)
            where TNode : ITreeNode<TNode>
        {
            yield return root;

            foreach (var node in root.Children)
                foreach (var child in WalkTreeBottomUp(node))
                    yield return child;
        }
    }
}
