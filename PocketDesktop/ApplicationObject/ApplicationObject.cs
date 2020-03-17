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
            ToolTip = GetPath().Replace(PocketDesktop.WorkFolder, "");
        }

        public string GetName() => Path.GetFileNameWithoutExtension(_appPath);

        public string GetPath() => _appPath;

        public string GetTruePath() => IconGetter.GetExePathFromInk(_appPath);

        public bool IsDir()
        {
            try
            {
                return File.GetAttributes(_appPath.EndsWith(".lnk") ? GetTruePath() : _appPath).HasFlag(FileAttributes.Directory);
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
            try
            {
                Process.Start(_appPath);
            }
            catch (Exception ex1)
            {
                try
                {
                    Process.Start(GetTruePath());
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex1);
                    Console.WriteLine(ex2);
                }
            }
            return true;
        }
    }
}
