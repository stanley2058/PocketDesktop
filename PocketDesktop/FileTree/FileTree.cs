using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PocketDesktop.FileTree
{
    public class FileTree
    {
        private readonly TreeNode<string> _treeRoot;
        private readonly List<string> _orderList;
        private int _skipped = 0;

        public FileTree()
        {
            _treeRoot = new TreeNode<string>(null, new List<TreeNode<string>>(), "root");
            _orderList = new List<string>();
            InitTree();
            InitOrderList();
        }

        public IEnumerable<string> GetPageView()
        {
            var page = new List<string>();
            for (var i = 0; i < 9 && i + _skipped < _orderList.Count; ++i)
                page.Add(_orderList[i]);
            return page;
        }

        public void NextLine()
        {
            if (_skipped + 3 > _orderList.Count) return;
            _skipped += 3;
        }

        public void PrevLine()
        {
            if (_skipped - 3 < 0) return;
            _skipped -= 3;
        }

        private void InitOrderList()
        {
            var apps = new List<string>();
            var dirs = new List<string>();
            foreach (var node in _treeRoot.GetChilds())
            {
                if (node.IsLeaf())
                    apps.Add(node.GetVal());
                else
                    dirs.Add(node.GetVal());
            }

            _orderList.AddRange(dirs.OrderBy(s => s));
            _orderList.AddRange(apps.OrderBy(s => s));
        }

        private void InitTree()
        {
            var folder = Environment.CurrentDirectory + "/PocketDesktop";
            ProcessDirectory(folder, _treeRoot);
        }

        private static void ProcessDirectory(string dir, TreeNode<string> parent)
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(dir);
            foreach (var fileName in fileEntries)
                parent.Append(new TreeNode<string>(parent, new List<TreeNode<string>>(), fileName));

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (var subdirectory in subdirectoryEntries)
            {
                var newNode = new TreeNode<string>(parent, new List<TreeNode<string>>(), subdirectory);
                parent.Append(newNode);
                ProcessDirectory(subdirectory, newNode);
            }
        }
    }
}
