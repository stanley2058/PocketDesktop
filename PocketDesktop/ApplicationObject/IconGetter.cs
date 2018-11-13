using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PocketDesktop.ApplicationObject
{
    public static class IconGetter
    {
        private static readonly Bitmap FolderIcon = (Bitmap)Properties.Resources.ResourceManager.GetObject("folder_icon");
        public static BitmapImage GetIconBitmapImage(string path)
        {
            //TODO: Fix here, ExtractAssociatedIcon only return 32*32 icon
            path = path.Replace("/", "\\");
            var bitmap = FolderIcon;
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon == null) return null;
                bitmap = icon.ToBitmap();
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
    }
}
