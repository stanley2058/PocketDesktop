using System.Collections.Generic;

namespace PocketDesktop
{
    public class TreeNode<T>
    {
        private TreeNode<T> _parent;
        private List<TreeNode<T>> _childs;
        private readonly T _value;

        public TreeNode(TreeNode<T> parent, List<TreeNode<T>> childs, T value)
        {
            _parent = parent;
            _childs = childs;
            _value = value;
        }

        public void Append(TreeNode<T> child)
        {
            if (_childs == null) _childs = new List<TreeNode<T>>();
            child._parent = this;
            _childs.Add(child);
        }

        public void Remove(TreeNode<T> child)
        {
            if (_childs == null || !_childs.Contains(child)) return;
            child._parent = null;
            _childs.Remove(child);
        }

        public bool IsRoot() => _parent == null;
        public bool IsLeaf() => _childs == null || _childs.Count == 0;

        public T GetVal() => _value;
        public TreeNode<T> GetParent() => _parent;
        public List<TreeNode<T>> GetChilds() => new List<TreeNode<T>>(_childs);
    }
}
