using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace PocketDesktop.ApplicationObject
{
    public class ApplicationObject : Image
    {
        private string AppPath;

        public ApplicationObject()
        {
        }

        public void UpdateInfos(string appPath)
        {
            AppPath = appPath;
            Source = IconGetter.GetIconBitmapImage(appPath);
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(AppPath);
        }

        public bool IsDir()
        {
            return File.GetAttributes(AppPath).HasFlag(FileAttributes.Directory);
        }

        public void StartApp()
        {
            Trace.WriteLine("Started!");
        }
    }
}
