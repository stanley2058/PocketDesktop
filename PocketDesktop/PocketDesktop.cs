using System;
using System.IO;
using System.Windows;

namespace PocketDesktop
{
    public static class PocketDesktop
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Directory.Exists(WorkFolder))
                Directory.CreateDirectory(WorkFolder);

            new Application().Run(new MainDesktopWindow());
        }

        public static readonly string WorkFolder = Environment.CurrentDirectory.Replace("\\", "/") + "/PocketDesktop/";

        public static void CenterWindow(Window w)
        {
            w.Left = (SystemParameters.PrimaryScreenWidth / 2) - (w.Width / 2);
            w.Top = (SystemParameters.PrimaryScreenHeight / 2) - (w.Height / 2);
        }
    }
}
