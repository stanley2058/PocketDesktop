using IWshRuntimeLibrary;
using PocketDesktop.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using File = System.IO.File;

namespace PocketDesktop.ApplicationObject
{
    public static class IconGetter
    {
        private static readonly Bitmap FolderIcon = (Bitmap)Resources.ResourceManager.GetObject("folder_icon");
        public static BitmapImage GetIconBitmapImage(string path)
        {
            path = path.Replace("/", "\\");
            var bitmap = FolderIcon;
            var tmpPath = path.EndsWith(".lnk") ? GetExePathFromInk(path) : path;
            if (!File.GetAttributes(tmpPath ?? path).HasFlag(FileAttributes.Directory))
            {
                try
                {
                    bitmap = path.EndsWith(".lnk")
                        ? (Icon.ExtractAssociatedIcon(path) ?? Icon.ExtractAssociatedIcon(GetExePathFromInk(path)))?.ToBitmap()
                        : Icon.ExtractAssociatedIcon(path)?.ToBitmap();
                }
                catch (Exception ex1)
                {
                    if (path.Contains("Program Files (x86)")) path = path.Replace("Program Files (x86)", "Program Files");
                    try
                    {
                        bitmap = path.EndsWith(".lnk")
                            ? (Icon.ExtractAssociatedIcon(path) ?? Icon.ExtractAssociatedIcon(GetExePathFromInk(path)))?.ToBitmap()
                            : Icon.ExtractAssociatedIcon(path)?.ToBitmap();
                    }
                    catch (Exception ex2)
                    {
                        try
                        {
                            Icon icon;
                            if (path.EndsWith(".lnk"))
                                icon = GetIcon(path) ?? GetIcon(GetExePathFromInk(path)) ?? GetIconForExtension(path);
                            else icon = GetIcon(path) ?? GetIconForExtension(path);
                            if (icon == null) return null;
                            bitmap = icon.ToBitmap();
                        }
                        catch (Exception ex3)
                        {
                            Console.WriteLine($"Error:\nCause1:\n{ex1}\nCause2:\n{ex2}\nCause3:\n{ex3}\n\n");
                        }
                    }
                }

            }

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        // DLL Import
        [DllImport("shell32")]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        // Constants/Enums
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;

        [Flags]
        private enum ShGetFileInfoConstants
        {
            ShgfiIcon = 0x100,                // get icon
            ShgfiTypename = 0x400,            // get type name
            ShgfiShelliconsize = 0x4,         // get shell size icon
            ShgfiUsefileattributes = 0x10    // use passed dwFileAttribute
        }

        private static Icon GetIconForExtension(string extension)
        {
            // Get the small icon and clone it, as we MUST destroy the handle when we are done.
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr ptr = SHGetFileInfo(
                extension,
                FILE_ATTRIBUTE_NORMAL,
                ref shinfo, (uint)Marshal.SizeOf(shinfo),
                (int)(
                ShGetFileInfoConstants.ShgfiIcon |
                ShGetFileInfoConstants.ShgfiShelliconsize |
                ShGetFileInfoConstants.ShgfiUsefileattributes |
                ShGetFileInfoConstants.ShgfiTypename
                ));
            Icon icon = Icon.FromHandle(shinfo.hIcon).Clone() as Icon;
            DestroyIcon(shinfo.hIcon);
            return icon;
        }



        public static string GetExePathFromInk(string path)
        {
            var shell = new WshShell();
            var shortcut = (WshShortcut)shell.CreateShortcut(path);

            if (File.Exists(shortcut.TargetPath)) return shortcut.TargetPath;
            if (shortcut.TargetPath.Contains("Program Files (x86)") && File.Exists(shortcut.TargetPath.Replace("Program Files (x86)", "Program Files")))
                return shortcut.TargetPath.Replace("Program Files (x86)", "Program Files");
            if (shortcut.TargetPath.Contains("Program Files") && !shortcut.TargetPath.Contains("Program Files (x86)") && File.Exists(shortcut.TargetPath.Replace("Program Files", "Program Files (x86)")))
                return shortcut.TargetPath.Replace("Program Files", "Program Files (x86)");
            return null;
        }

        [DllImport("Shell32.dll", EntryPoint = "SHDefExtractIconW")]
        private static extern int SHDefExtractIconW(
            [MarshalAs(UnmanagedType.LPTStr)] string pszIconFile, int iIndex,
            uint uFlags, ref IntPtr phiconLarge, ref IntPtr phiconSmall, uint nIconSize);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static Icon GetIcon(string fileName)
        {
            const int iconIndex = 0; // the index of the icon in the file (see the msdn documents)

            // Make the nIconSize value (See the Msdn documents). The LOWORD is the Large Icon Size. The HIWORD is the Small Icon Size.
            // The largest size for an icon is 256.
            var largeAndSmallSize = Convert.ToUInt32(Makeiconsize(256, 16));

            var hLrgIcon = IntPtr.Zero;
            var hSmlIcon = IntPtr.Zero;

            var result = SHDefExtractIconW(fileName, iconIndex, 0, ref hLrgIcon, ref hSmlIcon, largeAndSmallSize);

            if (result != 0) return null;

            // if the large and/or small icons where created in the unmanaged memory successfuly then create
            // a clone of them in the managed icons and delete the icons in the unmanaged memory.
            if (hLrgIcon == IntPtr.Zero) return null;
            var largeIcon = (Icon)Icon.FromHandle(hLrgIcon).Clone();
            DestroyIcon(hLrgIcon);

            return largeIcon;
        }

        private static int Makeiconsize(int low, int high)
        {
            return (high << 16) | (low & 0xFFFF);
        }
    }
}
