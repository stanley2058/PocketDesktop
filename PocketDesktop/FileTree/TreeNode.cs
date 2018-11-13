using System.Collections.Generic;

namespace PocketDesktop.FileTree
{
    internal class TreeNode
    {
        private TreeNode _parent;
        private List<TreeNode> _childs;
        private readonly string _value;

        internal TreeNode(TreeNode parent, List<TreeNode> childs, string value)
        {
            _parent = parent;
            _childs = childs;
            _value = value;
        }

        internal void Append(TreeNode child)
        {
            if (_childs == null) _childs = new List<TreeNode>();
            child._parent = this;
            _childs.Add(child);
        }

        internal void Remove(TreeNode child)
        {
            if (_childs == null || !_childs.Contains(child)) return;
            child._parent = null;
            _childs.Remove(child);
        }

        internal bool IsRoot() => _parent == null;
        internal bool IsLeaf() => _childs == null || _childs.Count == 0;

        internal string GetVal() => _value;
        internal TreeNode GetParent() => _parent;
        internal List<TreeNode> GetChilds()
        {
            return new List<TreeNode>(_childs);
        }
    }
}
