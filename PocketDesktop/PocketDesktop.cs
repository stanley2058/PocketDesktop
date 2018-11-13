using System;
using System.IO;

namespace PocketDesktop
{
    public static class PocketDesktop
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Directory.Exists(WorkFolder))
                Directory.CreateDirectory(WorkFolder);

            new System.Windows.Application().Run(new MainDesktopWindow());
        }

        public static readonly string WorkFolder = Environment.CurrentDirectory + "/PocketDesktop/";
    }
}
