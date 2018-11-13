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
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(_appPath);
        }

        public bool IsDir()
        {
            return File.GetAttributes(_appPath).HasFlag(FileAttributes.Directory);
        }

        public void StartApp()
        {
            Trace.WriteLine($"Starting {GetName()}");
        }
    }
}
