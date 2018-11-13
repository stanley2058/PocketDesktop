using PocketDesktop.ApplicationObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PocketDesktop.FileTree
{
    public class FileTree
    {
        private readonly TreeNode<string> _treeRoot;
        private TreeNode<string> _curRoot;
        private readonly List<string> _orderList;
        private int _skipped;
        private bool _searching;
        private readonly List<TreeNode<string>> _searchNodeList;

        public FileTree()
        {
            _treeRoot = new TreeNode<string>(null, new List<TreeNode<string>>(), "root");
            _orderList = new List<string>();
            _searchNodeList = new List<TreeNode<string>>();
            InitTree();
            InitOrderList(_treeRoot);
        }

        public IEnumerable<string> GetPageView()
        {
            var page = new List<string>();
            for (var i = 0; i < 9 && i + _skipped < _orderList.Count; ++i)
                page.Add(_orderList[i + _skipped]);
            return page;
        }

        public void NextLine()
        {
            if (_skipped + 6 > _orderList.Count) return;
            _skipped += 3;
        }

        public void PrevLine()
        {
            if (_skipped - 3 < 0) return;
            _skipped -= 3;
        }

        public void GoBackDir()
        {
            if (_curRoot == _treeRoot) return;
            _curRoot = _curRoot.GetParent();
            InitOrderList(_curRoot);
            _skipped = 0;
        }

        public void OpenDir(string dir)
        {
            if (dir.EndsWith(".lnk"))
                dir = IconGetter.GetExePathFromInk(dir);

            var list = _searching ? _searchNodeList : _curRoot.GetChilds();
            foreach (var node in list)
            {
                var nodeName = node.GetVal();
                if (nodeName.EndsWith(".lnk"))
                    nodeName = IconGetter.GetExePathFromInk(nodeName);
                if (!nodeName.Equals(dir)) continue;
                InitOrderList(_curRoot = node);
                _skipped = 0;
            }

            if (!_searching) return;
            _searching = false;
            _searchNodeList.Clear();
        }

        public void GoHome()
        {
            _searching = false;
            InitOrderList(_curRoot = _treeRoot);
            _skipped = 0;
        }

        public void Search(string target)
        {
            _searchNodeList.Clear();
            _searching = true;
            SearchInTree(_curRoot, target);
            InitOrderList(_searchNodeList);
            _skipped = 0;
        }
        public void UnSearch()
        {
            _searchNodeList.Clear();
            GoHome();
        }
        public bool IsSearchPage() => _searching;
        private void SearchInTree(TreeNode<string> root, string target)
        {
            if (root.IsLeaf()) return;
            root.GetChilds().ForEach(c =>
            {
                var path = Path.GetFileNameWithoutExtension(c.GetVal());
                if (path != null && path.Contains(target))
                    _searchNodeList.Add(c);
                if (!c.IsLeaf())
                    SearchInTree(c, target);
            });
        }

        private void InitOrderList(TreeNode<string> root)
        {
            _orderList.Clear();
            var apps = new List<string>();
            var dirs = new List<string>();
            foreach (var node in root.GetChilds())
            {
                if (node.IsLeaf())
                    apps.Add(node.GetVal());
                else
                    dirs.Add(node.GetVal());
            }

            _orderList.AddRange(dirs.OrderBy(s => s));
            _orderList.AddRange(apps.OrderBy(s => s));
        }

        private void InitOrderList(List<TreeNode<string>> nodeList)
        {
            _orderList.Clear();
            var apps = new List<string>();
            var dirs = new List<string>();
            foreach (var node in nodeList)
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
            ProcessDirectory(PocketDesktop.WorkFolder, _treeRoot);
            _curRoot = _treeRoot;
        }

        private static void ProcessDirectory(string dir, TreeNode<string> parent)
        {
            var lnkDir = new List<string>();
            if (dir.EndsWith(".lnk"))
                dir = IconGetter.GetExePathFromInk(dir);

            try
            {
                // Process the list of files found in the directory.
                var fileEntries = Directory.GetFiles(dir.Replace("\\", "/"));
                foreach (var fileName in fileEntries)
                {
                    if (fileName.EndsWith(".lnk"))
                    {
                        try
                        {
                            if (File.GetAttributes(IconGetter.GetExePathFromInk(fileName)).HasFlag(FileAttributes.Directory))
                                lnkDir.Add(fileName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        continue;
                    }
                    parent.Append(new TreeNode<string>(parent, new List<TreeNode<string>>(), fileName.Replace("\\", "/")));
                }

                // Recurse into subdirectories of this directory.
                var subdirectoryEntries = new List<string>(Directory.GetDirectories(dir));
                subdirectoryEntries.AddRange(lnkDir);
                foreach (var subdirectory in subdirectoryEntries)
                {
                    var filtered = subdirectory.Replace("\\", "/");
                    var newNode = new TreeNode<string>(parent, new List<TreeNode<string>>(), filtered);
                    parent.Append(newNode);
                    ProcessDirectory(filtered, newNode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
