using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace PocketDesktop.ApplicationObject
{
    public class ApplicationObject : Image
    {
        private string _appPath;

        public void UpdateInfos(string appPath)
        {
            _appPath = appPath;
            Source = IconGetter.GetIconBitmapImage(appPath);
            ToolTip = GetPath();
        }

        public string GetName() => Path.GetFileNameWithoutExtension(_appPath);

        public string GetPath() => _appPath;

        public bool IsDir()
        {
            if (_appPath.EndsWith(".lnk"))
                _appPath = IconGetter.GetExePathFromInk(_appPath);

            try
            {
                return File.GetAttributes(_appPath).HasFlag(FileAttributes.Directory);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }
        }

        public void Clear()
        {
            _appPath = null;
            Source = null;
            ToolTip = null;
        }

        public bool StartApp() // return ture if start successfully
        {
            if (IsDir()) return false;
            Process.Start(_appPath);
            return true;
        }
    }
}
