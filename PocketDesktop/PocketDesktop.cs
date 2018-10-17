using System;
using System.IO;

namespace PocketDesktop
{
    public static class PocketDesktop
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var currentDir = Environment.CurrentDirectory;
            if (!Directory.Exists(currentDir + "/PocketDesktop"))
                Directory.CreateDirectory(currentDir + "/PocketDesktop");

            new System.Windows.Application().Run(new MainDesktopWindow());
        }
    }
}
